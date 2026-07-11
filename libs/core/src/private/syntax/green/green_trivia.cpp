/**
 * @file green_trivia.cpp
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
module prism.core:syntax.green.green_trivia.impl;

import :syntax.green.green_trivia;

namespace prism
{
    const GreenPtr<GreenTrivia> GreenTrivia::carriage_return_line_feed =
        make_ref_counted<GreenTrivia>(SyntaxKind::new_line_trivia, "\r\n");
    const GreenPtr<GreenTrivia> GreenTrivia::carriage_return =
        make_ref_counted<GreenTrivia>(SyntaxKind::new_line_trivia, "\r");
    const GreenPtr<GreenTrivia> GreenTrivia::line_feed =
        make_ref_counted<GreenTrivia>(SyntaxKind::new_line_trivia, "\n");
} // namespace prism
