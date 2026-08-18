/**
 * @file block_binder.ixx
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */

export module prism.core:binder.block_binder;

import :binder.local_scope_binder;

namespace prism
{
    class BlockSyntax;

    class BlockBinder final : public LocalScopeBinder
    {
      public:
        explicit BlockBinder(const Binder &enclosing, const BlockSyntax &block_syntax);

        [[nodiscard]] Optional<const SyntaxNode &> scope_designator() const override;

        [[nodiscard]] VariablesSpan get_declared_local_variables_for_scope(const SyntaxNode &designator) const override;

      protected:
        [[nodiscard]] std::span<Ref<const VariableSymbol>> build_local_variables_impl() const override;

      private:
        const BlockSyntax &block_syntax_;
    };
} // namespace prism
