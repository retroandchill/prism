/**
 * @file symbol.cpp
 * @author Francesco Corso
 * @date 6/27/2026
 * @brief
 */
module;

#include <cassert>

module prism.core.semantic.symbol;

namespace prism
{
    Symbol &SymbolTable::add_symbol(const DeclarationSyntax &declaration, std::unique_ptr<Symbol> symbol)
    {
        assert(symbol != nullptr);
        auto &new_symbol = *all_symbols_.emplace_back(std::move(symbol));
        new_symbol.declarations_.emplace_back(declaration);
        auto [it, inserted] = symbols_.try_emplace(new_symbol.name(), &new_symbol);

        new_symbol.scope_ = this;
        if (!inserted)
        {
            diagnostic_sink_.report(Severity::error,
                                    get_range(declaration),
                                    DuplicateSymbolDefinition{
                                        .name = new_symbol.name(),
                                    });
        }

        return new_symbol;
    }

    Symbol *SymbolTable::resolve(const SharedString &symbol) const
    {
        if (const auto it = symbols_.find(symbol); it != symbols_.end())
        {
            return it->second;
        }

        return parent_ != nullptr ? parent_->resolve(symbol) : nullptr;
    }

    const TypeSymbol &TypeSymbol::built_in(const BuiltInType type)
    {
        static const std::flat_map<BuiltInType, TypeSymbol> built_in_types = {
            {BuiltInType::void_type, TypeSymbol{SharedString{"void"}}},
            {BuiltInType::boolean, TypeSymbol{SharedString{"bool"}}},
            {BuiltInType::i8, TypeSymbol{SharedString{"i8"}}},
            {BuiltInType::i16, TypeSymbol{SharedString{"i16"}}},
            {BuiltInType::i32, TypeSymbol{SharedString{"i32"}}},
            {BuiltInType::i64, TypeSymbol{SharedString{"i64"}}},
            {BuiltInType::i128, TypeSymbol{SharedString{"i128"}}},
            {BuiltInType::isize, TypeSymbol{SharedString{"isize"}}},
            {BuiltInType::u8, TypeSymbol{SharedString{"u8"}}},
            {BuiltInType::u16, TypeSymbol{SharedString{"u16"}}},
            {BuiltInType::u32, TypeSymbol{SharedString{"u32"}}},
            {BuiltInType::u64, TypeSymbol{SharedString{"u64"}}},
            {BuiltInType::u128, TypeSymbol{SharedString{"u128"}}},
            {BuiltInType::usize, TypeSymbol{SharedString{"usize"}}},
            {BuiltInType::f16, TypeSymbol{SharedString{"f16"}}},
            {BuiltInType::f32, TypeSymbol{SharedString{"f32"}}},
            {BuiltInType::f64, TypeSymbol{SharedString{"f64"}}},
            {BuiltInType::char8, TypeSymbol{SharedString{"char"}}},
            {BuiltInType::char16, TypeSymbol{SharedString{"char16"}}},
            {BuiltInType::rune, TypeSymbol{SharedString{"rune"}}},
            {BuiltInType::str, TypeSymbol{SharedString{"str"}}},
        };

        const auto it = built_in_types.find(type);
        if (it == built_in_types.end())
        {
            throw std::invalid_argument{"Not a valid built-in type"};
        }

        return it->second;
    }
} // namespace prism
