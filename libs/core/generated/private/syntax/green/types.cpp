module prism.core:syntax.green.types.impl;

import :syntax.green.types;

namespace prism
{
    GreenIdentifierNamedType::GreenIdentifierNamedType(RefCountPtr<const GreenToken> identifier,
                                                       DiagnosticInfoList diagnostics)
        : GreenSimpleNamedType{SyntaxKind::identifier_named_type, std::move(diagnostics)},
          identifier_{std::move(identifier)}
    {
        set_child_count(1);
        adjust_flags_and_width(*identifier_);
    }

    GreenIdentifierNamedType::~GreenIdentifierNamedType() = default;

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

    [[nodiscard]] RefCountPtr<const GreenIdentifierNamedType> GreenIdentifierNamedType::with_identifier(
        RefCountPtr<const GreenToken> identifier) const
    {
        return update(std::move(identifier));
    }

    RefCountPtr<const GreenIdentifierNamedType> GreenIdentifierNamedType::update(
        RefCountPtr<const GreenToken> identifier) const
    {
        if (identifier == identifier_)
            return shared_from_this();

        return make_ref_counted<const GreenIdentifierNamedType>(std::move(identifier));
    }
} // namespace prism
