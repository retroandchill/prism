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

    IntegerTargetKind get_integer_target_kind(const TypeSymbol *target_type)
    {
        if (target_type == nullptr)
            return IntegerTargetKind::best_fit;

        switch (target_type->special_type())
        {
            case SpecialType::i8:
                return IntegerTargetKind::i8;
            case SpecialType::i16:
                return IntegerTargetKind::i16;
            case SpecialType::i32:
                return IntegerTargetKind::i32;
            case SpecialType::i64:
                return IntegerTargetKind::i64;
            case SpecialType::i128:
                return IntegerTargetKind::i128;
            case SpecialType::isize:
                return IntegerTargetKind::isize;
            case SpecialType::u8:
                return IntegerTargetKind::u8;
            case SpecialType::u16:
                return IntegerTargetKind::u16;
            case SpecialType::u32:
                return IntegerTargetKind::u32;
            case SpecialType::u64:
                return IntegerTargetKind::u64;
            case SpecialType::u128:
                return IntegerTargetKind::u128;
            case SpecialType::usize:
                return IntegerTargetKind::usize;
            case SpecialType::f32:
                return IntegerTargetKind::f32;
            case SpecialType::f64:
                return IntegerTargetKind::f64;
            default:
                // This will most-likely result in an error unless there's a user-defined conversion from a given
                // integer type
                return IntegerTargetKind::best_fit;
        }
    }

    IntegerTargetKind get_integer_target_kind(const IntegerLiteralData &literal, const TypeSymbol *target_type)
    {
        switch (literal.suffix)
        {
            case IntegerSuffix::none:
                return get_integer_target_kind(target_type);
            case IntegerSuffix::i8:
                return IntegerTargetKind::i8;
            case IntegerSuffix::i16:
                return IntegerTargetKind::i16;
            case IntegerSuffix::i32:
                return IntegerTargetKind::i32;
            case IntegerSuffix::i64:
                return IntegerTargetKind::i64;
            case IntegerSuffix::i128:
                return IntegerTargetKind::i128;
            case IntegerSuffix::iz:
                return IntegerTargetKind::isize;
            case IntegerSuffix::u8:
                return IntegerTargetKind::u8;
            case IntegerSuffix::u16:
                return IntegerTargetKind::u16;
            case IntegerSuffix::u32:
                return IntegerTargetKind::u32;
            case IntegerSuffix::u64:
                return IntegerTargetKind::u64;
            case IntegerSuffix::u128:
                return IntegerTargetKind::u128;
            case IntegerSuffix::uz:
                return IntegerTargetKind::usize;
        }

        UNREACHABLE("Invalid integer suffix");
    }

    bool fits_in(const BigInteger &value, const IntegerTargetKind type, const TargetSettings &settings)
    {
        switch (type)
        {
            case IntegerTargetKind::i8:
                return fits_in<std::int8_t>(value);
            case IntegerTargetKind::i16:
                return fits_in<std::int16_t>(value);
            case IntegerTargetKind::i32:
                return fits_in<std::int32_t>(value);
            case IntegerTargetKind::i64:
                return fits_in<std::int64_t>(value);
            case IntegerTargetKind::i128:
                return fits_in<Int128>(value);
            case IntegerTargetKind::isize:
                switch (settings.pointer_width)
                {
                    case PointerWidth::x32:
                        return fits_in<std::int32_t>(value);
                    case PointerWidth::x64:
                        return fits_in<std::int64_t>(value);
                    default:
                        throw std::invalid_argument{"Invalid pointer width"};
                }
            case IntegerTargetKind::u8:
                return fits_in<std::uint8_t>(value);
            case IntegerTargetKind::u16:
                return fits_in<std::uint16_t>(value);
            case IntegerTargetKind::u32:
                return fits_in<std::uint32_t>(value);
            case IntegerTargetKind::u64:
                return fits_in<std::uint64_t>(value);
            case IntegerTargetKind::u128:
                return fits_in<UInt128>(value);
            case IntegerTargetKind::usize:
                switch (settings.pointer_width)
                {
                    case PointerWidth::x32:
                        return fits_in<std::uint32_t>(value);
                    case PointerWidth::x64:
                        return fits_in<std::uint64_t>(value);
                    default:
                        throw std::invalid_argument{"Invalid pointer width"};
                }
            case IntegerTargetKind::f32:
                return fits_in<float>(value);
            case IntegerTargetKind::f64:
                return fits_in<double>(value);
            case IntegerTargetKind::best_fit:
                return fits_in<UInt128>(value) || fits_in<Int128>(value);
        }

        UNREACHABLE("Invalid input");
    }
} // namespace prism
