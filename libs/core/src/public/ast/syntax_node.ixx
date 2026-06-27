/**
 * @file syntax_node.ixx
 * @author Francesco Corso
 * @date 6/27/2026
 * @brief
 */
export module prism.core.ast.syntax_node;

import std;
import prism.core.source.source_file;

namespace prism
{
    export class SyntaxNode
    {
      public:
      private:
        SourceRange range_;
    };
} // namespace prism
