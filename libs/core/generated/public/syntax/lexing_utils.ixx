export module prism.core:syntax.lexing_utils;

import :syntax.green.token;
import :text.cursor;

namespace prism
{
    GreenPtr<GreenToken> get_static_green_token(SyntaxKind kind);

    Optional<SyntaxKind> match_keyword(std::string_view text);

    Optional<SyntaxKind> match_punctuation(TextCursor &cursor);
} // namespace prism
