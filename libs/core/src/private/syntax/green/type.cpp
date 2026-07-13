module prism.core:syntax.green.type.impl;

import :syntax.green.type;

namespace prism
{
    GreenType::~GreenType() = default;

    GreenNamedType::~GreenNamedType() = default;

    GreenSimpleNamedType::~GreenSimpleNamedType() = default;

    GreenIdentifierNamedType::GreenIdentifierNamedType(const SyntaxKind kind,
                                                       GreenPtr<GreenToken> identifier,
                                                       DiagnosticInfoList diagnostics)
        : GreenSimpleNamedType{kind, std::move(diagnostics)}, identifier_{std::move(identifier)}
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

} // namespace prism
