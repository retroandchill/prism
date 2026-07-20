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

    export class PRISM_CORE_API SyntaxNode : public IntrusiveRefCounted
    {
      protected:
        SyntaxNode(RefCountPtr<const GreenNode> node, const SyntaxNode *parent, const std::uint32_t position)
            : green_{std::move(node)}, parent_{parent}, position_{position}
        {
        }

        SyntaxNode(RefCountPtr<const GreenNode> node,
                   const std::uint32_t position,
                   const std::shared_ptr<const SyntaxTree> &tree)
            : green_{std::move(node)}, tree_{tree}, position_{position}
        {
        }

      public:
        virtual ~SyntaxNode() = default;

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

        [[nodiscard]] constexpr Optional<const SyntaxNode &> parent() const noexcept
        {
            return parent_;
        }

        [[nodiscard]] std::shared_ptr<const SyntaxTree> syntax_tree() const noexcept;

      private:
        static std::shared_ptr<const SyntaxTree> compute_tree(const SyntaxNode *node);

        RefCountPtr<const GreenNode> green_;
        const SyntaxNode *parent_ = nullptr;
        mutable std::atomic<std::weak_ptr<const SyntaxTree>> tree_;
        std::uint32_t position_;
    };
} // namespace prism
