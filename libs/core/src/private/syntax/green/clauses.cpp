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

    GreenNamedParameter::GreenNamedParameter(const SyntaxKind kind,
                                             GreenPtr<GreenToken> name,
                                             GreenPtr<GreenToken> colon,
                                             DiagnosticInfoList diagnostics)
        : GreenNode{kind, std::move(diagnostics)}, name_{std::move(name)}, colon_{std::move(colon)}
    {
        set_child_count(2);
        adjust_flags_and_width(*name_);
        adjust_flags_and_width(*colon_);
    }
    GreenNamedParameter::~GreenNamedParameter() = default;

    Optional<const GreenNode &> GreenNamedParameter::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *name_;
            case 1:
                return *colon_;
            default:
                return std::nullopt;
        }
    }

    GreenArgumentList::GreenArgumentList(const SyntaxKind kind,
                                         GreenPtr<GreenToken> open_paren,
                                         GreenSeparatedList<GreenArgument> argument,
                                         GreenPtr<GreenToken> close_paren,
                                         DiagnosticInfoList diagnostics)
        : GreenNode{kind, std::move(diagnostics)}, open_paren_{std::move(open_paren)}, argument_{std::move(argument)},
          close_paren_{std::move(close_paren)}
    {
        set_child_count(3);
        adjust_flags_and_width(*open_paren_);
        adjust_flags_and_width(argument_);
        adjust_flags_and_width(*close_paren_);
    }
    GreenArgumentList::~GreenArgumentList() = default;

    Optional<const GreenNode &> GreenArgumentList::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *open_paren_;
            case 1:
                return argument_.node();
            case 2:
                return *close_paren_;
            default:
                return std::nullopt;
        }
    }

    GreenArgument::GreenArgument(const SyntaxKind kind,
                                 GreenPtr<GreenNamedParameter> name,
                                 GreenPtr<GreenExpression> value,
                                 DiagnosticInfoList diagnostics)
        : GreenNode{kind, std::move(diagnostics)}, name_{std::move(name)}, value_{std::move(value)}
    {
        set_child_count(2);
        if (name_ != nullptr)
            adjust_flags_and_width(*name_);
        adjust_flags_and_width(*value_);
    }
    GreenArgument::~GreenArgument() = default;

    Optional<const GreenNode &> GreenArgument::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return name_.get();
            case 1:
                return *value_;
            default:
                return std::nullopt;
        }
    }

} // namespace prism
