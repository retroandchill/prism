/**
 * @file node.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core:syntax.node;

import :syntax.green.node;
import :util.noncopyable;
import :text.text_span;

namespace prism
{
    class SyntaxTree;
    class SyntaxLifetime;

    export class PRISM_CORE_API SyntaxNode : NonCopyable
    {
      protected:
        constexpr SyntaxNode(const GreenNode &node, const SyntaxTree &tree, const std::uint32_t position)
            : green_{&node}, tree_{&tree}, position_{position}
        {
        }

        constexpr SyntaxNode(const GreenNode &node, const SyntaxNode &parent, const std::uint32_t position)
            : green_{&node}, parent_{&parent}, position_{position}
        {
        }

        constexpr ~SyntaxNode() = default;

      public:
        [[nodiscard]] constexpr SyntaxKind kind() const noexcept
        {
            return green_->kind();
        }

        [[nodiscard]] constexpr TextSpan full_span() const noexcept
        {
            return {.start = position_, .length = green_->full_width()};
        }

        [[nodiscard]] constexpr TextSpan span() const
        {
            return {.start = position_ + green_->leading_trivia_width(), .length = green_->width()};
        }

        [[nodiscard]] Optional<const SyntaxNode &> parent() const noexcept
        {
            return parent_;
        }

        [[nodiscard]] const SyntaxTree &tree() const;

      protected:
        [[nodiscard]] const SyntaxLifetime &lifetime() const;

      private:
        static const SyntaxTree *compute_tree(const SyntaxNode *node);

        const GreenNode *green_;
        const SyntaxNode *parent_ = nullptr;
        mutable std::atomic<const SyntaxTree *> tree_ = nullptr;
        std::uint32_t position_;
    };
} // namespace prism
