module prism.core:syntax.top_level.impl;

import :syntax.top_level;
import :syntax.declarations;

namespace prism
{
    SyntaxList<DeclarationSyntax> CompilationUnitSyntax::members() const
    {
        const auto red = get_red(members_);
        return make_syntax_list<DeclarationSyntax>(red);
    }

    Optional<const SyntaxNode &> CompilationUnitSyntax::get_node_slot(const std::size_t index) const
    {
        return index == 0 ? get_red(members_) : std::nullopt;
    }

    Optional<const SyntaxNode &> CompilationUnitSyntax::get_cached_slot(const std::size_t index) const
    {
        return index == 0 ? Optional<const SyntaxNode &>{members_.load(std::memory_order_acquire)} : std::nullopt;
    }
} // namespace prism
