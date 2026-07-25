module prism.core:syntax.green.types.impl;

import :syntax.lifetime;
import :syntax.green.types;
import :syntax.types;

namespace prism
{
    GreenIdentifierNamedType::GreenIdentifierNamedType(GreenPtr<GreenToken> identifier, DiagnosticInfoList diagnostics)
        : GreenSimpleNamedType{SyntaxKind::identifier_named_type, std::move(diagnostics)},
          identifier_{std::move(identifier)}
    {
        set_child_count(1);
        adjust_flags_and_width(*identifier_);
    }

    GreenIdentifierNamedType::~GreenIdentifierNamedType() = default;

    void GreenIdentifierNamedType::set_identifier(GreenPtr<GreenToken> value) noexcept
    {
        identifier_ = std::move(value);
    }

    Optional<const GreenNode &> GreenIdentifierNamedType::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *identifier_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] const SyntaxNode &GreenIdentifierNamedType::create_red(SyntaxLifetime &lifetime,
                                                                         const SyntaxNode *parent,
                                                                         std::uint32_t position) const
    {
        return lifetime.add<IdentifierNamedTypeSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenIdentifierNamedType> GreenIdentifierNamedType::with_identifier(
        GreenPtr<GreenToken> identifier) const
    {
        return update(std::move(identifier));
    }

    GreenPtr<GreenIdentifierNamedType> GreenIdentifierNamedType::update(GreenPtr<GreenToken> identifier) const
    {
        if (identifier == identifier_)
            return shared_from_this();

        return make_ref_counted<const GreenIdentifierNamedType>(std::move(identifier));
    }

    RefCountPtr<GreenNode> GreenIdentifierNamedType::clone_internal() const
    {
        return make_ref_counted<GreenIdentifierNamedType>(identifier_);
    }
} // namespace prism
