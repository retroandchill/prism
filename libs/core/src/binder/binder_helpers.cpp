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
import :binder;
import :binder.lookup_context;

namespace prism
{
    namespace
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
    } // namespace

    void diagnose_lookup_failure(const LookupResult &result,
                                 const NameSyntax &syntax,
                                 const LookupOptions expected,
                                 const LookupContext &context)
    {
        switch (result.kind())
        {

            case LookupResultKind::viable:
            case LookupResultKind::error:
                break;
            case LookupResultKind::not_found:
                {
                    const auto name = get_unqualified_name(syntax);
                    context.report_diagnostic(
                        Diagnostic{DiagnosticInfo::create<DiagnosticCode::unresolved_symbol>(name), syntax.location()});

                    break;
                }
            case LookupResultKind::ambiguous:
                {
                    const auto name =
                        !result.symbols().empty() ? result.symbols().front()->name() : get_unqualified_name(syntax);

                    context.report_diagnostic(
                        Diagnostic{DiagnosticInfo::create<DiagnosticCode::ambiguous_symbol>(name), syntax.location()});
                    break;
                }
            case LookupResultKind::inaccessible:
                {
                    const auto name = !result.symbols().empty()
                                          ? result.symbols().front()->name()
                                          : collect_names(syntax).back()->identifier().get_value<IdentifierData>().name;

                    context.report_diagnostic(
                        Diagnostic{DiagnosticInfo::create<DiagnosticCode::symbol_inaccessible>(name),
                                   syntax.location()});
                    break;
                }
            case LookupResultKind::wrong_kind:
                {
                    const auto name = get_unqualified_name(syntax);

                    context.report_diagnostic(
                        Diagnostic{DiagnosticInfo::create<DiagnosticCode::invalid_symbol>(name, to_string(expected)),
                                   syntax.location()});

                    break;
                }
        }
    }

    const TypeSymbol &require_type(const LookupResult &result,
                                   const NameSyntax &syntax,
                                   const Binder &binder,
                                   const LookupContext &context)
    {
        if (result.viable())
        {
            if (const auto type = result.symbol().as<TypeSymbol>(); type.has_value())
                return *type;
        }

        diagnose_lookup_failure(result, syntax, LookupOptions::type, context);
        auto names = collect_names(syntax);
        return create_error_type_symbol(binder.containing_symbol(), binder.compilation(), names);
    }

    const TypeSymbol &resolve_type(const TypeSyntax &syntax, const Binder &binder, const LookupContext &context)
    {
        return visit(syntax,
                     Overload{
                         [&](const NamedTypeSyntax &named) -> auto &
                         {
                             const auto result =
                                 binder.lookup_from_syntax(named.identifier(), LookupOptions::type, context);
                             return require_type(result, named.identifier(), binder, context);
                         },
                         [&](const PredefinedTypeSyntax &predefined) -> const TypeSymbol &
                         { return binder.compilation().get_special_type(from_token(predefined.keyword().kind())); },
                     });
    }

    const NamedTypeSymbol &create_error_type_symbol(Optional<const Symbol &> owning_symbol,
                                                    const Compilation &compilation,
                                                    const std::span<const Ref<const SimpleNameSyntax>> names)
    {
        ASSUME(!names.empty());
        for (const auto syntax : names.subspan(0, names.size() - 1))
        {
            auto name = get_identifier_name(syntax->identifier());
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

    Name get_unqualified_name(const NameSyntax &syntax)
    {
        return visit(syntax,
                     Overload{[](const SimpleNameSyntax &simple) { return get_unqualified_name(simple); },
                              [](const QualifiedNameSyntax &qualified)
                              {
                                  return get_unqualified_name(qualified.right());
                              }});
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
