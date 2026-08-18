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

    const TypeSymbol &require_type(const LookupResult &result,
                                   const NameSyntax &syntax,
                                   const Binder &binder,
                                   const LookupContext &context);

    const TypeSymbol &resolve_type(const TypeSyntax &syntax, const Binder &binder, const LookupContext &context);

    const NamedTypeSymbol &create_error_type_symbol(Optional<const Symbol &> owning_symbol,
                                                    const Compilation &compilation,
                                                    std::span<const Ref<const SimpleNameSyntax>> names);

    PooledVector<Ref<const SimpleNameSyntax>> collect_names(const NameSyntax &syntax);

    Name get_identifier_name(const SyntaxToken &syntax);

    Name get_unqualified_name(const NameSyntax &syntax);

    Name get_unqualified_name(const SimpleNameSyntax &syntax);
} // namespace prism
