/**
 * @file lexer.cpp
 * @author Francesco Corso
 * @date 6/20/2026
 * @brief Implementation of the lexer module.
 */
module prism.core.lexer;

namespace prism
{
    void Lexer::add_matcher(std::unique_ptr<TokenMatcher> matcher)
    {
        const auto it = std::ranges::lower_bound(matchers_, matcher, priority_less);
        matchers_.insert(it, std::move(matcher));
    }

    std::vector<Token> Lexer::lex(const SourceFile &source_file) const
    {
        std::vector<Token> tokens;
        TextCursor cursor{source_file.text()};
        while (true)
        {

            cursor.skip_whitespace();

            if (cursor.at_end())
                break;

            auto token_found = false;
            for (const auto &matcher : matchers_)
            {
                if (const auto token = matcher->try_match(cursor); token.has_value())
                {
                    tokens.push_back(*token);
                    token_found = true;
                    break;
                }
            }

            if (token_found)
                continue;

            // If none of the matchers matched, add an unrecognized token, so we just skip it
            tokens.emplace_back(TokenKind::unrecognized, SourceRange{cursor.position(), cursor.position() + 1});
            cursor.advance();
        }

        tokens.emplace_back(TokenKind::eof);
        return tokens;
    }
} // namespace prism
