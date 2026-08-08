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

    const TypeSymbol &resolve_type(const TypeSyntax &syntax, Compilation &compilation, DiagnosticBag &diagnostics);

    const NamedTypeSymbol &create_error_type_symbol(Optional<const Symbol &> owning_symbol,
                                                    Compilation &compilation,
                                                    std::span<const Ref<const SimpleNameSyntax>> names);

    PooledVector<Ref<const SimpleNameSyntax>> collect_names(const NameSyntax &syntax);
} // namespace prism
