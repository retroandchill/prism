/**
 * @file syntax_and_declaration_state.ixx
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
export module prism.core:semantic.syntax_and_declaration_state;

import :collections.immutable_array;
import :binder.declaration_binder;

namespace prism
{
    class SyntaxTree;

    class SyntaxAndDeclarationState final
    {
      public:
        explicit constexpr SyntaxAndDeclarationState(
            ImmutableArray<std::shared_ptr<const SyntaxTree>> syntax_trees) noexcept
            : syntax_trees_{std::move(syntax_trees)}
        {
        }

        [[nodiscard]] const ImmutableArray<std::shared_ptr<const SyntaxTree>> &syntax_trees() const noexcept
        {
            return syntax_trees_;
        }

        [[nodiscard]] const ImmutableArray<DeclarationRecord> &declarations() const noexcept;

      private:
        ImmutableArray<std::shared_ptr<const SyntaxTree>> syntax_trees_;
        mutable Lazy<ImmutableArray<DeclarationRecord>> declarations_;
    };
} // namespace prism
