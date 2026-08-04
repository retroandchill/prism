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
    class SourceAssemblySymbol;
    class Symbol;
    class SymbolLifetime;
    class SourceNamespaceSymbol;
    class AssemblySymbol;

    class DeclarationMerger final
    {
      public:
        constexpr DeclarationMerger(const Name assembly_name, SymbolLifetime &lifetime) noexcept
            : assembly_name_{assembly_name}, lifetime_{lifetime}
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
        SymbolLifetime &lifetime_;

        SourceAssemblySymbol *assembly_ = nullptr;
        SourceNamespaceSymbol *global_namespace_ = nullptr;

        std::unordered_map<const SourceNamespaceSymbol *, std::unordered_map<Name, SourceNamespaceSymbol *>>
            namespaces_;
    };
} // namespace prism
