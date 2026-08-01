module prism.core:syntax.green.directives.impl;

import :syntax.lifetime;
import :syntax.green.directives;
import :syntax.directives;
import :syntax.green.names;

namespace prism
{
    GreenUsingDirective::GreenUsingDirective(GreenPtr<GreenToken> using_keyword,
                                             GreenPtr<GreenName> name,
                                             GreenPtr<GreenToken> semicolon,
                                             DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::using_directive, std::move(diagnostics)}, using_keyword_{std::move(using_keyword)},
          name_{std::move(name)}, semicolon_{std::move(semicolon)}
    {
        set_slot_count(3);
        adjust_flags_and_width(*using_keyword_);
        adjust_flags_and_width(*name_);
        adjust_flags_and_width(*semicolon_);
    }

    GreenUsingDirective::~GreenUsingDirective() = default;

    void GreenUsingDirective::set_using_keyword(GreenPtr<GreenToken> value) noexcept
    {
        using_keyword_ = std::move(value);
    }

    void GreenUsingDirective::set_name(GreenPtr<GreenName> value) noexcept
    {
        name_ = std::move(value);
    }

    void GreenUsingDirective::set_semicolon(GreenPtr<GreenToken> value) noexcept
    {
        semicolon_ = std::move(value);
    }

    Optional<const GreenNode &> GreenUsingDirective::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *using_keyword_;
            case 1:
                return *name_;
            case 2:
                return *semicolon_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenUsingDirective::create_red(SyntaxLifetime &lifetime,
                                                              const SyntaxNode *parent,
                                                              std::uint32_t position) const
    {
        return lifetime.add<UsingDirectiveSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenUsingDirective> GreenUsingDirective::with_using_keyword(
        GreenPtr<GreenToken> using_keyword) const
    {
        return update(std::move(using_keyword), name_, semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenUsingDirective> GreenUsingDirective::with_name(GreenPtr<GreenName> name) const
    {
        return update(using_keyword_, std::move(name), semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenUsingDirective> GreenUsingDirective::with_semicolon(
        GreenPtr<GreenToken> semicolon) const
    {
        return update(using_keyword_, name_, std::move(semicolon));
    }

    GreenPtr<GreenUsingDirective> GreenUsingDirective::update(GreenPtr<GreenToken> using_keyword,
                                                              GreenPtr<GreenName> name,
                                                              GreenPtr<GreenToken> semicolon) const
    {
        if (using_keyword == using_keyword_ && name == name_ && semicolon == semicolon_)
            return shared_from_this();

        return make_ref_counted<const GreenUsingDirective>(std::move(using_keyword),
                                                           std::move(name),
                                                           std::move(semicolon));
    }

    RefCountPtr<GreenNode> GreenUsingDirective::clone_internal() const
    {
        return make_ref_counted<GreenUsingDirective>(using_keyword_, name_, semicolon_);
    }
} // namespace prism
