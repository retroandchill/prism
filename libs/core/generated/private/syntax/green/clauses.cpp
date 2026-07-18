module prism.core:syntax.green.clauses.impl;

import :syntax.green.clauses;
import :syntax.green.declaration;
import :syntax.green.expressions;
import :syntax.green.statement;
import :syntax.green.types;

namespace prism
{
    GreenInitializer::GreenInitializer(GreenPtr<GreenToken> equal_sign,
                                       GreenPtr<GreenExpression> value,
                                       DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::initializer, std::move(diagnostics)}, equal_sign_{std::move(equal_sign)},
          value_{std::move(value)}
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
    GreenTypeSpecifier::GreenTypeSpecifier(GreenPtr<GreenToken> colon,
                                           GreenPtr<GreenType> type,
                                           DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::type_specifier, std::move(diagnostics)}, colon_{std::move(colon)},
          type_{std::move(type)}
    {
        set_child_count(2);
        adjust_flags_and_width(*colon_);
        adjust_flags_and_width(*type_);
    }

    GreenTypeSpecifier::~GreenTypeSpecifier() = default;

    Optional<const GreenNode &> GreenTypeSpecifier::get_child(std::size_t index) const
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
    GreenNamedParameter::GreenNamedParameter(GreenPtr<GreenToken> name,
                                             GreenPtr<GreenToken> colon,
                                             DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::named_parameter, std::move(diagnostics)}, name_{std::move(name)},
          colon_{std::move(colon)}
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
    GreenArgumentList::GreenArgumentList(GreenPtr<GreenToken> open_paren,
                                         GreenSeparatedList<GreenArgument> arguments,
                                         GreenPtr<GreenToken> close_paren,
                                         DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::argument_list, std::move(diagnostics)}, open_paren_{std::move(open_paren)},
          arguments_{std::move(arguments)}, close_paren_{std::move(close_paren)}
    {
        set_child_count(3);
        adjust_flags_and_width(*open_paren_);
        adjust_flags_and_width(arguments_);
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
                return arguments_.node();
            case 2:
                return *close_paren_;
            default:
                return std::nullopt;
        }
    }
    GreenArgument::GreenArgument(GreenPtr<GreenNamedParameter> name,
                                 GreenPtr<GreenExpression> value,
                                 DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::argument, std::move(diagnostics)}, name_{std::move(name)}, value_{std::move(value)}
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
    GreenParameterList::GreenParameterList(GreenPtr<GreenToken> open_paren,
                                           GreenSeparatedList<GreenParameter> parameters,
                                           GreenPtr<GreenToken> close_paren,
                                           DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::parameter_list, std::move(diagnostics)}, open_paren_{std::move(open_paren)},
          parameters_{std::move(parameters)}, close_paren_{std::move(close_paren)}
    {
        set_child_count(3);
        adjust_flags_and_width(*open_paren_);
        adjust_flags_and_width(parameters_);
        adjust_flags_and_width(*close_paren_);
    }

    GreenParameterList::~GreenParameterList() = default;

    Optional<const GreenNode &> GreenParameterList::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *open_paren_;
            case 1:
                return parameters_.node();
            case 2:
                return *close_paren_;
            default:
                return std::nullopt;
        }
    }
    GreenParameter::GreenParameter(GreenPtr<GreenToken> mut_keyword,
                                   GreenPtr<GreenToken> name,
                                   GreenPtr<GreenTypeSpecifier> type_specifier,
                                   GreenPtr<GreenInitializer> default_value,
                                   DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::parameter, std::move(diagnostics)}, mut_keyword_{std::move(mut_keyword)},
          name_{std::move(name)}, type_specifier_{std::move(type_specifier)}, default_value_{std::move(default_value)}
    {
        set_child_count(4);
        if (mut_keyword_ != nullptr)
            adjust_flags_and_width(*mut_keyword_);
        adjust_flags_and_width(*name_);
        adjust_flags_and_width(*type_specifier_);
        if (default_value_ != nullptr)
            adjust_flags_and_width(*default_value_);
    }

    GreenParameter::~GreenParameter() = default;

    Optional<const GreenNode &> GreenParameter::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return mut_keyword_.get();
            case 1:
                return *name_;
            case 2:
                return *type_specifier_;
            case 3:
                return default_value_.get();
            default:
                return std::nullopt;
        }
    }
    GreenExpressionBody::GreenExpressionBody(GreenPtr<GreenToken> arrow,
                                             GreenPtr<GreenExpression> expression,
                                             DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::expression_body, std::move(diagnostics)}, arrow_{std::move(arrow)},
          expression_{std::move(expression)}
    {
        set_child_count(2);
        adjust_flags_and_width(*arrow_);
        adjust_flags_and_width(*expression_);
    }

    GreenExpressionBody::~GreenExpressionBody() = default;

    Optional<const GreenNode &> GreenExpressionBody::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *arrow_;
            case 1:
                return *expression_;
            default:
                return std::nullopt;
        }
    }
} // namespace prism
