/**
 * @file defaults.ixx
 * @author Francesco Corso
 * @date 6/21/2026
 * @brief Contains the functions that can be used to construct the default state of the lexer.
 */
module;

#include "prism/core/exports.h"

export module prism.core.lexer.defaults;

import std;
import prism.core.lexer;
import prism.core.lexer.token_matcher;

namespace prism
{
    PRISM_CORE_API export std::vector<std::unique_ptr<TokenMatcher>> default_matchers();

    PRISM_CORE_API export Lexer default_lexer();
} // namespace prism
