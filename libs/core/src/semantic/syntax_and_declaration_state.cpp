/**
 * @file syntax_and_declaration_state.cpp
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
module prism.core:semantic.syntax_and_declaration_state.impl;

import :semantic.syntax_and_declaration_state;

namespace prism
{
    const ImmutableArray<DeclarationRecord> &SyntaxAndDeclarationState::declarations() const noexcept
    {
        return declarations_.get_or_compute(
            [this]
            {
                return syntax_trees_ |
                       std::views::transform([](const auto &tree) { return DeclarationBinder{*tree}.bind(); }) |
                       std::views::join | std::ranges::to<ImmutableArray>();
            });
    }
} // namespace prism
