/**
 * @file matchers.ixx
 * @author Francesco Corso
 * @date 6/21/2026
 * @brief Contains the declaration for the built-in matchers
 */
module;

#include "prism/core/exports.h"

export module prism.core.lexer.matchers;

import std;
import prism.core.lexer.token_matcher;
import prism.core.lexer.token;
import prism.core.lexer.text_cursor;

namespace prism
{
    export class PRISM_CORE_API CommentMatcher final : public TokenMatcher
    {
      public:
        // This forces CommentMatcher to alway be run first
        constexpr CommentMatcher() : TokenMatcher(std::numeric_limits<std::size_t>::min())
        {
        }

        [[nodiscard]] std::optional<Token> try_match(TextCursor &cursor) const override;
    };

    export class PRISM_CORE_API IdentifierMatcher final : public TokenMatcher
    {
      public:
        [[nodiscard]] std::optional<Token> try_match(TextCursor &cursor) const override;
    };

    export class PRISM_CORE_API NumberMatcher final : public TokenMatcher
    {
      public:
        [[nodiscard]] std::optional<Token> try_match(TextCursor &cursor) const override;
    };

    export class PRISM_CORE_API OperatorMatcher final : public TokenMatcher
    {
      public:
        [[nodiscard]] std::optional<Token> try_match(TextCursor &cursor) const override;
    };
} // namespace prism
