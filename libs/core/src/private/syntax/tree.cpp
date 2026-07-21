/**
 * @file tree.cpp
 * @author Francesco Corso
 * @date 7/20/2026
 * @brief
 */
module prism.core:syntax.tree.impl;

import :syntax.tree;

namespace prism
{

    SyntaxTree::SyntaxTree(std::shared_ptr<SourceText> text, const GreenNode &root)
        : text_{std::move(text)}, root_{&root.create_red(lifetime_)}
    {
    }
} // namespace prism
