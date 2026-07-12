/**
 * @file parsed_tree.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
export module prism.core:syntax.parsed_tree;

import :syntax.tree;

namespace prism
{
    class ParsedSyntaxTree final : SyntaxTree
    {
      public:
        constexpr const SourceText &text() const override
        {
            return *text_;
        }

        constexpr const SyntaxNode &root() const override
        {
            return *root_;
        }

      private:
        std::shared_ptr<SourceText> text_;
        GreenPtr<GreenNode> green_root_;
        std::unique_ptr<SyntaxNode> root_;
    };
} // namespace prism
