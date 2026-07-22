/**
 * @file tree.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
export module prism.core:syntax.tree;

import :text.source_file;
import :syntax.green.node;
import :util.noncopyable;
import :syntax.node;
import :memory.persistent_allocator;
import :syntax.lifetime;

namespace prism
{
    export class SyntaxTree final : NonCopyable
    {
      public:
        SyntaxTree(std::shared_ptr<SourceText> text, GreenPtr<GreenNode> root);

        constexpr const SourceText &text() const noexcept
        {
            return *text_;
        }

        constexpr const SyntaxNode &root() const noexcept
        {
            return *root_;
        }

      private:
        friend SyntaxNode;

        std::shared_ptr<SourceText> text_;
        std::unique_ptr<SyntaxLifetime> lifetime_ = std::make_unique<SyntaxLifetime>();
        const SyntaxNode *root_ = nullptr;
    };
} // namespace prism
