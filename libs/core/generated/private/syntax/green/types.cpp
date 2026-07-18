module prism.core:syntax.green.types.impl;

import :syntax.green.types;
import :syntax.green.clauses;
import :syntax.green.declaration;
import :syntax.green.expressions;
import :syntax.green.statement;

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
