module prism.core:syntax.green.clauses.impl;

import :syntax.green.clauses;
import :syntax.green.expression;
import :syntax.green.type;

namespace prism
{
    GreenInitializer::GreenInitializer(const SyntaxKind kind,
                                       GreenPtr<GreenToken> equal_sign,
                                       GreenPtr<GreenExpression> value,
                                       DiagnosticInfoList diagnostics)
        : GreenNode{kind, std::move(diagnostics)}, equal_sign_{std::move(equal_sign)}, value_{std::move(value)}
    {
        set_child_count(2);
        adjust_flags_and_width(*equal_sign_);
        adjust_flags_and_width(*value_);
    }
    GreenInitializer::~GreenInitializer() = default;

    Optional<const GreenNode &> GreenInitializer::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *equal_sign_;
            case 1:
                return *value_;
            default:
                return std::nullopt;
        }
    }

    GreenTypeHint::GreenTypeHint(const SyntaxKind kind,
                                 GreenPtr<GreenToken> colon,
                                 GreenPtr<GreenType> type,
                                 DiagnosticInfoList diagnostics)
        : GreenNode{kind, std::move(diagnostics)}, colon_{std::move(colon)}, type_{std::move(type)}
    {
        set_child_count(2);
        adjust_flags_and_width(*colon_);
        adjust_flags_and_width(*type_);
    }
    GreenTypeHint::~GreenTypeHint() = default;

    Optional<const GreenNode &> GreenTypeHint::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *colon_;
            case 1:
                return *type_;
            default:
                return std::nullopt;
        }
    }

} // namespace prism
