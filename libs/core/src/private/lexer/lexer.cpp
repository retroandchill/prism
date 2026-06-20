/**
 * @file lexer.cpp
 * @author Francesco Corso
 * @date 6/20/2026
 * @brief Implementation of the lexer module.
 */
module prism.core.lexer;

namespace prism
{
    namespace
    {
        TokenKind get_identifier_kind(const std::string_view identifier)
        {
            if (identifier == "var")
                return TokenKind::kw_var;
            if (identifier == "func")
                return TokenKind::kw_func;
            if (identifier == "extern")
                return TokenKind::kw_extern;
            if (identifier == "return")
                return TokenKind::kw_return;

            return TokenKind::identifier;
        }
    } // namespace

    Token Lexer::next_token()
    {
        skip_whitespace();

        if (cursor_.at_end())
            return make_token(TokenKind::eof);

        auto current = cursor_.current();
        if (std::isalpha(current) || current == '_')
            return make_identifier();

        if (std::isdigit(current))
            return make_number();

        TokenKind kind{};
        switch (current)
        {
            case '{':
                kind = TokenKind::lbrace;
                break;
            case '}':
                kind = TokenKind::rbrace;
                break;
            case '[':
                kind = TokenKind::lbracket;
                break;
            case ']':
                kind = TokenKind::rbracket;
                break;
            case '(':
                kind = TokenKind::lparen;
                break;
            case ')':
                kind = TokenKind::rparen;
                break;
            case ',':
                kind = TokenKind::comma;
                break;
            case ';':
                kind = TokenKind::semicolon;
                break;
            case ':':
                kind = TokenKind::colon;
                break;
            case '.':
                kind = TokenKind::dot;
                break;
            case '=':
                kind = TokenKind::equal;
                break;
            case '+':
                kind = TokenKind::plus;
                break;
            case '-':
                kind = TokenKind::minus;
                break;
            case '*':
                kind = TokenKind::star;
                break;
            case '/':
                kind = TokenKind::slash;
                break;
            case '%':
                kind = TokenKind::percent;
                break;
            case '&':
                kind = TokenKind::ampersand;
                break;
            case '|':
                kind = TokenKind::pipe;
                break;
            case '^':
                kind = TokenKind::caret;
                break;
            case '?':
                kind = TokenKind::question;
                break;
            case '!':
                kind = TokenKind::exclamation;
                break;
            default:
                kind = TokenKind::unrecognized;
                break;
        }

        const auto token = make_token(kind, cursor_.position(), cursor_.position() + 1);
        cursor_.advance();
        return token;
    }

    void Lexer::skip_whitespace()
    {
        while (!cursor_.at_end())
        {
            if (std::isspace(cursor_.current()))
                cursor_.advance();
            else
                break;
        }
    }

    Token Lexer::make_identifier()
    {
        const auto start = cursor_.position();
        cursor_.advance();
        while (!cursor_.at_end())
        {
            if (std::isalnum(cursor_.current()) || cursor_.current() == '_')
                cursor_.advance();
            else
                break;
        }

        const auto identifier = std::string_view(source_file_.text()).substr(start, cursor_.position() - start);
        return make_token(get_identifier_kind(identifier), start, cursor_.position());
    }

    Token Lexer::make_number()
    {
        const auto start = cursor_.position();
        cursor_.advance();
        while (!cursor_.at_end())
        {
            if (std::isdigit(cursor_.current()))
                cursor_.advance();
            else
                break;
        }

        return make_token(TokenKind::number, start, cursor_.position());
    }
} // namespace prism
