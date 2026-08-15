/**
 * @file immutable_hash_collections_test.cpp
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
    struct ConstantHash
    {
        using is_transparent = void;

        template <typename T>
        [[nodiscard]] constexpr std::size_t operator()(const T &) const noexcept
        {
            return 0;
        }
    };

    struct TransparentStringHash
    {
        using is_transparent = void;

        [[nodiscard]] std::size_t operator()(const std::string &value) const noexcept
        {
            return std::hash<std::string_view>{}(value);
        }

        [[nodiscard]] std::size_t operator()(std::string_view value) const noexcept
        {
            return std::hash<std::string_view>{}(value);
        }

        [[nodiscard]] std::size_t operator()(const char *value) const noexcept
        {
            return std::hash<std::string_view>{}(value);
        }
    };

    struct TransparentStringEqual
    {
        using is_transparent = void;

        [[nodiscard]] bool operator()(std::string_view lhs, std::string_view rhs) const noexcept
        {
            return lhs == rhs;
        }

        [[nodiscard]] bool operator()(const std::string &lhs, const std::string &rhs) const noexcept
        {
            return lhs == rhs;
        }

        [[nodiscard]] bool operator()(const std::string &lhs, std::string_view rhs) const noexcept
        {
            return lhs == rhs;
        }

        [[nodiscard]] bool operator()(std::string_view lhs, const std::string &rhs) const noexcept
        {
            return lhs == rhs;
        }

        [[nodiscard]] bool operator()(const std::string &lhs, const char *rhs) const noexcept
        {
            return lhs == rhs;
        }

        [[nodiscard]] bool operator()(const char *lhs, const std::string &rhs) const noexcept
        {
            return lhs == rhs;
        }
    };

    template <typename Range>
    [[nodiscard]] auto to_sorted_vector(Range &&range)
    {
        std::vector<std::ranges::range_value_t<Range>> result{std::ranges::begin(range), std::ranges::end(range)};
        std::ranges::sort(result);
        return result;
    }
} // namespace

static_assert(std::forward_iterator<ImmutableHashSet<int>::const_iterator>);
static_assert(std::forward_iterator<ImmutableHashMap<int, int>::const_iterator>);
static_assert(std::ranges::forward_range<ImmutableHashSet<int>>);
static_assert(std::ranges::forward_range<ImmutableHashMap<int, int>>);

TEST_CASE("ImmutableHashSet default construction creates an empty set", "[ImmutableHashSet]")
{
    const ImmutableHashSet<int> set;

    CHECK(set.empty());
    CHECK(set.size() == 0);
    CHECK_FALSE(set.contains(1));
    CHECK(set.begin() == set.end());
}

TEST_CASE("ImmutableHashSet add returns a new set and leaves the original unchanged", "[ImmutableHashSet]")
{
    const ImmutableHashSet<int> original;

    const auto one = original.add(1);
    const auto two = one.add(2);

    CHECK(original.empty());
    CHECK_FALSE(original.contains(1));

    CHECK(one.size() == 1);
    CHECK(one.contains(1));
    CHECK_FALSE(one.contains(2));

    CHECK(two.size() == 2);
    CHECK(two.contains(1));
    CHECK(two.contains(2));
}

TEST_CASE("ImmutableHashSet adding an existing value is a no-op", "[ImmutableHashSet]")
{
    const auto set = ImmutableHashSet<int>{}.add(1).add(2);

    const auto same = set.add(1);

    CHECK(same.size() == 2);
    CHECK(same.contains(1));
    CHECK(same.contains(2));

    // If same_storage is exposed, this should be true after no-op set refinement.
    CHECK(same.same_storage(set));
}

TEST_CASE("ImmutableHashSet remove returns a new set and leaves the original unchanged", "[ImmutableHashSet]")
{
    const auto original = ImmutableHashSet<int>{}.add(1).add(2).add(3);

    const auto removed = original.remove(2);

    CHECK(original.size() == 3);
    CHECK(original.contains(1));
    CHECK(original.contains(2));
    CHECK(original.contains(3));

    CHECK(removed.size() == 2);
    CHECK(removed.contains(1));
    CHECK_FALSE(removed.contains(2));
    CHECK(removed.contains(3));
}

TEST_CASE("ImmutableHashSet removing a missing value is a no-op", "[ImmutableHashSet]")
{
    const auto set = ImmutableHashSet<int>{}.add(1).add(2);

    const auto same = set.remove(3);

    CHECK(same.size() == 2);
    CHECK(same.contains(1));
    CHECK(same.contains(2));
    CHECK(same.same_storage(set));
}

TEST_CASE("ImmutableHashSet iteration visits all values", "[ImmutableHashSet]")
{
    const auto set = ImmutableHashSet<int>{}.add(3).add(1).add(2);

    CHECK(to_sorted_vector(set) == std::vector{1, 2, 3});
}

TEST_CASE("ImmutableHashSet handles full hash collisions", "[ImmutableHashSet]")
{
    using Set = ImmutableHashSet<int, ConstantHash>;

    const auto set = Set{}.add(1).add(2).add(3).add(4);

    CHECK(set.size() == 4);
    CHECK(set.contains(1));
    CHECK(set.contains(2));
    CHECK(set.contains(3));
    CHECK(set.contains(4));

    const auto removed = set.remove(2);

    CHECK(removed.size() == 3);
    CHECK(removed.contains(1));
    CHECK_FALSE(removed.contains(2));
    CHECK(removed.contains(3));
    CHECK(removed.contains(4));
}

TEST_CASE("ImmutableHashSet supports alternative lookup", "[ImmutableHashSet]")
{
    using Set = ImmutableHashSet<std::string, TransparentStringHash, TransparentStringEqual>;

    const auto set = Set{}.add(std::string{"alpha"}).add(std::string{"beta"});

    CHECK(set.contains(std::string_view{"alpha"}));
    CHECK(set.contains("beta"));
    CHECK_FALSE(set.contains(std::string_view{"gamma"}));

    const auto removed = set.remove(std::string_view{"alpha"});

    CHECK_FALSE(removed.contains("alpha"));
    CHECK(removed.contains("beta"));
}

TEST_CASE("ImmutableHashSet clear returns an empty set", "[ImmutableHashSet]")
{
    const auto set = ImmutableHashSet<int>{}.add(1).add(2);

    const auto cleared = set.clear();

    CHECK(cleared.empty());
    CHECK(cleared.size() == 0);
    CHECK_FALSE(cleared.contains(1));

    CHECK(set.size() == 2);
}

TEST_CASE("ImmutableHashMap default construction creates an empty map", "[ImmutableHashMap]")
{
    const ImmutableHashMap<int, std::string> map;

    CHECK(map.empty());
    CHECK(map.size() == 0);
    CHECK_FALSE(map.contains(1));
    CHECK_FALSE(map.try_get(1).has_value());
    CHECK(map.begin() == map.end());
}

TEST_CASE("ImmutableHashMap set adds values and leaves the original unchanged", "[ImmutableHashMap]")
{
    const ImmutableHashMap<int, std::string> original;

    const auto one = original.set(1, "one");
    const auto two = one.set(2, "two");

    CHECK(original.empty());
    CHECK_FALSE(original.contains(1));

    REQUIRE(one.try_get(1).has_value());
    CHECK(one.get(1) == "one");
    CHECK_FALSE(one.contains(2));

    CHECK(two.size() == 2);
    CHECK(two.get(1) == "one");
    CHECK(two.get(2) == "two");
}

TEST_CASE("ImmutableHashMap set replaces an existing value", "[ImmutableHashMap]")
{
    const auto original = ImmutableHashMap<int, std::string>{}.set(1, "one").set(2, "two");

    const auto updated = original.set(1, "uno");

    CHECK(original.size() == 2);
    CHECK(original.get(1) == "one");
    CHECK(original.get(2) == "two");

    CHECK(updated.size() == 2);
    CHECK(updated.get(1) == "uno");
    CHECK(updated.get(2) == "two");
}

TEST_CASE("ImmutableHashMap set with the same value is a no-op", "[ImmutableHashMap]")
{
    const auto map = ImmutableHashMap<int, std::string>{}.set(1, "one").set(2, "two");

    const auto same = map.set(1, "one");

    CHECK(same.size() == 2);
    CHECK(same.get(1) == "one");
    CHECK(same.get(2) == "two");
    CHECK(same.same_storage(map));
}

TEST_CASE("ImmutableHashMap add rejects an existing key", "[ImmutableHashMap]")
{
    const auto map = ImmutableHashMap<int, std::string>{}.add(1, "one");

    CHECK_THROWS_AS(map.add(1, "uno"), std::invalid_argument);
}

TEST_CASE("ImmutableHashMap try_add leaves existing keys unchanged", "[ImmutableHashMap]")
{
    const auto map = ImmutableHashMap<int, std::string>{}.add(1, "one");

    const auto same = map.try_add(1, "uno");

    CHECK(same.get(1) == "one");
    CHECK(same.same_storage(map));
}

TEST_CASE("ImmutableHashMap remove returns a new map and leaves the original unchanged", "[ImmutableHashMap]")
{
    const auto original = ImmutableHashMap<int, std::string>{}.set(1, "one").set(2, "two").set(3, "three");

    const auto removed = original.remove(2);

    CHECK(original.size() == 3);
    CHECK(original.contains(2));

    CHECK(removed.size() == 2);
    CHECK(removed.contains(1));
    CHECK_FALSE(removed.contains(2));
    CHECK(removed.contains(3));
}

TEST_CASE("ImmutableHashMap removing a missing key is a no-op", "[ImmutableHashMap]")
{
    const auto map = ImmutableHashMap<int, std::string>{}.set(1, "one");

    const auto same = map.remove(2);

    CHECK(same.size() == 1);
    CHECK(same.get(1) == "one");
    CHECK(same.same_storage(map));
}

TEST_CASE("ImmutableHashMap get throws when missing", "[ImmutableHashMap]")
{
    const ImmutableHashMap<int, std::string> map;

    CHECK_THROWS_AS(map.get(1), std::out_of_range);
}

TEST_CASE("ImmutableHashMap iteration visits all entries", "[ImmutableHashMap]")
{
    const auto map = ImmutableHashMap<int, std::string>{}.set(3, "three").set(1, "one").set(2, "two");

    std::vector<int> keys;
    for (const auto &[key, value] : map)
    {
        keys.push_back(key);
        CHECK(value == map.get(key));
    }

    std::ranges::sort(keys);
    CHECK(keys == std::vector{1, 2, 3});
}

TEST_CASE("ImmutableHashMap handles full hash collisions", "[ImmutableHashMap]")
{
    using Map = ImmutableHashMap<int, std::string, ConstantHash>;

    const auto map = Map{}.set(1, "one").set(2, "two").set(3, "three");

    CHECK(map.size() == 3);
    CHECK(map.get(1) == "one");
    CHECK(map.get(2) == "two");
    CHECK(map.get(3) == "three");

    const auto updated = map.set(2, "dos");
    CHECK(updated.size() == 3);
    CHECK(updated.get(1) == "one");
    CHECK(updated.get(2) == "dos");
    CHECK(updated.get(3) == "three");

    const auto removed = updated.remove(1);
    CHECK(removed.size() == 2);
    CHECK_FALSE(removed.contains(1));
    CHECK(removed.get(2) == "dos");
    CHECK(removed.get(3) == "three");
}

TEST_CASE("ImmutableHashMap supports alternative lookup", "[ImmutableHashMap]")
{
    using Map = ImmutableHashMap<std::string, int, TransparentStringHash, TransparentStringEqual>;

    const auto map = Map{}.set(std::string{"alpha"}, 1).set(std::string{"beta"}, 2);

    CHECK(map.contains(std::string_view{"alpha"}));
    CHECK(map.contains("beta"));
    CHECK(map.get(std::string_view{"alpha"}) == 1);
    CHECK(map.get("beta") == 2);
    CHECK_FALSE(map.try_get(std::string_view{"gamma"}).has_value());

    const auto removed = map.remove(std::string_view{"alpha"});

    CHECK_FALSE(removed.contains("alpha"));
    CHECK(removed.contains("beta"));
}

TEST_CASE("ImmutableHashMap clear returns an empty map", "[ImmutableHashMap]")
{
    const auto map = ImmutableHashMap<int, std::string>{}.set(1, "one").set(2, "two");

    const auto cleared = map.clear();

    CHECK(cleared.empty());
    CHECK(cleared.size() == 0);
    CHECK_FALSE(cleared.contains(1));

    CHECK(map.size() == 2);
}
