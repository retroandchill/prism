/**
 * @file symbol_creation_test.cpp
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
#include <catch2/catch_test_macros.hpp>

import std;
import prism.core;

using namespace prism;

TEST_CASE("Compilation merges namespace declarations", "[compilation]")
{
    auto tree = SyntaxTree::parse(R"(
        namespace A::B {
            var x: i32 = 5;
        }

        namespace A {
            namespace B {
                func f(p: i32) {}
            }
        }
    )");

    const Name assembly_name = "test";
    ImmutableArray trees{std::move(tree)};
    const auto compilation = Compilation::create(assembly_name, std::move(trees));

    auto members = compilation->assembly().global_namespace().members();
    REQUIRE(members.size() == 1);
    CHECK(members[0]->name() == Name{"A"});

    REQUIRE(members[0]->is<NamespaceSymbol>());
    members = members[0]->as<NamespaceSymbol>()->members();
    REQUIRE(members.size() == 1);

    CHECK(members[0]->name() == Name{"B"});
    members = members[0]->as<NamespaceSymbol>()->members();
    REQUIRE(members.size() == 2);

    CHECK(members[0]->name() == Name{"x"});
    CHECK(members[0]->is<VariableSymbol>());
    CHECK(members[1]->name() == Name{"f"});
    CHECK(members[1]->is<FunctionSymbol>());

    auto &x = members[0]->as<VariableSymbol>()->type();
    CHECK(x.special_type() == SpecialType::i32);

    auto &f = members[1]->as<FunctionSymbol>()->return_type();
    CHECK(f.special_type() == SpecialType::void_);

    auto &p = members[1]->as<FunctionSymbol>()->parameters()[0]->type();
    CHECK(p.special_type() == SpecialType::i32);
}
