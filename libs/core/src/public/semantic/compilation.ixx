/**
 * @file compilation.ixx
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

#include <libassert/assert-macros.hpp>

export module prism.core:semantic.compilation;

import :util.noncopyable;
import :syntax.tree;
import :semantic.semantic_lifetime;
import :semantic.semantic_model;
import :binder.declaration_scope;
import :binder.semantic_mappings;
import :symbols.type_symbol;
import :semantic.bound_node_lookup;
import :context.target_settings;

namespace prism
{
    class BoundStatement;
    class BoundExpression;
    class NamedTypeSymbol;
    class AssemblySymbol;

    struct SymbolLookupKey
    {
        const Symbol *symbol = nullptr;
        Name name{};

        [[nodiscard]] constexpr friend bool operator==(const SymbolLookupKey &lhs,
                                                       const SymbolLookupKey &rhs) noexcept = default;
    };
} // namespace prism

template <>
struct std::hash<prism::SymbolLookupKey>
{
    constexpr std::size_t operator()(const prism::SymbolLookupKey &key) const noexcept
    {
        return prism::hash_combine(key.symbol, key.name);
    }
};

namespace prism
{
    export class PRISM_CORE_API Compilation : NonCopyable
    {
        struct CreateTag
        {
        };

      public:
        Compilation(CreateTag, std::vector<std::unique_ptr<SyntaxTree>> trees, TargetSettings target_settings) noexcept;

        static std::unique_ptr<Compilation> create(Name assembly_name,
                                                   std::vector<std::unique_ptr<SyntaxTree>> trees,
                                                   TargetSettings target_settings = TargetSettings::current_platform());

        [[nodiscard]] constexpr const AssemblySymbol &assembly() const noexcept
        {
            ASSUME(assembly_ != nullptr);
            return *assembly_;
        }

        [[nodiscard]] constexpr const NamespaceSymbol &common_global_namespace() const noexcept
        {
            ASSUME(global_namespace_ != nullptr);
            return *global_namespace_;
        }

        [[nodiscard]] constexpr const std::vector<std::unique_ptr<SyntaxTree>> &trees() const noexcept
        {
            return trees_;
        }

        [[nodiscard]] SemanticModel get_semantic_model(const SyntaxTree &tree) noexcept
        {
            return SemanticModel{*this, tree};
        }

        [[nodiscard]] constexpr const std::vector<Diagnostic> &diagnostics() const noexcept
        {
            return diagnostics_;
        }

        [[nodiscard]] const DeclarationScope &get_declaration_scope(const SyntaxNode &node) const;

        [[nodiscard]] const NamedTypeSymbol &get_special_type(SpecialType type) const;

        [[nodiscard]] const NamedTypeSymbol &create_error_type_symbol(Optional<const Symbol &> container, Name name);

        [[nodiscard]] const NamespaceSymbol &create_error_namespace_symbol(Optional<const NamespaceSymbol &> container,
                                                                           Name name);

        [[nodiscard]] Conversion classify_conversion(const TypeSymbol &source, const TypeSymbol &destination) const;

      private:
        friend class MergedNamespaceSymbol;
        friend class SemanticModel;

        std::unique_ptr<SemanticLifetime> lifetime_ = std::make_unique<SemanticLifetime>();
        TargetSettings target_settings_;
        const AssemblySymbol *assembly_ = nullptr;
        const NamespaceSymbol *global_namespace_ = nullptr;
        std::vector<std::unique_ptr<SyntaxTree>> trees_;
        std::vector<Diagnostic> diagnostics_;
        SemanticMappings semantic_mappings_;
        BoundNodeLookup bound_node_lookup_;

        std::mutex error_type_mutex_;
        std::unordered_map<SymbolLookupKey, const NamedTypeSymbol *> error_types_;

        std::mutex error_namespace_mutex_;
        std::unordered_map<SymbolLookupKey, const NamespaceSymbol *> error_namespaces_;

        std::mutex variable_initializer_mutex_;
        std::unordered_map<const VariableSymbol *, const BoundExpression *> variable_initializers_;

        std::mutex function_body_mutex_;
        std::unordered_map<const FunctionSymbol *, const BoundStatement *> function_bodies_;
    };
} // namespace prism
