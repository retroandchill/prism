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
        constexpr explicit Lexer(const SourceFile &source_file) : source_file_(source_file), cursor_(source_file.text())
        {
        }

        Token next_token();

      private:
        void skip_whitespace();
        Token make_identifier();
        Token make_number();

        const SourceFile &source_file_;
        TextCursor cursor_;
    };
} // namespace prism
