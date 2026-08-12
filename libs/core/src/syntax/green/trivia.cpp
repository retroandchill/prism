/**
 * @file trivia.cpp
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
module prism.core:syntax.green.trivia.impl;

import :syntax.green.trivia;
import :diagnostics.info;

namespace prism
{
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

    GreenStructuredTrivia::GreenStructuredTrivia(const SyntaxKind kind, DiagnosticInfoList diagnostics)
        : GreenNode(kind, std::move(diagnostics))
    {
        set_flags(SyntaxFlags::contains_structured_trivia);

        if (kind == SyntaxKind::skipped_tokens_trivia)
        {
            set_flags(SyntaxFlags::contains_skipped_text);
        }
    }
} // namespace prism
