/**
 * @file green_trivia_list.cpp
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
module prism.core:syntax.green.green_trivia_list.impl;

import :syntax.green.green_trivia_list;

namespace prism
{
    std::uint32_t GreenTriviaList::calculate_width(const std::vector<RefCountPtr<const GreenTrivia>> &trivia)
    {
        return std::ranges::fold_left(trivia,
                                      0u,
                                      [](const std::uint32_t acc, const auto &t) { return acc + t->full_width(); });
    }

    RefCountPtr<const GreenTriviaList> normalize_trivia(RefCountPtr<const GreenTriviaList> trivia)
    {
        if (trivia == nullptr || trivia->empty())
            return nullptr;

        return std::move(trivia);
    }
} // namespace prism
