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

namespace prism
{
    class NamedTypeSymbol;
    class AssemblySymbol;

    export class PRISM_CORE_API Compilation : NonCopyable
    {
        struct CreateTag
        {
        };

      public:
        Compilation(CreateTag, std::vector<std::unique_ptr<SyntaxTree>> trees) noexcept;

        static std::unique_ptr<Compilation> create(Name assembly_name, std::vector<std::unique_ptr<SyntaxTree>> trees);

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

        [[nodiscard]] SemanticModel get_semantic_model(const SyntaxTree &tree) const noexcept
        {
            return SemanticModel{*this, tree};
        }

        [[nodiscard]] constexpr const std::vector<Diagnostic> &diagnostics() const noexcept
        {
            return diagnostics_;
        }

        [[nodiscard]] const DeclarationScope &get_declaration_scope(const SyntaxNode &node) const;

        [[nodiscard]] const NamedTypeSymbol &get_special_type(SpecialType type) const;

      private:
        friend class MergedNamespaceSymbol;

        std::unique_ptr<SemanticLifetime> lifetime_ = std::make_unique<SemanticLifetime>();
        const AssemblySymbol *assembly_ = nullptr;
        const NamespaceSymbol *global_namespace_ = nullptr;
        std::vector<std::unique_ptr<SyntaxTree>> trees_;
        std::vector<Diagnostic> diagnostics_;
        SemanticMappings semantic_mappings_;
    };
} // namespace prism
