/**
 * @file tree.cpp
 * @author Francesco Corso
 * @date 7/20/2026
 * @brief
 */
module prism.core:syntax.tree.impl;

import :syntax.tree;
import :parser.language_parser;

namespace prism
{
    SyntaxTree::SyntaxTree(std::shared_ptr<SourceText> text, GreenPtr<GreenNode> root)
        : text_{std::move(text)}, root_{&root->create_red(*lifetime_)}
    {
        lifetime_->add_root(std::move(root));
        root_->tree_.store(this);
    }

    std::unique_ptr<SyntaxTree> SyntaxTree::parse(std::string text)
    {
        return parse(std::make_shared<SourceText>(std::move(text)));
    }

    std::unique_ptr<SyntaxTree> SyntaxTree::parse(std::shared_ptr<SourceText> text)
    {
        LanguageParser parser{text->text()};
        auto root = parser.parse_compilation_unit();
        return std::make_unique<SyntaxTree>(text, std::move(root));
    }
} // namespace prism
