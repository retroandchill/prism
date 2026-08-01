module prism.core:syntax.green.top_level.impl;

import :syntax.lifetime;
import :syntax.green.top_level;
import :syntax.top_level;
import :syntax.green.declarations;
import :syntax.green.directives;

namespace prism
{
    GreenCompilationUnit::GreenCompilationUnit(GreenSyntaxList<GreenUsingDirective> usings,
                                               GreenSyntaxList<GreenDeclaration> members,
                                               DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::compilation_unit, std::move(diagnostics)}, usings_{std::move(usings)},
          members_{std::move(members)}
    {
        set_slot_count(2);
        adjust_flags_and_width(usings_);
        adjust_flags_and_width(members_);
    }

    GreenCompilationUnit::~GreenCompilationUnit() = default;

    void GreenCompilationUnit::set_usings(GreenSyntaxList<GreenUsingDirective> value) noexcept
    {
        usings_ = std::move(value);
    }

    void GreenCompilationUnit::set_members(GreenSyntaxList<GreenDeclaration> value) noexcept
    {
        members_ = std::move(value);
    }

    Optional<const GreenNode &> GreenCompilationUnit::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return usings_.node();
            case 1:
                return members_.node();
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenCompilationUnit::create_red(SyntaxLifetime &lifetime,
                                                               const SyntaxNode *parent,
                                                               std::uint32_t position) const
    {
        return lifetime.add<CompilationUnitSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenCompilationUnit> GreenCompilationUnit::with_usings(
        GreenSyntaxList<GreenUsingDirective> usings) const
    {
        return update(std::move(usings), members_);
    }

    [[nodiscard]] GreenPtr<GreenCompilationUnit> GreenCompilationUnit::with_members(
        GreenSyntaxList<GreenDeclaration> members) const
    {
        return update(usings_, std::move(members));
    }

    GreenPtr<GreenCompilationUnit> GreenCompilationUnit::update(GreenSyntaxList<GreenUsingDirective> usings,
                                                                GreenSyntaxList<GreenDeclaration> members) const
    {
        if (usings == usings_ && members == members_)
            return shared_from_this();

        return make_ref_counted<const GreenCompilationUnit>(std::move(usings), std::move(members));
    }

    RefCountPtr<GreenNode> GreenCompilationUnit::clone_internal() const
    {
        return make_ref_counted<GreenCompilationUnit>(usings_, members_);
    }
} // namespace prism
