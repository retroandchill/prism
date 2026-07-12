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
    export class SyntaxTree : NonCopyable
    {
      public:
        virtual ~SyntaxTree() = default;

        virtual const SourceText &text() const = 0;
        virtual const SyntaxNode &root() const = 0;
    };
} // namespace prism
