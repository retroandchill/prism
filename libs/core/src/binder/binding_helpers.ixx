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

namespace prism
{
    class Symbol;
    class NamedTypeSymbol;
    class DiagnosticBag;
    class NameSyntax;
    class SimpleNameSyntax;
    class Compilation;
    class TypeSyntax;
    class TypeSymbol;

    const TypeSymbol &resolve_type(const TypeSyntax &syntax,
                                   const Compilation &compilation,
                                   DiagnosticBag &diagnostics);

    const NamedTypeSymbol &create_error_type_symbol(Optional<const Symbol &> owning_symbol,
                                                    const Compilation &compilation,
                                                    std::span<const Ref<const SimpleNameSyntax>> names);

    PooledVector<Ref<const SimpleNameSyntax>> collect_names(const NameSyntax &syntax);

    [[nodiscard]] bool fits_in(const BigDecimal &value, SpecialType type, TargetSettings settings);
} // namespace prism
