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

namespace prism
{
    export class SyntaxTree final : public std::enable_shared_from_this<SyntaxTree>
    {
        struct NoClone
        {
        };

        static constexpr NoClone no_clone;

      public:
        constexpr SyntaxTree(NoClone, RefCountPtr<const SyntaxNode> root) noexcept : root_{std::move(root)}
        {
        }

        [[nodiscard]] constexpr Optional<const SourceText &> text() const noexcept
        {
            return *text_;
        }

        [[nodiscard]] constexpr const SyntaxNode &root() const noexcept
        {
            return *root_;
        }

      private:
        friend SyntaxNode;

        std::shared_ptr<SourceText> text_;
        RefCountPtr<const SyntaxNode> root_;
    };
} // namespace prism
