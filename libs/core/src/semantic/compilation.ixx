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
import :semantic.syntax_and_declaration_manager;

namespace prism
{
    class Binder;
    class BinderFactory;
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
    export class PRISM_CORE_API Compilation final : NonCopyable
    {
        struct CreateTag
        {
        };

      public:
        Compilation(CreateTag,
                    SemanticLifetime &lifetime,
                    Name assembly_name,
                    TargetSettings target_settings,
                    RefCountPtr<SyntaxAndDeclarationManager> syntax_and_declarations) noexcept;

        static std::shared_ptr<Compilation> create(Name assembly_name,
                                                   ImmutableArray<std::shared_ptr<const SyntaxTree>> trees = {},
                                                   TargetSettings target_settings = TargetSettings::current_platform());

        [[nodiscard]] constexpr const TargetSettings &target_settings() const noexcept
        {
            return target_settings_;
        }

        [[nodiscard]] constexpr const Name &assembly_name() const noexcept
        {
            return assembly_name_;
        }

        [[nodiscard]] const AssemblySymbol &assembly() const;

        [[nodiscard]] const NamespaceSymbol &global_namespace() const;

        [[nodiscard]] Optional<const NamespaceSymbol &> get_compilation_namespace(const NamespaceSymbol &symbol) const;

        [[nodiscard]] const ImmutableArray<std::shared_ptr<const SyntaxTree>> &trees() const noexcept;

        [[nodiscard]] bool contains_syntax_tree(const SyntaxTree &tree) const noexcept;

        [[nodiscard]] const SemanticModel &get_semantic_model(const SyntaxTree &tree) const;

        [[nodiscard]] constexpr const std::vector<Diagnostic> &diagnostics() const noexcept
        {
            return diagnostics_;
        }

        [[nodiscard]] const DeclarationScope &get_declaration_scope(const SyntaxNode &node) const;

        [[nodiscard]] const NamedTypeSymbol &get_special_type(SpecialType type) const;

        [[nodiscard]] const NamedTypeSymbol &create_error_type_symbol(Optional<const Symbol &> container,
                                                                      Name name) const;

        [[nodiscard]] const NamespaceSymbol &create_error_namespace_symbol(Optional<const NamespaceSymbol &> container,
                                                                           Name name) const;

        [[nodiscard]] Conversion classify_conversion(const TypeSymbol &source, const TypeSymbol &destination) const;

        [[nodiscard]] std::shared_ptr<Compilation> shared_from_this() noexcept;

        [[nodiscard]] std::shared_ptr<const Compilation> shared_from_this() const noexcept;

      private:
        [[nodiscard]] const DeclarationTable &declarations() const;
        [[nodiscard]] const MergedNamespaceDeclaration &merged_root_declaration() const;

        [[nodiscard]] std::strong_ordering compare_source_locations(const Location &lhs, const Location &rhs) const;
        [[nodiscard]] std::strong_ordering compare_source_locations(const SourceLocation &lhs,
                                                                    const SourceLocation &rhs) const;
        [[nodiscard]] std::strong_ordering compare_source_locations(const SyntaxReference &lhs,
                                                                    const SyntaxReference &rhs) const;
        [[nodiscard]] std::strong_ordering compare_source_locations(const SyntaxNode &lhs, const SyntaxNode &rhs) const;
        [[nodiscard]] std::strong_ordering compare_syntax_tree_ordering(const SyntaxTree &lhs,
                                                                        const SyntaxTree &rhs) const;

        [[nodiscard]] std::uint32_t get_syntax_tree_ordinal(const SyntaxTree &tree) const;

        [[nodiscard]] const BinderFactory &get_binder_factory(const SyntaxTree &tree) const;
        [[nodiscard]] const Binder &root_binder() const;

        friend struct CompilationInternal;

        SemanticLifetime &lifetime_;
        Name assembly_name_;
        TargetSettings target_settings_;
        RefCountPtr<SyntaxAndDeclarationManager> syntax_and_declaration_manager_;
        mutable Lazy<const AssemblySymbol &> assembly_;
        mutable Lazy<const NamespaceSymbol &> global_namespace_;

        mutable std::mutex compilation_namespace_mutex_;
        mutable std::unordered_map<const NamespaceSymbol *, const NamespaceSymbol *> compilation_namespaces_;

        mutable std::mutex binder_factory_mutex_;
        mutable std::unordered_map<const SyntaxTree *, const BinderFactory *> binder_factories_;
        mutable Lazy<const Binder &> root_binder_;

        // Old-stuff, subject to pruning as we refactor to the lazy model
        std::vector<Diagnostic> diagnostics_;
        SemanticMappings semantic_mappings_;
        BoundNodeLookup bound_node_lookup_;

        mutable std::mutex semantic_models_mutex_;
        mutable std::unordered_map<const SyntaxTree *, SemanticModel *> semantic_models_;

        mutable std::mutex error_type_mutex_;
        mutable std::unordered_map<SymbolLookupKey, const NamedTypeSymbol *> error_types_;

        mutable std::mutex error_namespace_mutex_;
        mutable std::unordered_map<SymbolLookupKey, const NamespaceSymbol *> error_namespaces_;

        mutable std::mutex variable_initializer_mutex_;
        mutable std::unordered_map<const VariableSymbol *, const BoundExpression *> variable_initializers_;

        mutable std::mutex function_body_mutex_;
        mutable std::unordered_map<const FunctionSymbol *, const BoundStatement *> function_bodies_;
    };

    struct CompilationInternal
    {
        [[nodiscard]] static inline std::strong_ordering compare_source_locations(const Compilation &compilation,
                                                                                  const Location &lhs,
                                                                                  const Location &rhs)
        {
            return compilation.compare_source_locations(lhs, rhs);
        }

        [[nodiscard]] static inline std::strong_ordering compare_source_locations(const Compilation &compilation,
                                                                                  const SourceLocation &lhs,
                                                                                  const SourceLocation &rhs)
        {
            return compilation.compare_source_locations(lhs, rhs);
        }

        [[nodiscard]] static inline std::strong_ordering compare_source_locations(const Compilation &compilation,
                                                                                  const SyntaxReference &lhs,
                                                                                  const SyntaxReference &rhs)
        {
            return compilation.compare_source_locations(lhs, rhs);
        }

        [[nodiscard]] static inline std::strong_ordering compare_source_locations(const Compilation &compilation,
                                                                                  const SyntaxNode &lhs,
                                                                                  const SyntaxNode &rhs)
        {
            return compilation.compare_source_locations(lhs, rhs);
        }

        [[nodiscard]] static inline std::strong_ordering compare_syntax_tree_ordering(const Compilation &compilation,
                                                                                      const SyntaxTree &lhs,
                                                                                      const SyntaxTree &rhs)
        {
            return compilation.compare_syntax_tree_ordering(lhs, rhs);
        }

        [[nodiscard]] static inline std::uint32_t get_syntax_tree_ordinal(const Compilation &compilation,
                                                                          const SyntaxTree &tree)
        {
            return compilation.get_syntax_tree_ordinal(tree);
        }

        [[nodiscard]] static inline const SemanticMappings &get_semantic_mappings(const Compilation &compilation)
        {
            return compilation.semantic_mappings_;
        }

        [[nodiscard]] static inline SemanticLifetime &get_lifetime(const Compilation &compilation)
        {
            return compilation.lifetime_;
        }

        [[nodiscard]] static inline const MergedNamespaceDeclaration &merged_root_declaration(
            const Compilation &compilation)
        {
            return compilation.merged_root_declaration();
        }

        [[nodiscard]] static inline const BinderFactory &get_binder_factory(const Compilation &compilation,
                                                                            const SyntaxTree &tree)
        {
            return compilation.get_binder_factory(tree);
        }

        [[nodiscard]] static inline const Binder &get_root_binder(const Compilation &compilation)
        {
            return compilation.root_binder();
        }
    };
} // namespace prism
