/**
 * @file immutable_string_test.cpp
 * @author Francesco Corso
 * @date 8/21/2026
 * @brief
 */
#include <catch2/catch_test_macros.hpp>

import std;
import prism.core;

using namespace prism;

TEST_CASE("ImmutableString default construction creates an empty string", "[ImmutableString]")
{
    constexpr ImmutableString text;

    CHECK(text.empty());
    CHECK(text.size() == 0);
    CHECK(text.length() == 0);
    CHECK(text.data() == nullptr);
    CHECK(text.begin() == text.end());
    CHECK(text.as_span().empty());
    CHECK(text.view().empty());
    CHECK(text.use_count() == 0);
}

TEST_CASE("ImmutableString can be constructed from string literals and string_view", "[ImmutableString]")
{
    const ImmutableString from_literal = "hello";
    const ImmutableString from_view = std::string_view{"world"};

    CHECK(from_literal.size() == 5);
    CHECK(from_literal.view() == "hello");

    CHECK(from_view.size() == 5);
    CHECK(from_view.view() == "world");
}

TEST_CASE("ImmutableString supports indexing and bounds-checked access", "[ImmutableString]")
{
    const ImmutableString text = "hello";

    CHECK(text[0] == 'h');
    CHECK(text[4] == 'o');
    CHECK(text.at(1) == 'e');
    CHECK(text.front() == 'h');
    CHECK(text.back() == 'o');
    CHECK_THROWS_AS(text.at(5), std::out_of_range);
}

TEST_CASE("ImmutableString supports forward and reverse iteration", "[ImmutableString]")
{
    const ImmutableString text = "abc";

    const std::string forward{text.begin(), text.end()};
    const std::string reverse{text.rbegin(), text.rend()};

    CHECK(forward == "abc");
    CHECK(reverse == "cba");
}

TEST_CASE("ImmutableString copy construction shares storage", "[ImmutableString]")
{
    ImmutableString original = "shared";
    const auto *const original_data = original.data();

    const ImmutableString copy = original;

    CHECK(copy.view() == "shared");
    CHECK(copy.data() == original_data);
    CHECK(copy.data() == original.data());
    CHECK(copy.use_count() == 2);
    CHECK(original.use_count() == 2);
}

TEST_CASE("ImmutableString substr returns a new string", "[ImmutableString]")
{
    const ImmutableString text = "hello world";

    const auto sub1 = text.substr(0, 5);
    const auto sub2 = text.substr(6);
    const auto sub3 = text.substr(3, 100);

    CHECK(sub1.view() == "hello");
    CHECK(sub2.view() == "world");
    CHECK(sub3.view() == "lo world");
    CHECK_THROWS_AS(text.substr(12), std::out_of_range);
}

TEST_CASE("ImmutableString append returns a new string and leaves original unchanged", "[ImmutableString]")
{
    const ImmutableString original = "hello";

    const auto updated = original.append(" world");

    CHECK(original.view() == "hello");
    CHECK(updated.view() == "hello world");
    CHECK(updated.data() != original.data());
}

TEST_CASE("ImmutableString push_back returns a new string", "[ImmutableString]")
{
    const ImmutableString original = "ab";

    const auto updated = original.push_back('c');

    CHECK(original.view() == "ab");
    CHECK(updated.view() == "abc");
}

TEST_CASE("ImmutableString insert can insert text and single characters", "[ImmutableString]")
{
    const ImmutableString original = "helo";

    const auto inserted_char = original.insert(3, 'l');
    const auto inserted_text = inserted_char.insert(4, " world");

    CHECK(original.view() == "helo");
    CHECK(inserted_char.view() == "hello");
    CHECK(inserted_text.view() == "hell worldo");

    CHECK_THROWS_AS(original.insert(5, 'x'), std::out_of_range);
}

TEST_CASE("ImmutableString erase removes ranges immutably", "[ImmutableString]")
{
    const ImmutableString original = "hello world";

    const auto erased_middle = original.erase(5, 1);
    const auto erased_tail = original.erase(5);
    const auto no_op = original.erase(3, 0);

    CHECK(original.view() == "hello world");
    CHECK(erased_middle.view() == "helloworld");
    CHECK(erased_tail.view() == "hello");
    CHECK(no_op.view() == "hello world");
    CHECK(no_op.data() == original.data());

    CHECK_THROWS_AS(original.erase(12, 1), std::out_of_range);
}

TEST_CASE("ImmutableString replace returns a new string", "[ImmutableString]")
{
    const ImmutableString original = "hello world";

    const auto updated = original.replace(6, 5, "there");

    CHECK(original.view() == "hello world");
    CHECK(updated.view() == "hello there");

    CHECK_THROWS_AS(original.replace(12, 1, "x"), std::out_of_range);
}

TEST_CASE("ImmutableString set replaces a character immutably", "[ImmutableString]")
{
    const ImmutableString original = "hello";

    const auto updated = original.set(1, 'a');

    CHECK(original.view() == "hello");
    CHECK(updated.view() == "hallo");
    CHECK_THROWS_AS(original.set(5, 'x'), std::out_of_range);
}

TEST_CASE("ImmutableString clear returns an empty string", "[ImmutableString]")
{
    const ImmutableString original = "hello";

    const auto cleared = original.clear();

    CHECK(original.view() == "hello");
    CHECK(cleared.empty());
    CHECK(cleared.size() == 0);
    CHECK(cleared.data() == nullptr);
}

TEST_CASE("ImmutableString supports find and rfind", "[ImmutableString]")
{
    const ImmutableString text = "banana";

    CHECK(text.find('b') == 0);
    CHECK(text.find('a') == 1);
    CHECK(text.find("ana") == 1);
    CHECK(text.find("xyz") == ImmutableString::npos);

    CHECK(text.rfind("ana") == 3);
    CHECK(text.rfind("xyz") == ImmutableString::npos);
}

TEST_CASE("ImmutableString supports starts_with ends_with and contains", "[ImmutableString]")
{
    const ImmutableString text = "hello world";

    CHECK(text.starts_with("hello"));
    CHECK(text.starts_with('h'));
    CHECK_FALSE(text.starts_with("world"));

    CHECK(text.ends_with("world"));
    CHECK(text.ends_with('d'));
    CHECK_FALSE(text.ends_with("hello"));

    CHECK(text.contains("lo wo"));
    CHECK(text.contains('w'));
    CHECK_FALSE(text.contains("xyz"));
}

TEST_CASE("ImmutableString equality and ordering compare by value", "[ImmutableString]")
{
    const ImmutableString first = "abc";
    const ImmutableString second = "abc";
    const ImmutableString third = "abd";
    const ImmutableString shorter = "ab";

    CHECK(first == second);
    CHECK(first < third);
    CHECK(shorter < first);
    CHECK((first <=> second) == std::strong_ordering::equal);
}

TEST_CASE("ImmutableString operator plus concatenates strings", "[ImmutableString]")
{
    const ImmutableString a = "hello";
    const ImmutableString b = " world";

    const auto c = a + b;
    const auto d = a + std::string_view{" there"};
    const auto e = std::string_view{"say "} + a;

    CHECK(c.view() == "hello world");
    CHECK(d.view() == "hello there");
    CHECK(e.view() == "say hello");
}
