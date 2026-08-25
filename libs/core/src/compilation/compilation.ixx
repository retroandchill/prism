/**
 * @file compilation.ixx
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

#include <libassert/assert-macros.hpp>

export module prism.core:compilation;

import :util.noncopyable;
import :syntax.tree;
import :semantic.semantic_lifetime;
import :semantic.semantic_model;
import :symbols.type_symbol;
import :context.compilation_settings;
import :semantic.syntax_and_declaration_manager;
import :diagnostics.diagnostic_bag;
import :util.function_ref;

namespace prism
{
    class Binder;
    class BinderFactory;
    class BoundStatement;
    class BoundExpression;
    export class NamedTypeSymbol;
    export class AssemblySymbol;

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
    export struct EmitResult
    {
        bool success = false;
        ImmutableArray<Diagnostic> diagnostics{};
    };

    enum class CompilationStage : std::uint8_t
    {
        parse,
        declare,
        compile
    };

    struct EntryPoint
    {
        Optional<const FunctionSymbol &> function_symbol{};
        ImmutableArray<Diagnostic> diagnostics{};
    };

    export class PRISM_CORE_API Compilation final : NonCopyable
    {
        struct CreateTag
        {
        };

        class Cache;

      public:
        Compilation(CreateTag,
                    SemanticLifetime &lifetime,
                    Name assembly_name,
                    CompilationSettings target_settings,
                    RefCountPtr<SyntaxAndDeclarationManager> syntax_and_declarations) noexcept;

        static std::shared_ptr<Compilation> create(Name assembly_name,
                                                   ImmutableArray<std::shared_ptr<const SyntaxTree>> trees = {},
                                                   CompilationSettings target_settings = {});

        [[nodiscard]] constexpr const CompilationSettings &target_settings() const noexcept
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

        [[nodiscard]] const NamedTypeSymbol &get_special_type(SpecialType type) const;

        [[nodiscard]] const NamedTypeSymbol &create_error_type_symbol(Optional<const Symbol &> container,
                                                                      Name name) const;

        [[nodiscard]] const NamespaceSymbol &create_error_namespace_symbol(Optional<const NamespaceSymbol &> container,
                                                                           Name name) const;

        [[nodiscard]] EmitResult emit(std::filesystem::path output_directory) const;

        [[nodiscard]] Optional<const FunctionSymbol &> get_entry_point() const;

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

        [[nodiscard]] const ImmutableArray<Ref<const VariableSymbol>> &get_global_variables() const;
        [[nodiscard]] const ImmutableArray<Ref<const FunctionSymbol>> &get_global_functions() const;

        [[nodiscard]] Optional<const BoundExpression &> get_bound_initializer(const VariableSymbol &symbol) const;
        [[nodiscard]] Optional<const BoundStatement &> get_bound_body(const FunctionSymbol &symbol) const;

        [[nodiscard]] const EntryPoint &get_entry_point_and_diagnostics() const;
        [[nodiscard]] EntryPoint compute_entry_point() const;
        [[nodiscard]] Optional<const FunctionSymbol &> find_entry_point(DiagnosticBag &diagnostics) const;
        static void append_entry_points(const NamespaceSymbol &ns,
                                        PooledVector<Ref<const FunctionSymbol>> &entry_points,
                                        DiagnosticBag &diagnostics);
        [[nodiscard]] static bool is_valid_entry_point(const FunctionSymbol &entry_point, DiagnosticBag &diagnostics);

        friend struct CompilationInternal;

        SemanticLifetime &lifetime_;
        Name assembly_name_;
        CompilationSettings target_settings_;
        RefCountPtr<SyntaxAndDeclarationManager> syntax_and_declaration_manager_;
        mutable Lazy<const AssemblySymbol &> assembly_;
        mutable Lazy<const NamespaceSymbol &> global_namespace_;
        mutable Lazy<EntryPoint> entry_point_;
        mutable DiagnosticBag declaration_diagnostics_;
        Cache &cache_;
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

        [[nodiscard]] static inline DiagnosticBag &get_declaration_diagnostics(const Compilation &compilation)
        {
            return compilation.declaration_diagnostics_;
        }

        [[nodiscard]] static inline const ImmutableArray<Ref<const VariableSymbol>> &get_global_variables(
            const Compilation &compilation)
        {
            return compilation.get_global_variables();
        }

        [[nodiscard]] static inline const ImmutableArray<Ref<const FunctionSymbol>> &get_functions(
            const Compilation &compilation)
        {
            return compilation.get_global_functions();
        }

        [[nodiscard]] static inline Optional<const BoundExpression &> get_bound_initializer(
            const Compilation &compilation,
            const VariableSymbol &symbol)
        {
            return compilation.get_bound_initializer(symbol);
        }

        [[nodiscard]] static inline Optional<const BoundStatement &> get_bound_body(const Compilation &compilation,
                                                                                    const FunctionSymbol &symbol)
        {
            return compilation.get_bound_body(symbol);
        }
    };
} // namespace prism
