/**
 * @file syntax_parser.cpp
 * @author Francesco Corso
 * @date 7/24/2026
 * @brief
 */
module prism.core:parser.syntax_parser.impl;

import :parser.syntax_parser;

namespace prism
{

    const GreenToken &SyntaxParser::current_token()
    {
        return stream_.peek();
    }

    const GreenToken &SyntaxParser::peek_token(const int offset)
    {
        return stream_.peek(offset + 1);
    }

    GreenPtr<GreenToken> SyntaxParser::eat_token()
    {
        auto token = current_token().shared_from_this();
        stream_.advance();
        return token;
    }
} // namespace prism
