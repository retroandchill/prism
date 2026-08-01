module prism.core:syntax.green.types.impl;

import :syntax.lifetime;
import :syntax.green.types;
import :syntax.types;
import :syntax.green.names;

namespace prism
{
    GreenPredefinedType::GreenPredefinedType(GreenPtr<GreenToken> keyword, DiagnosticInfoList diagnostics)
        : GreenType{SyntaxKind::predefined_type, std::move(diagnostics)}, keyword_{std::move(keyword)}
    {
        set_slot_count(1);
        adjust_flags_and_width(*keyword_);
    }

    GreenPredefinedType::~GreenPredefinedType() = default;

    void GreenPredefinedType::set_keyword(GreenPtr<GreenToken> value) noexcept
    {
        keyword_ = std::move(value);
    }

    Optional<const GreenNode &> GreenPredefinedType::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *keyword_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenPredefinedType::create_red(SyntaxLifetime &lifetime,
                                                              const SyntaxNode *parent,
                                                              std::uint32_t position) const
    {
        return lifetime.add<PredefinedTypeSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenPredefinedType> GreenPredefinedType::with_keyword(GreenPtr<GreenToken> keyword) const
    {
        return update(std::move(keyword));
    }

    GreenPtr<GreenPredefinedType> GreenPredefinedType::update(GreenPtr<GreenToken> keyword) const
    {
        if (keyword == keyword_)
            return shared_from_this();

        return make_ref_counted<const GreenPredefinedType>(std::move(keyword));
    }

    RefCountPtr<GreenNode> GreenPredefinedType::clone_internal() const
    {
        return make_ref_counted<GreenPredefinedType>(keyword_);
    }

    GreenNamedType::GreenNamedType(GreenPtr<GreenName> identifier, DiagnosticInfoList diagnostics)
        : GreenType{SyntaxKind::named_type, std::move(diagnostics)}, identifier_{std::move(identifier)}
    {
        set_slot_count(1);
        adjust_flags_and_width(*identifier_);
    }

    GreenNamedType::~GreenNamedType() = default;

    void GreenNamedType::set_identifier(GreenPtr<GreenName> value) noexcept
    {
        identifier_ = std::move(value);
    }

    Optional<const GreenNode &> GreenNamedType::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *identifier_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenNamedType::create_red(SyntaxLifetime &lifetime,
                                                         const SyntaxNode *parent,
                                                         std::uint32_t position) const
    {
        return lifetime.add<NamedTypeSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenNamedType> GreenNamedType::with_identifier(GreenPtr<GreenName> identifier) const
    {
        return update(std::move(identifier));
    }

    GreenPtr<GreenNamedType> GreenNamedType::update(GreenPtr<GreenName> identifier) const
    {
        if (identifier == identifier_)
            return shared_from_this();

        return make_ref_counted<const GreenNamedType>(std::move(identifier));
    }

    RefCountPtr<GreenNode> GreenNamedType::clone_internal() const
    {
        return make_ref_counted<GreenNamedType>(identifier_);
    }
} // namespace prism
