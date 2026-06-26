/**
 * @file lexer.ixx
 * @author Francesco Corso
 * @date 6/20/2026
 * @brief Defines the lexer class that holds the state of the parse
 */
module;

#include "prism/core/exports.h"

export module prism.core.lexer;

import std;
import prism.core.lexer.text_cursor;
import prism.core.lexer.token;
import prism.core.source.source_file;

namespace prism
{
    export class PRISM_CORE_API Lexer
    {
      public:
        constexpr explicit Lexer(const SourceFile &source_file) : cursor_{source_file.text()}
        {
        }

        Token next();

      private:
        Token make_eof() const;
        void skip_whitespace();

        std::optional<Token> match_comment();
        void skip_line_comment();
        std::optional<Token> skip_block_comment();

        std::optional<Token> match_identifier_or_keyword();
        std::optional<Token> match_number();
        std::optional<Token> match_operator();
        std::optional<Token> match_string_literal();

        TextCursor cursor_;
    };
} // namespace prism
