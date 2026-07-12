/**
 * @file source_file_test.cpp
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
module;
#include <catch2/catch_test_macros.hpp>

module prism.core:tests.source_file;

import std;
import :text.source_file;

using namespace prism;

TEST_CASE("Detect line and columns a source text sample", "[source_file]")
{
    constexpr static auto *sample = "func main() {\n"
                                    "    return 0;\n"
                                    "}";

    const SourceText file{sample};
    CHECK(file.position_of(8) == SourcePosition{1, 9});
    CHECK(file.position_of(21) == SourcePosition{2, 8});
    CHECK_THROWS_AS(file.position_of(40), std::out_of_range);
}
