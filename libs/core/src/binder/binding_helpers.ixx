/**
 * @file binding_helpers.ixx
 * @author Francesco Corso
 * @date 8/6/2026
 * @brief
 */

export module prism.core:binder.binding_helpers;

import :memory.buffer_pool;
import :text.name;
import :util.ref;
import :symbols.type_symbol;
import :context.target_settings;
import :util.numerics;

namespace prism
{
    struct FloatLiteralData;
    struct IntegerLiteralData;
    enum class LookupOptions : std::uint32_t;
    class LookupContext;
    class Binder;
    class Symbol;
    class NamedTypeSymbol;
    class DiagnosticBag;
    class NameSyntax;
    class SimpleNameSyntax;
    class Compilation;
    class TypeSyntax;
    class TypeSymbol;

    void diagnose_lookup_failure(const LookupResult &result,
                                 const NameSyntax &syntax,
                                 LookupOptions expected,
                                 const LookupContext &context);

    const NamedTypeSymbol &create_error_type_symbol(Optional<const Symbol &> owning_symbol,
                                                    const Compilation &compilation,
                                                    std::span<const Ref<const SimpleNameSyntax>> names);

    PooledVector<Ref<const SimpleNameSyntax>> collect_names(const NameSyntax &syntax);

    Name get_identifier_name(const SyntaxToken &syntax);

    Name get_unqualified_name(const NameSyntax &syntax);

    Name get_unqualified_name(const SimpleNameSyntax &syntax);

    enum class IntegerTargetKind
    {
        i8,
        i16,
        i32,
        i64,
        i128,
        isize,
        u8,
        u16,
        u32,
        u64,
        u128,
        usize,
        f32,
        f64,
        best_fit
    };

    IntegerTargetKind get_integer_target_kind(const TypeSymbol *target_type);

    IntegerTargetKind get_integer_target_kind(const IntegerLiteralData &literal, const TypeSymbol *target_type);

    bool fits_in(const BigInteger &value, IntegerTargetKind type, TargetSettings settings);
} // namespace prism
