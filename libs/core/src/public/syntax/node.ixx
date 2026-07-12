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
    export class PRISM_CORE_API SyntaxNode : NonCopyable
    {
        SyntaxNode(const GreenNode &node, const std::uint32_t position) : green_{&node}, position_{position}
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
            return {position_, green_->full_width()};
        }

        [[nodiscard]] constexpr TextSpan span() const
        {
            return {position_ + green_->leading_trivia_width(), green_->width()};
        }

      private:
        const GreenNode *green_;
        SyntaxNode *parent_ = nullptr;
        std::uint32_t position_;
    };
} // namespace prism
