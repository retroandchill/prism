/**
 * @file name_test.cpp
 * @author Francesco Corso
 * @date 6/30/2026
 * @brief
 */
#include <catch2/catch_test_macros.hpp>

import std;
import prism.core.strings.name;

using namespace prism;

namespace
{
    std::string generate_random_string(const size_t length)
    {
        constexpr static std::string_view characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        std::random_device random_device;
        std::mt19937 generator(random_device());
        std::uniform_int_distribution<> distribution(0, characters.size() - 1);

        std::string random_string;
        for (size_t i = 0; i < length; ++i)
        {
            random_string += characters[distribution(generator)];
        }

        return random_string;
    }
} // namespace

TEST_CASE("Find does not create new entries", "[strings]")
{
    const Name existing = "Knight";
    CHECK_FALSE(existing.is_none());

    auto existing_id = existing.id();

    const Name found_existing{"Knight", FindName::find};
    CHECK_FALSE(found_existing.is_none());
    CHECK(found_existing.id() == existing_id);

    const auto unknown_name = generate_random_string(100);
    const Name unknown{unknown_name, FindName::find};
    CHECK(unknown.is_none());
}
