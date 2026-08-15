/**
 * @file immutable_small_array_test.cpp
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
    template <typename Range>
    [[nodiscard]] auto to_vector(Range &&range)
    {
        return std::vector<std::ranges::range_value_t<Range>>{std::ranges::begin(range), std::ranges::end(range)};
    }
} // namespace

TEST_CASE("ImmutableSmallArray default construction creates an empty inline array", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> array;

    CHECK(array.empty());
    CHECK(array.size() == 0);
    CHECK(array.begin() == array.end());
    CHECK(array.as_span().empty());
    CHECK(array.use_count() == 0);
    CHECK(array.is_inline());
    CHECK_FALSE(array.is_heap_allocated());
}

TEST_CASE("ImmutableSmallArray stores initializer lists within inline capacity inline", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> array{1, 2, 3, 4};

    REQUIRE(array.size() == 4);
    CHECK_FALSE(array.empty());
    CHECK(array[0] == 1);
    CHECK(array[1] == 2);
    CHECK(array[2] == 3);
    CHECK(array[3] == 4);
    CHECK(array.front() == 1);
    CHECK(array.back() == 4);
    CHECK(array.use_count() == 1);
    CHECK(array.is_inline());
    CHECK_FALSE(array.is_heap_allocated());
}

TEST_CASE("ImmutableSmallArray stores initializer lists larger than inline capacity", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> array{1, 2, 3, 4, 5};

    REQUIRE(array.size() == 5);
    CHECK(std::ranges::equal(array, std::array{1, 2, 3, 4, 5}));
    CHECK(array.use_count() == 1);
    CHECK_FALSE(array.is_inline());
    CHECK(array.is_heap_allocated());
}

TEST_CASE("ImmutableSmallArray can be constructed from a range inline", "[ImmutableSmallArray]")
{
    const std::array values{1, 2, 3};
    const auto array = prism::make_immutable_small_array<4>(values);

    REQUIRE(array.size() == values.size());
    CHECK(std::ranges::equal(array, values));
    CHECK(array.is_inline());
}

TEST_CASE("ImmutableSmallArray can be constructed from a range on the heap", "[ImmutableSmallArray]")
{
    const std::array values{1, 2, 3, 4, 5};
    const auto array = prism::make_immutable_small_array<4>(values);

    REQUIRE(array.size() == values.size());
    CHECK(std::ranges::equal(array, values));
    CHECK(array.is_heap_allocated());
}

TEST_CASE("ImmutableSmallArray exposes contiguous const storage", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> array{10, 20, 30};

    REQUIRE(array.data() != nullptr);
    CHECK(std::to_address(array.begin()) == array.data());
    CHECK(std::to_address(array.end()) == array.data() + array.size());

    const std::span<const int> span = array.as_span();
    CHECK(span.data() == array.data());
    CHECK(span.size() == array.size());
    CHECK(std::ranges::equal(span, std::array{10, 20, 30}));
}

TEST_CASE("ImmutableSmallArray exposes contiguous const heap storage", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 2> array{10, 20, 30};

    REQUIRE(array.data() != nullptr);
    CHECK(std::to_address(array.begin()) == array.data());
    CHECK(std::to_address(array.end()) == array.data() + array.size());

    const std::span<const int> span = array.as_span();
    CHECK(span.data() == array.data());
    CHECK(span.size() == array.size());
    CHECK(std::ranges::equal(span, std::array{10, 20, 30}));
    CHECK(array.is_heap_allocated());
}

TEST_CASE("ImmutableSmallArray supports forward and reverse iteration", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> array{1, 2, 3};

    CHECK(to_vector(array) == std::vector{1, 2, 3});
    CHECK(to_vector(std::ranges::subrange{array.rbegin(), array.rend()}) == std::vector{3, 2, 1});
}

TEST_CASE("ImmutableSmallArray inline copy construction copies inline values", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> original{1, 2, 3};

    const auto copy = original;

    CHECK(original.data() != copy.data());
    CHECK(original.use_count() == 1);
    CHECK(copy.use_count() == 1);
    CHECK(original.is_inline());
    CHECK(copy.is_inline());
    CHECK(std::ranges::equal(original, std::array{1, 2, 3}));
    CHECK(std::ranges::equal(copy, std::array{1, 2, 3}));
}

TEST_CASE("ImmutableSmallArray heap copy construction shallow-copies heap storage", "[ImmutableSmallArray]")
{
    ImmutableSmallArray<int, 4> original{1, 2, 3, 4, 5};

    {
        const auto copy = original;

        CHECK(original.data() == copy.data());
        CHECK(original.use_count() == 2);
        CHECK(copy.use_count() == 2);
        CHECK(original.is_heap_allocated());
        CHECK(copy.is_heap_allocated());
        CHECK(std::ranges::equal(copy, std::array{1, 2, 3, 4, 5}));
    }

    CHECK(original.use_count() == 1);
    CHECK(std::ranges::equal(original, std::array{1, 2, 3, 4, 5}));
}

TEST_CASE("ImmutableSmallArray inline copy assignment copies inline values", "[ImmutableSmallArray]")
{
    ImmutableSmallArray<int, 4> lhs{9, 8, 7};
    const ImmutableSmallArray<int, 4> rhs{1, 2, 3};

    lhs = rhs;

    CHECK(lhs.data() != rhs.data());
    CHECK(lhs.use_count() == 1);
    CHECK(rhs.use_count() == 1);
    CHECK(lhs.is_inline());
    CHECK(rhs.is_inline());
    CHECK(std::ranges::equal(lhs, std::array{1, 2, 3}));
}

TEST_CASE("ImmutableSmallArray heap copy assignment shares heap storage", "[ImmutableSmallArray]")
{
    ImmutableSmallArray<int, 4> lhs{9, 8};
    const ImmutableSmallArray<int, 4> rhs{1, 2, 3, 4, 5};

    lhs = rhs;

    CHECK(lhs.data() == rhs.data());
    CHECK(lhs.use_count() == 2);
    CHECK(rhs.use_count() == 2);
    CHECK(lhs.is_heap_allocated());
    CHECK(rhs.is_heap_allocated());
    CHECK(std::ranges::equal(lhs, std::array{1, 2, 3, 4, 5}));
}

TEST_CASE("ImmutableSmallArray move construction transfers inline values", "[ImmutableSmallArray]")
{
    ImmutableSmallArray<int, 4> source{1, 2, 3};

    const ImmutableSmallArray<int, 4> moved{std::move(source)};

    CHECK(std::ranges::equal(moved, std::array{1, 2, 3}));
    CHECK(moved.is_inline());
    CHECK(moved.use_count() == 1);

    CHECK(source.empty());
    CHECK(source.use_count() == 0);
    CHECK(source.is_inline());
}

TEST_CASE("ImmutableSmallArray move construction transfers heap storage", "[ImmutableSmallArray]")
{
    ImmutableSmallArray<int, 4> source{1, 2, 3, 4, 5};
    const auto *const data = source.data();

    const ImmutableSmallArray<int, 4> moved{std::move(source)};

    CHECK(moved.data() == data);
    CHECK(moved.use_count() == 1);
    CHECK(moved.is_heap_allocated());
    CHECK(std::ranges::equal(moved, std::array{1, 2, 3, 4, 5}));

    CHECK(source.empty());
    CHECK(source.use_count() == 0);
    CHECK(source.is_inline());
}

TEST_CASE("ImmutableSmallArray move assignment transfers inline values", "[ImmutableSmallArray]")
{
    ImmutableSmallArray<int, 4> source{1, 2, 3};
    ImmutableSmallArray<int, 4> target{9, 8, 7, 6, 5};

    target = std::move(source);

    CHECK(std::ranges::equal(target, std::array{1, 2, 3}));
    CHECK(target.is_inline());
    CHECK(target.use_count() == 1);

    CHECK(source.empty());
    CHECK(source.use_count() == 0);
    CHECK(source.is_inline());
}

TEST_CASE("ImmutableSmallArray move assignment transfers heap storage", "[ImmutableSmallArray]")
{
    ImmutableSmallArray<int, 4> source{1, 2, 3, 4, 5};
    ImmutableSmallArray<int, 4> target{9, 8};

    const auto *const data = source.data();

    target = std::move(source);

    CHECK(target.data() == data);
    CHECK(target.use_count() == 1);
    CHECK(target.is_heap_allocated());
    CHECK(std::ranges::equal(target, std::array{1, 2, 3, 4, 5}));

    CHECK(source.empty());
    CHECK(source.use_count() == 0);
    CHECK(source.is_inline());
}

TEST_CASE("ImmutableSmallArray at performs bounds checking", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> array{1, 2, 3};

    CHECK(array.at(0) == 1);
    CHECK(array.at(2) == 3);
    CHECK_THROWS_AS(array.at(3), std::out_of_range);
}

TEST_CASE("ImmutableSmallArray add returns an inline array when result fits", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> original{1, 2};

    const auto updated = original.add(3);

    CHECK(std::ranges::equal(original, std::array{1, 2}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3}));
    CHECK(original.is_inline());
    CHECK(updated.is_inline());
    CHECK(original.data() != updated.data());
}

TEST_CASE("ImmutableSmallArray add transitions from inline to heap", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 3> original{1, 2, 3};

    const auto updated = original.add(4);

    CHECK(std::ranges::equal(original, std::array{1, 2, 3}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3, 4}));
    CHECK(original.is_inline());
    CHECK(updated.is_heap_allocated());
}

TEST_CASE("ImmutableSmallArray add preserves heap storage category", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 3> original{1, 2, 3, 4};

    const auto updated = original.add(5);

    CHECK(std::ranges::equal(original, std::array{1, 2, 3, 4}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3, 4, 5}));
    CHECK(original.is_heap_allocated());
    CHECK(updated.is_heap_allocated());
    CHECK(original.data() != updated.data());
}

TEST_CASE("ImmutableSmallArray add_range returns a new inline array when result fits", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 5> original{1, 2};
    const std::array values{3, 4};

    const auto updated = original.add_range(values);

    CHECK(std::ranges::equal(original, std::array{1, 2}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3, 4}));
    CHECK(updated.is_inline());
}

TEST_CASE("ImmutableSmallArray add_range transitions from inline to heap when needed", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 3> original{1, 2};
    const std::array values{3, 4};

    const auto updated = original.add_range(values);

    CHECK(std::ranges::equal(original, std::array{1, 2}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3, 4}));
    CHECK(original.is_inline());
    CHECK(updated.is_heap_allocated());
}

TEST_CASE("ImmutableSmallArray add_range of an empty sized range returns a copy", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> original{1, 2, 3};
    const std::array<int, 0> values{};

    const auto updated = original.add_range(values);

    CHECK(std::ranges::equal(updated, original));
    CHECK(updated.is_inline());
    CHECK(updated.data() != original.data());
}

TEST_CASE("ImmutableSmallArray insert returns a new inline array", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> original{1, 3};

    const auto updated = original.insert(1, 2);

    CHECK(std::ranges::equal(original, std::array{1, 3}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3}));
    CHECK(updated.is_inline());
}

TEST_CASE("ImmutableSmallArray insert accepts inserting at the end", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> original{1, 2, 3};

    const auto updated = original.insert(original.size(), 4);

    CHECK(std::ranges::equal(original, std::array{1, 2, 3}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3, 4}));
    CHECK(updated.is_inline());
}

TEST_CASE("ImmutableSmallArray insert transitions from inline to heap when needed", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 3> original{1, 2, 4};

    const auto updated = original.insert(2, 3);

    CHECK(std::ranges::equal(original, std::array{1, 2, 4}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3, 4}));
    CHECK(original.is_inline());
    CHECK(updated.is_heap_allocated());
}

TEST_CASE("ImmutableSmallArray insert rejects indexes past the end", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> array{1, 2, 3};

    CHECK_THROWS_AS(array.insert(4, 10), std::out_of_range);
}

TEST_CASE("ImmutableSmallArray insert_range returns a new inline array", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 5> original{1, 5};
    const std::array values{2, 3, 4};

    const auto updated = original.insert_range(1, values);

    CHECK(std::ranges::equal(original, std::array{1, 5}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3, 4, 5}));
    CHECK(updated.is_inline());
}

TEST_CASE("ImmutableSmallArray insert_range accepts inserting at the end", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> original{1, 2};
    const std::array values{3, 4};

    const auto updated = original.insert_range(original.size(), values);

    CHECK(std::ranges::equal(original, std::array{1, 2}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3, 4}));
    CHECK(updated.is_inline());
}

TEST_CASE("ImmutableSmallArray insert_range transitions from inline to heap when needed", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 3> original{1, 4};
    const std::array values{2, 3};

    const auto updated = original.insert_range(1, values);

    CHECK(std::ranges::equal(original, std::array{1, 4}));
    CHECK(std::ranges::equal(updated, std::array{1, 2, 3, 4}));
    CHECK(original.is_inline());
    CHECK(updated.is_heap_allocated());
}

TEST_CASE("ImmutableSmallArray set returns a new inline array", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> original{1, 2, 3};

    const auto updated = original.set(1, 20);

    CHECK(std::ranges::equal(original, std::array{1, 2, 3}));
    CHECK(std::ranges::equal(updated, std::array{1, 20, 3}));
    CHECK(original.data() != updated.data());
    CHECK(original.is_inline());
    CHECK(updated.is_inline());
}

TEST_CASE("ImmutableSmallArray set returns a new heap array when source is heap", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 3> original{1, 2, 3, 4};

    const auto updated = original.set(1, 20);

    CHECK(std::ranges::equal(original, std::array{1, 2, 3, 4}));
    CHECK(std::ranges::equal(updated, std::array{1, 20, 3, 4}));
    CHECK(original.data() != updated.data());
    CHECK(original.is_heap_allocated());
    CHECK(updated.is_heap_allocated());
}

TEST_CASE("ImmutableSmallArray set rejects out-of-range indexes", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> array{1, 2, 3};

    CHECK_THROWS_AS(array.set(3, 10), std::out_of_range);
}

TEST_CASE("ImmutableSmallArray remove_at returns a new inline array", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> original{1, 2, 3, 4};

    const auto updated = original.remove_at(1);

    CHECK(std::ranges::equal(original, std::array{1, 2, 3, 4}));
    CHECK(std::ranges::equal(updated, std::array{1, 3, 4}));
    CHECK(original.data() != updated.data());
    CHECK(updated.is_inline());
}

TEST_CASE("ImmutableSmallArray remove_at can transition from heap to inline", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 3> original{1, 2, 3, 4};

    const auto updated = original.remove_at(1);

    CHECK(std::ranges::equal(original, std::array{1, 2, 3, 4}));
    CHECK(std::ranges::equal(updated, std::array{1, 3, 4}));
    CHECK(original.is_heap_allocated());
    CHECK(updated.is_inline());
}

TEST_CASE("ImmutableSmallArray remove_at of the only element returns an empty array", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> original{1};

    const auto updated = original.remove_at(0);

    CHECK(std::ranges::equal(original, std::array{1}));
    CHECK(updated.empty());
    CHECK(updated.use_count() == 0);
    CHECK(updated.is_inline());
}

TEST_CASE("ImmutableSmallArray remove_at rejects out-of-range indexes", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> array{1, 2, 3};

    CHECK_THROWS_AS(array.remove_at(3), std::out_of_range);
}

TEST_CASE("ImmutableSmallArray clear returns an empty inline array", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> original{1, 2, 3};

    const auto cleared = original.clear();

    CHECK(std::ranges::equal(original, std::array{1, 2, 3}));
    CHECK(cleared.empty());
    CHECK(cleared.use_count() == 0);
    CHECK(cleared.is_inline());
}

TEST_CASE("ImmutableSmallArray equality compares values across inline and heap storage", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> inline_array{1, 2, 3};
    const ImmutableSmallArray<int, 2> heap_array{1, 2, 3};
    const ImmutableSmallArray<int, 4> different{1, 2, 4};

    CHECK(inline_array == heap_array);
    CHECK_FALSE(inline_array == different);
}

TEST_CASE("ImmutableSmallArray ordering is lexicographical", "[ImmutableSmallArray]")
{
    const ImmutableSmallArray<int, 4> first{1, 2, 3};
    const ImmutableSmallArray<int, 4> second{1, 2, 4};
    const ImmutableSmallArray<int, 4> shorter{1, 2};

    CHECK(first < second);
    CHECK(shorter < first);
    CHECK((first <=> ImmutableSmallArray<int, 4>{1, 2, 3}) == std::strong_ordering::equal);
}

TEST_CASE("ImmutableSmallArray swap exchanges inline arrays", "[ImmutableSmallArray]")
{
    ImmutableSmallArray<int, 4> first{1, 2};
    ImmutableSmallArray<int, 4> second{3, 4};

    swap(first, second);

    CHECK(std::ranges::equal(first, std::array{3, 4}));
    CHECK(std::ranges::equal(second, std::array{1, 2}));
    CHECK(first.is_inline());
    CHECK(second.is_inline());
}

TEST_CASE("ImmutableSmallArray swap exchanges inline and heap arrays", "[ImmutableSmallArray]")
{
    ImmutableSmallArray<int, 3> first{1, 2};
    ImmutableSmallArray<int, 3> second{3, 4, 5, 6};

    swap(first, second);

    CHECK(std::ranges::equal(first, std::array{3, 4, 5, 6}));
    CHECK(std::ranges::equal(second, std::array{1, 2}));
    CHECK(first.is_heap_allocated());
    CHECK(second.is_inline());
}

TEST_CASE("ImmutableSmallArray swap exchanges heap arrays", "[ImmutableSmallArray]")
{
    ImmutableSmallArray<int, 2> first{1, 2, 3};
    ImmutableSmallArray<int, 2> second{4, 5, 6};

    const auto *const first_data = first.data();
    const auto *const second_data = second.data();

    swap(first, second);

    CHECK(first.data() == second_data);
    CHECK(second.data() == first_data);
    CHECK(std::ranges::equal(first, std::array{4, 5, 6}));
    CHECK(std::ranges::equal(second, std::array{1, 2, 3}));
    CHECK(first.is_heap_allocated());
    CHECK(second.is_heap_allocated());
}
