/**
 * @file immutable_array_test.cpp
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
#include <catch2/catch_test_macros.hpp>

import std;
import prism.core;

using namespace prism;

namespace
{
    struct State
    {
        std::size_t allocations = 0;
        std::size_t deallocations = 0;
        std::size_t allocated_count = 0;
        std::size_t deallocated_count = 0;
    };

    template <typename T>
    class CountingAllocator
    {
      public:
        using value_type = T;

        CountingAllocator() : state_{std::make_shared<State>()}
        {
        }

        explicit CountingAllocator(std::shared_ptr<State> state) : state_{std::move(state)}
        {
        }

        template <typename U>
        explicit(false) CountingAllocator(const CountingAllocator<U> &other) noexcept : state_{other.state()}
        {
        }

        [[nodiscard]] T *allocate(const std::size_t count)
        {
            ++state_->allocations;
            state_->allocated_count += count;
            return std::allocator<T>{}.allocate(count);
        }

        [[nodiscard]] std::allocation_result<T *> allocate_at_least(const std::size_t count)
        {
            constexpr auto extra_count = 8uz;

            ++state_->allocations;
            state_->allocated_count += count + extra_count;
            return {std::allocator<T>{}.allocate(count + extra_count), count + extra_count};
        }

        void deallocate(T *ptr, const std::size_t count) noexcept
        {
            ++state_->deallocations;
            state_->deallocated_count += count;
            std::allocator<T>{}.deallocate(ptr, count);
        }

        [[nodiscard]] std::shared_ptr<State> state() const noexcept
        {
            return state_;
        }

        template <typename U>
        [[nodiscard]] friend bool operator==(const CountingAllocator &lhs, const CountingAllocator<U> &rhs) noexcept
        {
            return lhs.state_ == rhs.state();
        }

      private:
        template <typename>
        friend class CountingAllocator;

        std::shared_ptr<State> state_;
    };

    template <typename Range>
    [[nodiscard]] auto to_vector(Range &&range)
    {
        return std::vector<std::ranges::range_value_t<Range>>{std::ranges::begin(range), std::ranges::end(range)};
    }
} // namespace

TEST_CASE("ImmutableArray default construction creates an empty array", "[ImmutableArray]")
{
    constexpr ImmutableArray<int> array;

    CHECK(array.empty());
    CHECK(array.size() == 0);
    CHECK(array.data() == nullptr);
    CHECK(array.begin() == array.end());
    CHECK(array.as_span().empty());
    CHECK(array.use_count() == 0);
}

TEST_CASE("ImmutableArray can be constructed from an initializer list", "[ImmutableArray]")
{
    const ImmutableArray array{1, 2, 3};

    REQUIRE(array.size() == 3);
    CHECK_FALSE(array.empty());
    CHECK(array[0] == 1);
    CHECK(array[1] == 2);
    CHECK(array[2] == 3);
    CHECK(array.front() == 1);
    CHECK(array.back() == 3);
    CHECK(array.use_count() == 1);
}

TEST_CASE("ImmutableArray can be constructed from a range", "[ImmutableArray]")
{
    const std::vector values{1, 2, 3, 4};
    const auto array = make_immutable_array(values);

    REQUIRE(array.size() == values.size());
    CHECK(std::ranges::equal(array, values));
}

TEST_CASE("ImmutableArray exposes contiguous const storage", "[ImmutableArray]")
{
    const ImmutableArray array{10, 20, 30};

    REQUIRE(array.data() != nullptr);
    CHECK(std::to_address(array.begin()) == array.data());
    CHECK(std::to_address(array.end()) == array.data() + array.size());

    const std::span<const int> span = array.as_span();
    CHECK(span.data() == array.data());
    CHECK(span.size() == array.size());
    CHECK(std::ranges::equal(span, std::array{10, 20, 30}));
}

TEST_CASE("ImmutableArray supports forward and reverse iteration", "[ImmutableArray]")
{
    const ImmutableArray array{1, 2, 3};

    CHECK(to_vector(array) == std::vector{1, 2, 3});
    CHECK(to_vector(std::ranges::subrange{array.rbegin(), array.rend()}) == std::vector{3, 2, 1});
}

TEST_CASE("ImmutableArray copy construction is shallow and increments the use count", "[ImmutableArray]")
{
    ImmutableArray original{1, 2, 3};

    {
        const auto copy = original;

        CHECK(original.data() == copy.data());
        CHECK(original.use_count() == 2);
        CHECK(copy.use_count() == 2);
        CHECK(std::ranges::equal(copy, std::array{1, 2, 3}));
    }

    CHECK(original.use_count() == 1);
    CHECK(std::ranges::equal(original, std::array{1, 2, 3}));
}

TEST_CASE("ImmutableArray copy assignment shares the new storage", "[ImmutableArray]")
{
    ImmutableArray lhs{1, 2};
    const ImmutableArray rhs{3, 4, 5};

    lhs = rhs;

    CHECK(lhs.data() == rhs.data());
    CHECK(lhs.use_count() == 2);
    CHECK(rhs.use_count() == 2);
    CHECK(std::ranges::equal(lhs, std::array{3, 4, 5}));
}

TEST_CASE("ImmutableArray move construction transfers the storage", "[ImmutableArray]")
{
    ImmutableArray source{1, 2, 3};
    const auto *const data = source.data();

    const ImmutableArray moved{std::move(source)};

    CHECK(moved.data() == data);
    CHECK(moved.use_count() == 1);
    CHECK(std::ranges::equal(moved, std::array{1, 2, 3}));

    CHECK(source.empty());
    CHECK(source.data() == nullptr);
    CHECK(source.use_count() == 0);
}

TEST_CASE("ImmutableArray move assignment transfers the storage", "[ImmutableArray]")
{
    ImmutableArray source{1, 2, 3};
    ImmutableArray target{4, 5};

    const auto *const data = source.data();

    target = std::move(source);

    CHECK(target.data() == data);
    CHECK(target.use_count() == 1);
    CHECK(std::ranges::equal(target, std::array{1, 2, 3}));

    CHECK(source.empty());
    CHECK(source.data() == nullptr);
    CHECK(source.use_count() == 0);
}

TEST_CASE("ImmutableArray at performs bounds checking", "[ImmutableArray]")
{
    const ImmutableArray array{1, 2, 3};

    CHECK(array.at(0) == 1);
    CHECK(array.at(2) == 3);
    CHECK_THROWS_AS(array.at(3), std::out_of_range);
}

TEST_CASE("ImmutableArray add returns a new array and leaves the original unchanged", "[ImmutableArray]")
{
    const ImmutableArray original{1, 2, 3};

    const auto updated = original.add(4);

    CHECK(std::ranges::equal(original, std::array{1, 2, 3}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3, 4}));
    CHECK(original.data() != updated.data());
    CHECK(original.use_count() == 1);
    CHECK(updated.use_count() == 1);
}

TEST_CASE("ImmutableArray add_range returns a new array", "[ImmutableArray]")
{
    const ImmutableArray original{1, 2};
    const std::array values{3, 4, 5};

    const auto updated = original.add_range(values);

    CHECK(std::ranges::equal(original, std::array{1, 2}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3, 4, 5}));
    CHECK(original.data() != updated.data());
}

TEST_CASE("ImmutableArray add_range of an empty sized range returns a shallow copy", "[ImmutableArray]")
{
    ImmutableArray original{1, 2, 3};
    const std::array<int, 0> values{};

    const auto updated = original.add_range(values);

    CHECK(updated.data() == original.data());
    CHECK(updated.use_count() == 2);
    CHECK(original.use_count() == 2);
}

TEST_CASE("ImmutableArray insert returns a new array and leaves the original unchanged", "[ImmutableArray]")
{
    const ImmutableArray original{1, 3, 4};

    const auto updated = original.insert(1, 2);

    CHECK(std::ranges::equal(original, std::array{1, 3, 4}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3, 4}));
    CHECK(original.data() != updated.data());
}

TEST_CASE("ImmutableArray insert accepts inserting at the end", "[ImmutableArray]")
{
    const ImmutableArray original{1, 2, 3};

    const auto updated = original.insert(original.size(), 4);

    CHECK(std::ranges::equal(original, std::array{1, 2, 3}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3, 4}));
}

TEST_CASE("ImmutableArray insert rejects indexes past the end", "[ImmutableArray]")
{
    const ImmutableArray array{1, 2, 3};

    CHECK_THROWS_AS(array.insert(4, 10), std::out_of_range);
}

TEST_CASE("ImmutableArray insert_range returns a new array", "[ImmutableArray]")
{
    const ImmutableArray original{1, 5};
    const std::array values{2, 3, 4};

    const auto updated = original.insert_range(1, values);

    CHECK(std::ranges::equal(original, std::array{1, 5}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3, 4, 5}));
    CHECK(original.data() != updated.data());
}

TEST_CASE("ImmutableArray insert_range accepts inserting at the end", "[ImmutableArray]")
{
    const ImmutableArray original{1, 2};
    const std::array values{3, 4};

    const auto updated = original.insert_range(original.size(), values);

    CHECK(std::ranges::equal(original, std::array{1, 2}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3, 4}));
}

TEST_CASE("ImmutableArray set returns a new array and leaves the original unchanged", "[ImmutableArray]")
{
    const ImmutableArray original{1, 2, 3};

    const auto updated = original.set(1, 20);

    CHECK(std::ranges::equal(original, std::array{1, 2, 3}));
    CHECK(std::ranges::equal(updated, std::array{1, 20, 3}));
    CHECK(original.data() != updated.data());
}

TEST_CASE("ImmutableArray set rejects out-of-range indexes", "[ImmutableArray]")
{
    const ImmutableArray array{1, 2, 3};

    CHECK_THROWS_AS(array.set(3, 10), std::out_of_range);
}

TEST_CASE("ImmutableArray remove_at returns a new array", "[ImmutableArray]")
{
    const ImmutableArray original{1, 2, 3, 4};

    const auto updated = original.remove_at(1);

    CHECK(std::ranges::equal(original, std::array{1, 2, 3, 4}));
    CHECK(std::ranges::equal(updated, std::array{1, 3, 4}));
    CHECK(original.data() != updated.data());
}

TEST_CASE("ImmutableArray remove_at of the only element returns an empty array", "[ImmutableArray]")
{
    const ImmutableArray original{1};

    const auto updated = original.remove_at(0);

    CHECK(std::ranges::equal(original, std::array{1}));
    CHECK(updated.empty());
    CHECK(updated.data() == nullptr);
}

TEST_CASE("ImmutableArray remove_at rejects out-of-range indexes", "[ImmutableArray]")
{
    const ImmutableArray array{1, 2, 3};

    CHECK_THROWS_AS(array.remove_at(3), std::out_of_range);
}

TEST_CASE("ImmutableArray clear returns an empty array and leaves the original unchanged", "[ImmutableArray]")
{
    const ImmutableArray original{1, 2, 3};

    const auto cleared = original.clear();

    CHECK(std::ranges::equal(original, std::array{1, 2, 3}));
    CHECK(cleared.empty());
    CHECK(cleared.data() == nullptr);
    CHECK(cleared.use_count() == 0);
}

TEST_CASE("ImmutableArray equality compares values", "[ImmutableArray]")
{
    const ImmutableArray first{1, 2, 3};
    const ImmutableArray second{1, 2, 3};
    const ImmutableArray different{1, 2, 4};

    CHECK(first == second);
    CHECK_FALSE(first == different);
}

TEST_CASE("ImmutableArray ordering is lexicographical", "[ImmutableArray]")
{
    const ImmutableArray first{1, 2, 3};
    const ImmutableArray second{1, 2, 4};
    const ImmutableArray shorter{1, 2};

    CHECK(first < second);
    CHECK(shorter < first);
    CHECK((first <=> ImmutableArray{1, 2, 3}) == std::strong_ordering::equal);
}

TEST_CASE("ImmutableArray uses allocate_at_least deallocation count", "[ImmutableArray]")
{
    using Allocator = CountingAllocator<int>;
    using Array = ImmutableArray<int, Allocator>;

    const auto allocator = Allocator{};
    const auto state = allocator.state();

    {
        const Array array{{1, 2, 3}, allocator};

        CHECK(state->allocations == 1);
        CHECK(state->deallocations == 0);
        CHECK(state->allocated_count > 0);
    }

    CHECK(state->deallocations == 1);
    CHECK(state->deallocated_count == state->allocated_count);
}
