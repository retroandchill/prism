module prism.core:syntax.green.top_level.impl;

import :syntax.lifetime;
import :syntax.green.top_level;
import :syntax.top_level;
import :syntax.green.declarations;

namespace prism
{
    GreenCompilationUnit::GreenCompilationUnit(GreenSyntaxList<GreenDeclaration> members,
                                               DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::compilation_unit, std::move(diagnostics)}, members_{std::move(members)}
    {
        set_slot_count(1);
        adjust_flags_and_width(members_);
    }

    GreenCompilationUnit::~GreenCompilationUnit() = default;

    void GreenCompilationUnit::set_members(GreenSyntaxList<GreenDeclaration> value) noexcept
    {
        members_ = std::move(value);
    }

    Optional<const GreenNode &> GreenCompilationUnit::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
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

    [[nodiscard]] GreenPtr<GreenCompilationUnit> GreenCompilationUnit::with_members(
        GreenSyntaxList<GreenDeclaration> members) const
    {
        return update(std::move(members));
    }

    GreenPtr<GreenCompilationUnit> GreenCompilationUnit::update(GreenSyntaxList<GreenDeclaration> members) const
    {
        if (members == members_)
            return shared_from_this();

        return make_ref_counted<const GreenCompilationUnit>(std::move(members));
    }

    RefCountPtr<GreenNode> GreenCompilationUnit::clone_internal() const
    {
        return make_ref_counted<GreenCompilationUnit>(members_);
    }
} // namespace prism
