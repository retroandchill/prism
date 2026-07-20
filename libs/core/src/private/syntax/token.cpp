/**
 * @file token.cpp
 * @author Francesco Corso
 * @date 7/20/2026
 * @brief
 */
module prism.core:syntax.token.impl;

import :syntax.token;
import :syntax.node;

namespace prism
{

    std::shared_ptr<const SyntaxTree> SyntaxToken::syntax_tree() const noexcept
    {
        if (parent_ == nullptr)
            return nullptr;

        return parent_->syntax_tree();
    }
} // namespace prism
