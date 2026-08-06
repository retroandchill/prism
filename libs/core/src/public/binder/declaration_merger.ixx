/**
 * @file declaration_merger.ixx
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
export module prism.core:binder.declaration_merger;

import :binder.declaration_binder;
import :diagnostics.diagnostic_bag;

namespace prism
{
    class SourceParameterSymbol;
    class SourceFunctionSymbol;
    class SourceVariableSymbol;
    class SourceAssemblySymbol;
    class Symbol;
    class SemanticLifetime;
    class SourceNamespaceSymbol;
    class AssemblySymbol;
    class SemanticMappings;

    using PartiallyBoundSymbol =
        std::variant<Ref<SourceVariableSymbol>, Ref<SourceFunctionSymbol>, Ref<SourceParameterSymbol>>;

    class DeclarationMerger final : NonCopyable
    {
      public:
        constexpr DeclarationMerger(const Name assembly_name,
                                    SemanticLifetime &lifetime,
                                    SemanticMappings &mappings,
                                    std::vector<PartiallyBoundSymbol> &partially_bound) noexcept
            : assembly_name_{assembly_name}, lifetime_{lifetime}, mappings_{mappings}, partially_bound_{partially_bound}
        {
        }

        const AssemblySymbol &merge(std::span<const DeclarationRecord> records);

      private:
        void merge_declaration(const DeclarationRecord &record, SourceNamespaceSymbol &containing_namespace);
        void merge_namespace(const NamespaceRecord &record, SourceNamespaceSymbol &containing_namespace);
        void merge_variable(const VariableRecord &record, SourceNamespaceSymbol &containing_namespace) const;
        void merge_function(const FunctionRecord &record, SourceNamespaceSymbol &containing_namespace) const;

        [[nodiscard]] SourceNamespaceSymbol &get_or_create_namespace(SourceNamespaceSymbol &containing_namespace,
                                                                     Name name);

        Name assembly_name_;
        SemanticLifetime &lifetime_;
        SemanticMappings &mappings_;
        std::vector<PartiallyBoundSymbol> &partially_bound_;

        SourceAssemblySymbol *assembly_ = nullptr;
        SourceNamespaceSymbol *global_namespace_ = nullptr;

        std::unordered_map<const SourceNamespaceSymbol *, std::unordered_map<Name, SourceNamespaceSymbol *>>
            namespaces_;
    };
} // namespace prism
