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

namespace prism
{
    export class SyntaxTree final : NonCopyable
    {
      public:
        constexpr const SourceText &text() const noexcept
        {
            return *text_;
        }

        constexpr const SyntaxNode &root() const noexcept
        {
            return *root_;
        }

      private:
        std::shared_ptr<SourceText> text_;
        GreenPtr<GreenNode> green_root_;
        PersistentAllocator allocator_;
        const SyntaxNode *root_ = nullptr;
    };
} // namespace prism
