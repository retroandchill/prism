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
    namespace
    {
        GreenTriviaList empty_trivia_list{};
    }

    const GreenPtr<GreenTrivia> &GreenTrivia::carriage_return_line_feed()
    {
        static auto instance = make_ref_counted<const GreenTrivia>(SyntaxKind::new_line_trivia, "\r\n");
        return instance;
    }

    const GreenPtr<GreenTrivia> &GreenTrivia::carriage_return()
    {
        static auto instance = make_ref_counted<const GreenTrivia>(SyntaxKind::new_line_trivia, "\r");
        return instance;
    }

    const GreenPtr<GreenTrivia> &GreenTrivia::line_feed()
    {
        static auto instance = make_ref_counted<const GreenTrivia>(SyntaxKind::new_line_trivia, "\n");
        return instance;
    }

    void GreenTrivia::write_to(TextWriter &writer) const
    {
        writer.write(text());
    }

    RefCountPtr<GreenNode> GreenTrivia::clone_internal() const
    {
        return make_ref_counted<GreenTrivia>(kind(), text());
    }
} // namespace prism
