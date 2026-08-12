/**
 * @file view.cpp
 * @author Francesco Corso
 * @date 7/26/2026
 * @brief
 */
module prism.core:syntax.green.view.impl;

import :syntax.green.view;

namespace prism
{

    GreenNodeIterator::GreenNodeIterator(const GreenNode &node) : current_{&node}
    {
        const auto view = node.child_nodes_and_tokens();
        stack_.emplace_back(view.begin(), view.end());
    }

    GreenNodeIterator &GreenNodeIterator::operator++()
    {
        while (!stack_.empty())
        {
            auto &[it, sentinel] = stack_.back();
            if (it == sentinel)
            {
                stack_.pop_back();
                continue;
            }

            ++it;
            current_ = std::addressof(*it);

            if (!current_->is_token())
            {
                auto view = current_->child_nodes_and_tokens();
                stack_.emplace_back(view.begin(), view.end());
            }
            return *this;
        }

        current_ = nullptr;
        return *this;
    }
} // namespace prism
