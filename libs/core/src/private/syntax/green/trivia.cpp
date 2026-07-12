/**
 * @file green_trivia.cpp
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
module prism.core:syntax.green.trivia.impl;

import :syntax.green.trivia;

namespace prism
{
    const GreenPtr<GreenTrivia> GreenTrivia::carriage_return_line_feed =
        make_ref_counted<GreenTrivia>(SyntaxKind::new_line_trivia, "\r\n");
    const GreenPtr<GreenTrivia> GreenTrivia::carriage_return =
        make_ref_counted<GreenTrivia>(SyntaxKind::new_line_trivia, "\r");
    const GreenPtr<GreenTrivia> GreenTrivia::line_feed =
        make_ref_counted<GreenTrivia>(SyntaxKind::new_line_trivia, "\n");

    GreenPtr<GreenTriviaList> normalize_trivia(GreenPtr<GreenTriviaList> trivia)
    {
        if (trivia == nullptr || trivia->empty())
            return nullptr;

        return std::move(trivia);
    }
} // namespace prism
