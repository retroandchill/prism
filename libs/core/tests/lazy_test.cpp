/**
 * @file lazy_test.cpp
 * @author Francesco Corso
 * @date 8/12/2026
 * @brief
 */
module;

#include <catch2/catch_test_macros.hpp>

module prism.core:tests.lazy;

import :util.lazy;

using namespace prism;

namespace
{
    struct LifetimeCounter
    {
        inline static std::atomic<int> constructions = 0;
        inline static std::atomic<int> destructions = 0;

        int value = 0;

        explicit LifetimeCounter(const int value) : value{value}
        {
            ++constructions;
        }

        LifetimeCounter(const LifetimeCounter &other) : value{other.value}
        {
            ++constructions;
        }

        LifetimeCounter(LifetimeCounter &&other) noexcept : value{other.value}
        {
            ++constructions;
        }

        ~LifetimeCounter()
        {
            ++destructions;
        }

        static void reset_counts() noexcept
        {
            constructions.store(0, std::memory_order::relaxed);
            destructions.store(0, std::memory_order::relaxed);
        }
    };
} // namespace

TEST_CASE("Lazy value starts uninitialized", "[lazy]")
{
    Lazy<int> lazy;

    CHECK(lazy.state() == LazyState::uninitialized);
    CHECK_FALSE(lazy.has_value());
    CHECK_THROWS_AS(static_cast<void>(lazy.value()), InvalidStateException);
}

TEST_CASE("Lazy value computes once and reuses the stored value", "[lazy]")
{
    Lazy<int> lazy;
    int calls = 0;

    const auto &first = lazy.get_or_compute(
        [&]
        {
            ++calls;
            return 42;
        });

    const auto &second = lazy.get_or_compute(
        [&]
        {
            ++calls;
            return 100;
        });

    CHECK(first == 42);
    CHECK(second == 42);
    CHECK(std::addressof(first) == std::addressof(second));
    CHECK(calls == 1);
    CHECK(lazy.state() == LazyState::computed);
    CHECK(lazy.has_value());
}

TEST_CASE("Lazy value resets and can be recomputed", "[lazy]")
{
    Lazy<int> lazy;
    int calls = 0;

    CHECK(lazy.get_or_compute(
              [&]
              {
                  ++calls;
                  return 1;
              }) == 1);

    lazy.reset();

    CHECK(lazy.state() == LazyState::uninitialized);
    CHECK_FALSE(lazy.has_value());

    CHECK(lazy.get_or_compute(
              [&]
              {
                  ++calls;
                  return 2;
              }) == 2);

    CHECK(calls == 2);
    CHECK(lazy.state() == LazyState::computed);
}

TEST_CASE("Lazy value destroys non-trivial value on reset", "[lazy]")
{
    LifetimeCounter::reset_counts();

    {
        Lazy<LifetimeCounter> lazy;

        const auto &value = lazy.get_or_compute([] { return LifetimeCounter{123}; });

        CHECK(value.value == 123);
        CHECK(LifetimeCounter::constructions.load(std::memory_order::relaxed) >= 1);
        CHECK(LifetimeCounter::destructions.load(std::memory_order::relaxed) <
              LifetimeCounter::constructions.load(std::memory_order::relaxed));

        lazy.reset();

        CHECK(lazy.state() == LazyState::uninitialized);
        CHECK(LifetimeCounter::destructions.load(std::memory_order::relaxed) ==
              LifetimeCounter::constructions.load(std::memory_order::relaxed));
    }

    CHECK(LifetimeCounter::destructions.load(std::memory_order::relaxed) ==
          LifetimeCounter::constructions.load(std::memory_order::relaxed));
}

TEST_CASE("Lazy value destroys non-trivial value in destructor", "[lazy]")
{
    LifetimeCounter::reset_counts();

    {
        Lazy<LifetimeCounter> lazy;

        const auto &value = lazy.get_or_compute([] { return LifetimeCounter{456}; });

        CHECK(value.value == 456);
        CHECK(LifetimeCounter::constructions.load(std::memory_order::relaxed) >= 1);
    }

    CHECK(LifetimeCounter::destructions.load(std::memory_order::relaxed) ==
          LifetimeCounter::constructions.load(std::memory_order::relaxed));
}

TEST_CASE("Lazy value resets after evaluator throws", "[lazy]")
{
    Lazy<int> lazy;
    int calls = 0;

    CHECK_THROWS_AS(lazy.get_or_compute(
                        [&]() -> int
                        {
                            ++calls;
                            throw std::runtime_error{"boom"};
                        }),
                    std::runtime_error);

    CHECK(calls == 1);
    CHECK(lazy.state() == LazyState::uninitialized);
    CHECK_FALSE(lazy.has_value());

    CHECK(lazy.get_or_compute(
              [&]
              {
                  ++calls;
                  return 77;
              }) == 77);

    CHECK(calls == 2);
    CHECK(lazy.state() == LazyState::computed);
}

TEST_CASE("Lazy reference starts uninitialized", "[lazy]")
{
    Lazy<int &> lazy;

    CHECK(lazy.state() == LazyState::uninitialized);
    CHECK_FALSE(lazy.has_value());
    CHECK_FALSE(lazy.wait_if_computing().has_value());
    CHECK_THROWS_AS(static_cast<void>(lazy.value()), InvalidStateException);
}

TEST_CASE("Lazy reference stores and returns the referenced object", "[lazy]")
{
    int value = 10;
    Lazy<int &> lazy;

    lazy.set(value);

    CHECK(lazy.state() == LazyState::computed);
    CHECK(lazy.has_value());
    CHECK(std::addressof(lazy.value()) == std::addressof(value));

    lazy.value() = 20;

    CHECK(value == 20);
}

TEST_CASE("Lazy reference computes once and reuses the referenced object", "[lazy]")
{
    int first_value = 1;
    int second_value = 2;
    int calls = 0;
    Lazy<int &> lazy;

    auto &first = lazy.get_or_compute(
        [&]() -> int &
        {
            ++calls;
            return first_value;
        });

    auto &second = lazy.get_or_compute(
        [&]() -> int &
        {
            ++calls;
            return second_value;
        });

    CHECK(std::addressof(first) == std::addressof(first_value));
    CHECK(std::addressof(second) == std::addressof(first_value));
    CHECK(calls == 1);
}

TEST_CASE("Lazy value evaluator runs once under concurrent access", "[lazy]")
{
    Lazy<int> lazy;
    std::atomic<int> calls = 0;

    constexpr int thread_count = 16;
    std::vector<std::jthread> threads;
    std::vector<const int *> results(thread_count, nullptr);

    for (int i = 0; i < thread_count; ++i)
    {
        threads.emplace_back(
            [&, i]
            {
                const auto &value = lazy.get_or_compute(
                    [&]
                    {
                        calls.fetch_add(1, std::memory_order::relaxed);
                        std::this_thread::sleep_for(std::chrono::milliseconds{10});
                        return 999;
                    });

                results[i] = std::addressof(value);
            });
    }

    threads.clear();

    CHECK(calls.load(std::memory_order::relaxed) == 1);
    CHECK(lazy.has_value());
    CHECK(lazy.value() == 999);

    for (const auto *result : results)
    {
        REQUIRE(result != nullptr);
        CHECK(result == std::addressof(lazy.value()));
    }
}

TEST_CASE("Lazy reference evaluator runs once under concurrent access", "[lazy]")
{
    int value = 123;
    Lazy<int &> lazy;
    std::atomic<int> calls = 0;

    constexpr int thread_count = 16;
    std::vector<std::jthread> threads;
    std::vector<int *> results(thread_count, nullptr);

    for (int i = 0; i < thread_count; ++i)
    {
        threads.emplace_back(
            [&, i]
            {
                auto &result = lazy.get_or_compute(
                    [&]() -> int &
                    {
                        calls.fetch_add(1, std::memory_order::relaxed);
                        std::this_thread::sleep_for(std::chrono::milliseconds{10});
                        return value;
                    });

                results[i] = std::addressof(result);
            });
    }

    threads.clear();

    CHECK(calls.load(std::memory_order::relaxed) == 1);
    CHECK(lazy.has_value());

    for (const auto *result : results)
    {
        CHECK(result == std::addressof(value));
    }
}
