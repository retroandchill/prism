module prism.core:syntax.top_level.impl;

import :syntax.top_level;
import :syntax.declarations;
import :syntax.directives;

namespace prism
{
    SyntaxList<UsingDirectiveSyntax> CompilationUnitSyntax::usings() const
    {
        const auto red = get_red(usings_);
        return make_syntax_list<UsingDirectiveSyntax>(red);
    }

    SyntaxList<DeclarationSyntax> CompilationUnitSyntax::members() const
    {
        const auto red = get_red(members_, 1);
        return make_syntax_list<DeclarationSyntax>(red);
    }

    Optional<const SyntaxNode &> CompilationUnitSyntax::get_node_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return get_red(usings_);
            case 1:
                return get_red(members_, 1);
            default:
                return std::nullopt;
        }
    }

    Optional<const SyntaxNode &> CompilationUnitSyntax::get_cached_slot(const std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return usings_.try_get_value(nullptr);
            case 1:
                return members_.try_get_value(nullptr);
            default:
                return std::nullopt;
        }
    }
} // namespace prism
