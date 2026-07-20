module prism.core:syntax.green.clauses.impl;

import :syntax.green.clauses;
import :syntax.green.expressions;
import :syntax.green.types;

namespace prism
{
    GreenInitializer::GreenInitializer(RefCountPtr<const GreenToken> equal_sign,
                                       RefCountPtr<const GreenExpression> value,
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

    [[nodiscard]] RefCountPtr<const GreenInitializer> GreenInitializer::with_equal_sign(
        RefCountPtr<const GreenToken> equal_sign) const
    {
        return update(std::move(equal_sign), value_);
    }

    [[nodiscard]] RefCountPtr<const GreenInitializer> GreenInitializer::with_value(
        RefCountPtr<const GreenExpression> value) const
    {
        return update(equal_sign_, std::move(value));
    }

    RefCountPtr<const GreenInitializer> GreenInitializer::update(RefCountPtr<const GreenToken> equal_sign,
                                                                 RefCountPtr<const GreenExpression> value) const
    {
        if (equal_sign == equal_sign_ && value == value_)
            return shared_from_this();

        return make_ref_counted<const GreenInitializer>(std::move(equal_sign), std::move(value));
    }

    GreenTypeSpecifier::GreenTypeSpecifier(RefCountPtr<const GreenToken> colon,
                                           RefCountPtr<const GreenType> type,
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

    [[nodiscard]] RefCountPtr<const GreenTypeSpecifier> GreenTypeSpecifier::with_colon(
        RefCountPtr<const GreenToken> colon) const
    {
        return update(std::move(colon), type_);
    }

    [[nodiscard]] RefCountPtr<const GreenTypeSpecifier> GreenTypeSpecifier::with_type(
        RefCountPtr<const GreenType> type) const
    {
        return update(colon_, std::move(type));
    }

    RefCountPtr<const GreenTypeSpecifier> GreenTypeSpecifier::update(RefCountPtr<const GreenToken> colon,
                                                                     RefCountPtr<const GreenType> type) const
    {
        if (colon == colon_ && type == type_)
            return shared_from_this();

        return make_ref_counted<const GreenTypeSpecifier>(std::move(colon), std::move(type));
    }

    GreenNamedParameter::GreenNamedParameter(RefCountPtr<const GreenToken> name,
                                             RefCountPtr<const GreenToken> colon,
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

    [[nodiscard]] RefCountPtr<const GreenNamedParameter> GreenNamedParameter::with_name(
        RefCountPtr<const GreenToken> name) const
    {
        return update(std::move(name), colon_);
    }

    [[nodiscard]] RefCountPtr<const GreenNamedParameter> GreenNamedParameter::with_colon(
        RefCountPtr<const GreenToken> colon) const
    {
        return update(name_, std::move(colon));
    }

    RefCountPtr<const GreenNamedParameter> GreenNamedParameter::update(RefCountPtr<const GreenToken> name,
                                                                       RefCountPtr<const GreenToken> colon) const
    {
        if (name == name_ && colon == colon_)
            return shared_from_this();

        return make_ref_counted<const GreenNamedParameter>(std::move(name), std::move(colon));
    }

    GreenArgumentList::GreenArgumentList(RefCountPtr<const GreenToken> open_paren,
                                         GreenSeparatedList<GreenArgument> arguments,
                                         RefCountPtr<const GreenToken> close_paren,
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

    [[nodiscard]] RefCountPtr<const GreenArgumentList> GreenArgumentList::with_open_paren(
        RefCountPtr<const GreenToken> open_paren) const
    {
        return update(std::move(open_paren), arguments_, close_paren_);
    }

    [[nodiscard]] RefCountPtr<const GreenArgumentList> GreenArgumentList::with_arguments(
        GreenSeparatedList<GreenArgument> arguments) const
    {
        return update(open_paren_, std::move(arguments), close_paren_);
    }

    [[nodiscard]] RefCountPtr<const GreenArgumentList> GreenArgumentList::with_close_paren(
        RefCountPtr<const GreenToken> close_paren) const
    {
        return update(open_paren_, arguments_, std::move(close_paren));
    }

    RefCountPtr<const GreenArgumentList> GreenArgumentList::update(RefCountPtr<const GreenToken> open_paren,
                                                                   GreenSeparatedList<GreenArgument> arguments,
                                                                   RefCountPtr<const GreenToken> close_paren) const
    {
        if (open_paren == open_paren_ && arguments == arguments_ && close_paren == close_paren_)
            return shared_from_this();

        return make_ref_counted<const GreenArgumentList>(std::move(open_paren),
                                                         std::move(arguments),
                                                         std::move(close_paren));
    }

    GreenArgument::GreenArgument(RefCountPtr<const GreenNamedParameter> name,
                                 RefCountPtr<const GreenExpression> value,
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

    [[nodiscard]] RefCountPtr<const GreenArgument> GreenArgument::with_name(
        RefCountPtr<const GreenNamedParameter> name) const
    {
        return update(std::move(name), value_);
    }

    [[nodiscard]] RefCountPtr<const GreenArgument> GreenArgument::with_value(
        RefCountPtr<const GreenExpression> value) const
    {
        return update(name_, std::move(value));
    }

    RefCountPtr<const GreenArgument> GreenArgument::update(RefCountPtr<const GreenNamedParameter> name,
                                                           RefCountPtr<const GreenExpression> value) const
    {
        if (name == name_ && value == value_)
            return shared_from_this();

        return make_ref_counted<const GreenArgument>(std::move(name), std::move(value));
    }

    GreenParameterList::GreenParameterList(RefCountPtr<const GreenToken> open_paren,
                                           GreenSeparatedList<GreenParameter> parameters,
                                           RefCountPtr<const GreenToken> close_paren,
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

    [[nodiscard]] RefCountPtr<const GreenParameterList> GreenParameterList::with_open_paren(
        RefCountPtr<const GreenToken> open_paren) const
    {
        return update(std::move(open_paren), parameters_, close_paren_);
    }

    [[nodiscard]] RefCountPtr<const GreenParameterList> GreenParameterList::with_parameters(
        GreenSeparatedList<GreenParameter> parameters) const
    {
        return update(open_paren_, std::move(parameters), close_paren_);
    }

    [[nodiscard]] RefCountPtr<const GreenParameterList> GreenParameterList::with_close_paren(
        RefCountPtr<const GreenToken> close_paren) const
    {
        return update(open_paren_, parameters_, std::move(close_paren));
    }

    RefCountPtr<const GreenParameterList> GreenParameterList::update(RefCountPtr<const GreenToken> open_paren,
                                                                     GreenSeparatedList<GreenParameter> parameters,
                                                                     RefCountPtr<const GreenToken> close_paren) const
    {
        if (open_paren == open_paren_ && parameters == parameters_ && close_paren == close_paren_)
            return shared_from_this();

        return make_ref_counted<const GreenParameterList>(std::move(open_paren),
                                                          std::move(parameters),
                                                          std::move(close_paren));
    }

    GreenParameter::GreenParameter(RefCountPtr<const GreenToken> mut_keyword,
                                   RefCountPtr<const GreenToken> name,
                                   RefCountPtr<const GreenTypeSpecifier> type_specifier,
                                   RefCountPtr<const GreenInitializer> default_value,
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

    [[nodiscard]] RefCountPtr<const GreenParameter> GreenParameter::with_mut_keyword(
        RefCountPtr<const GreenToken> mut_keyword) const
    {
        return update(std::move(mut_keyword), name_, type_specifier_, default_value_);
    }

    [[nodiscard]] RefCountPtr<const GreenParameter> GreenParameter::with_name(RefCountPtr<const GreenToken> name) const
    {
        return update(mut_keyword_, std::move(name), type_specifier_, default_value_);
    }

    [[nodiscard]] RefCountPtr<const GreenParameter> GreenParameter::with_type_specifier(
        RefCountPtr<const GreenTypeSpecifier> type_specifier) const
    {
        return update(mut_keyword_, name_, std::move(type_specifier), default_value_);
    }

    [[nodiscard]] RefCountPtr<const GreenParameter> GreenParameter::with_default_value(
        RefCountPtr<const GreenInitializer> default_value) const
    {
        return update(mut_keyword_, name_, type_specifier_, std::move(default_value));
    }

    RefCountPtr<const GreenParameter> GreenParameter::update(RefCountPtr<const GreenToken> mut_keyword,
                                                             RefCountPtr<const GreenToken> name,
                                                             RefCountPtr<const GreenTypeSpecifier> type_specifier,
                                                             RefCountPtr<const GreenInitializer> default_value) const
    {
        if (mut_keyword == mut_keyword_ && name == name_ && type_specifier == type_specifier_ &&
            default_value == default_value_)
            return shared_from_this();

        return make_ref_counted<const GreenParameter>(std::move(mut_keyword),
                                                      std::move(name),
                                                      std::move(type_specifier),
                                                      std::move(default_value));
    }

    GreenExpressionBody::GreenExpressionBody(RefCountPtr<const GreenToken> arrow,
                                             RefCountPtr<const GreenExpression> expression,
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

    [[nodiscard]] RefCountPtr<const GreenExpressionBody> GreenExpressionBody::with_arrow(
        RefCountPtr<const GreenToken> arrow) const
    {
        return update(std::move(arrow), expression_);
    }

    [[nodiscard]] RefCountPtr<const GreenExpressionBody> GreenExpressionBody::with_expression(
        RefCountPtr<const GreenExpression> expression) const
    {
        return update(arrow_, std::move(expression));
    }

    RefCountPtr<const GreenExpressionBody> GreenExpressionBody::update(
        RefCountPtr<const GreenToken> arrow,
        RefCountPtr<const GreenExpression> expression) const
    {
        if (arrow == arrow_ && expression == expression_)
            return shared_from_this();

        return make_ref_counted<const GreenExpressionBody>(std::move(arrow), std::move(expression));
    }
} // namespace prism
