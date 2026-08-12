module prism.core:syntax.structured_trivia.impl;

import :syntax.structured_trivia;

namespace prism
{
    SyntaxTokenList SkippedTokensTriviaSyntax::tokens() const
    {
        return SyntaxTokenList{this, static_cast<const GreenSkippedTokensTrivia &>(green()).tokens(), position()};
    }

    Optional<const SyntaxNode &> SkippedTokensTriviaSyntax::get_node_slot(const std::size_t) const
    {
        return std::nullopt;
    }

    Optional<const SyntaxNode &> SkippedTokensTriviaSyntax::get_cached_slot(const std::size_t) const
    {
        return std::nullopt;
    }
} // namespace prism
