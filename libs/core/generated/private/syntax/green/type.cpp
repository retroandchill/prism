module prism.core:syntax.green.type.impl;

import :syntax.green.type;
import :syntax.green.clauses;
import :syntax.green.declaration;
import :syntax.green.expression;
import :syntax.green.statement;

namespace prism
{
    GreenType::~GreenType() = default;

    GreenNamedType::~GreenNamedType() = default;

    GreenSimpleNamedType::~GreenSimpleNamedType() = default;

    GreenIdentifierNamedType::GreenIdentifierNamedType(GreenPtr<GreenToken> identifier, DiagnosticInfoList diagnostics)
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

} // namespace prism
