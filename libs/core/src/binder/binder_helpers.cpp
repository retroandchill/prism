/**
 * @file binder_helpers.cpp
 * @author Francesco Corso
 * @date 8/6/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:binder.binding_helpers.impl;

import std;
import :binder.binding_helpers;
import :syntax.visit;
import :semantic.compilation;
import :symbols.named_type_symbol;
import :diagnostics.diagnostic_bag;
import :diagnostics.info;
import :symbols.visit;

namespace prism
{

    SpecialType from_token(const SyntaxKind kind)
    {
        switch (kind)
        {
            case SyntaxKind::void_keyword:
                return SpecialType::void_;
            case SyntaxKind::bool_keyword:
                return SpecialType::bool_;
            case SyntaxKind::i8_keyword:
                return SpecialType::i8;
            case SyntaxKind::i16_keyword:
                return SpecialType::i16;
            case SyntaxKind::i32_keyword:
                return SpecialType::i32;
            case SyntaxKind::i64_keyword:
                return SpecialType::i64;
            case SyntaxKind::i128_keyword:
                return SpecialType::i128;
            case SyntaxKind::u8_keyword:
                return SpecialType::u8;
            case SyntaxKind::u16_keyword:
                return SpecialType::u16;
            case SyntaxKind::u32_keyword:
                return SpecialType::u32;
            case SyntaxKind::u64_keyword:
                return SpecialType::u64;
            case SyntaxKind::u128_keyword:
                return SpecialType::u128;
            case SyntaxKind::isize_keyword:
                return SpecialType::isize;
            case SyntaxKind::usize_keyword:
                return SpecialType::usize;
            case SyntaxKind::f16_keyword:
                return SpecialType::f16;
            case SyntaxKind::f32_keyword:
                return SpecialType::f32;
            case SyntaxKind::f64_keyword:
                return SpecialType::f64;
            case SyntaxKind::char_keyword:
                return SpecialType::char_;
            case SyntaxKind::char16_keyword:
                return SpecialType::char16;
            case SyntaxKind::rune_keyword:
                return SpecialType::rune;
            case SyntaxKind::str_keyword:
                return SpecialType::str;
            default:
                [[unlikely]] throw std::invalid_argument{"unknown special type"};
        }
    }

    namespace
    {

        const TypeSymbol &resolve_symbol_chain(const Compilation &compilation,
                                               DiagnosticBag &diagnostics,
                                               const NamedTypeSyntax &named)
        {
            auto &scope = compilation.get_declaration_scope(named);
            const auto names = collect_names(named.identifier());
            std::span names_span{names};
            ASSUME(!names_span.empty());

            const Symbol *current_symbol = nullptr;
            while (!names_span.empty())
            {
                const auto outer_name = names_span.front()->identifier().get_value<IdentifierData>().name;

                auto result = scope.lookup_nearest(outer_name);
                if (!result.found())
                {
                    diagnostics.add(Diagnostic{DiagnosticInfo::create<DiagnosticCode::unresolved_symbol>(outer_name),
                                               names.front()->location()});

                    return create_error_type_symbol(std::nullopt, compilation, names);
                }

                if (result.ambiguous())
                {
                    diagnostics.add(Diagnostic{DiagnosticInfo::create<DiagnosticCode::ambiguous_symbol>(outer_name),
                                               names.front()->location()});

                    return create_error_type_symbol(std::nullopt, compilation, names);
                }

                current_symbol = &result.symbol();
                names_span = names_span.subspan(1);
            }

            return visit(*current_symbol,
                         Overload{[](const TypeSymbol &type) -> auto & { return type; },
                                  [&](const Symbol &symbol) -> const TypeSymbol &
                                  {
                                      return compilation.create_error_type_symbol(symbol.containing_symbol(),
                                                                                  symbol.name());
                                  }});
        }
    } // namespace

    const TypeSymbol &resolve_type(const TypeSyntax &syntax, const Compilation &compilation, DiagnosticBag &diagnostics)
    {
        return visit(syntax,
                     Overload{[&](const PredefinedTypeSyntax &predefined) -> const TypeSymbol &
                              { return compilation.get_special_type(from_token(predefined.keyword().kind())); },
                              [&](const NamedTypeSyntax &named) -> const TypeSymbol &
                              {
                                  return resolve_symbol_chain(compilation, diagnostics, named);
                              }});
    }

    const NamedTypeSymbol &create_error_type_symbol(Optional<const Symbol &> owning_symbol,
                                                    const Compilation &compilation,
                                                    const std::span<const Ref<const SimpleNameSyntax>> names)
    {
        ASSUME(!names.empty());
        for (const auto syntax : names.subspan(0, names.size() - 1))
        {
            auto name = syntax->identifier().get_value<IdentifierData>().name;
            if (!owning_symbol.has_value())
                owning_symbol = compilation.create_error_namespace_symbol(std::nullopt, name);

            visit(*owning_symbol,
                  Overload{[&](const NamespaceSymbol &symbol)
                           { owning_symbol = compilation.create_error_namespace_symbol(symbol, name); },
                           [&](const TypeSymbol &symbol)
                           { owning_symbol = compilation.create_error_type_symbol(symbol, name); },
                           [&](const Symbol &)
                           {
                               // TODO: Figure out what to do if we get a variable or a function
                           }});
        }

        const auto last = names.back()->identifier().get_value<IdentifierData>().name;
        return compilation.create_error_type_symbol(owning_symbol, last);
    }

    PooledVector<Ref<const SimpleNameSyntax>> collect_names(const NameSyntax &syntax)
    {
        PooledVector<Ref<const SimpleNameSyntax>> stack;

        auto *current = &syntax;
        while (current != nullptr)
        {
            visit(*current,
                  Overload{[&](const SimpleNameSyntax &simple)
                           {
                               stack.emplace_back(simple);
                               current = nullptr;
                           },
                           [&](const QualifiedNameSyntax &qualified)
                           {
                               stack.emplace_back(qualified.right());
                               current = &qualified.left();
                           }});
        }

        // We added elements in the reverse order we need to go through them in so we reverse the stack
        std::ranges::reverse(stack);
        return stack;
    }

    Name get_identifier_name(const SyntaxToken &syntax)
    {
        DEBUG_ASSERT(syntax.kind() == SyntaxKind::identifier_token);
        return syntax.get_value<IdentifierData>().name;
    }

    Name get_unqualified_name(const SimpleNameSyntax &syntax)
    {
        return get_identifier_name(syntax.identifier());
    }

    bool fits_in(const BigDecimal &value, const SpecialType type, TargetSettings settings)
    {
        switch (type)
        {
            case SpecialType::i8:
                return fits_in<std::int8_t>(value);
            case SpecialType::i16:
                return fits_in<std::int16_t>(value);
            case SpecialType::i32:
                return fits_in<std::int32_t>(value);
            case SpecialType::i64:
                return fits_in<std::int64_t>(value);
            case SpecialType::i128:
                return fits_in<Int128>(value);
            case SpecialType::isize:
                switch (settings.pointer_width)
                {
                    case 32:
                        return fits_in<std::int32_t>(value);
                    case 64:
                        return fits_in<std::int64_t>(value);
                    default:
                        throw std::invalid_argument{"Invalid pointer width"};
                }
            case SpecialType::u8:
                return fits_in<std::uint8_t>(value);
            case SpecialType::u16:
                return fits_in<std::uint16_t>(value);
            case SpecialType::u32:
                return fits_in<std::uint32_t>(value);
            case SpecialType::u64:
                return fits_in<std::uint64_t>(value);
            case SpecialType::u128:
                return fits_in<UInt128>(value);
            case SpecialType::usize:
                switch (settings.pointer_width)
                {
                    case 32:
                        return fits_in<std::uint32_t>(value);
                    case 64:
                        return fits_in<std::uint64_t>(value);
                    default:
                        throw std::invalid_argument{"Invalid pointer width"};
                }
            case SpecialType::f16:
                return fits_in_f16(value);
            case SpecialType::f32:
                return fits_in<float>(value);
            case SpecialType::f64:
                return fits_in<double>(value);
            default:
                throw std::invalid_argument{"Not an integral type"};
        }
    }
} // namespace prism
