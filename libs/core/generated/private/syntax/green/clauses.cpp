module prism.core:syntax.green.clauses.impl;

import :syntax.lifetime;
import :syntax.green.clauses;
import :syntax.clauses;
import :syntax.green.expressions;
import :syntax.green.types;

namespace prism
{
    GreenInitializer::GreenInitializer(GreenPtr<GreenToken> equal_sign,
                                       GreenPtr<GreenExpression> value,
                                       DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::initializer, std::move(diagnostics)}, equal_sign_{std::move(equal_sign)},
          value_{std::move(value)}
    {
        set_slot_count(2);
        adjust_flags_and_width(*equal_sign_);
        adjust_flags_and_width(*value_);
    }

    GreenInitializer::~GreenInitializer() = default;

    void GreenInitializer::set_equal_sign(GreenPtr<GreenToken> value) noexcept
    {
        equal_sign_ = std::move(value);
    }

    void GreenInitializer::set_value(GreenPtr<GreenExpression> value) noexcept
    {
        value_ = std::move(value);
    }

    Optional<const GreenNode &> GreenInitializer::get_slot(std::size_t index) const
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

    [[nodiscard]] SyntaxNode &GreenInitializer::create_red(SyntaxLifetime &lifetime,
                                                           const SyntaxNode *parent,
                                                           std::uint32_t position) const
    {
        return lifetime.add<InitializerSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenInitializer> GreenInitializer::with_equal_sign(GreenPtr<GreenToken> equal_sign) const
    {
        return update(std::move(equal_sign), value_);
    }

    [[nodiscard]] GreenPtr<GreenInitializer> GreenInitializer::with_value(GreenPtr<GreenExpression> value) const
    {
        return update(equal_sign_, std::move(value));
    }

    GreenPtr<GreenInitializer> GreenInitializer::update(GreenPtr<GreenToken> equal_sign,
                                                        GreenPtr<GreenExpression> value) const
    {
        if (equal_sign == equal_sign_ && value == value_)
            return shared_from_this();

        return make_ref_counted<const GreenInitializer>(std::move(equal_sign), std::move(value));
    }

    RefCountPtr<GreenNode> GreenInitializer::clone_internal() const
    {
        return make_ref_counted<GreenInitializer>(equal_sign_, value_);
    }

    GreenTypeSpecifier::GreenTypeSpecifier(GreenPtr<GreenToken> colon,
                                           GreenPtr<GreenType> type,
                                           DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::type_specifier, std::move(diagnostics)}, colon_{std::move(colon)},
          type_{std::move(type)}
    {
        set_slot_count(2);
        adjust_flags_and_width(*colon_);
        adjust_flags_and_width(*type_);
    }

    GreenTypeSpecifier::~GreenTypeSpecifier() = default;

    void GreenTypeSpecifier::set_colon(GreenPtr<GreenToken> value) noexcept
    {
        colon_ = std::move(value);
    }

    void GreenTypeSpecifier::set_type(GreenPtr<GreenType> value) noexcept
    {
        type_ = std::move(value);
    }

    Optional<const GreenNode &> GreenTypeSpecifier::get_slot(std::size_t index) const
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

    [[nodiscard]] SyntaxNode &GreenTypeSpecifier::create_red(SyntaxLifetime &lifetime,
                                                             const SyntaxNode *parent,
                                                             std::uint32_t position) const
    {
        return lifetime.add<TypeSpecifierSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenTypeSpecifier> GreenTypeSpecifier::with_colon(GreenPtr<GreenToken> colon) const
    {
        return update(std::move(colon), type_);
    }

    [[nodiscard]] GreenPtr<GreenTypeSpecifier> GreenTypeSpecifier::with_type(GreenPtr<GreenType> type) const
    {
        return update(colon_, std::move(type));
    }

    GreenPtr<GreenTypeSpecifier> GreenTypeSpecifier::update(GreenPtr<GreenToken> colon, GreenPtr<GreenType> type) const
    {
        if (colon == colon_ && type == type_)
            return shared_from_this();

        return make_ref_counted<const GreenTypeSpecifier>(std::move(colon), std::move(type));
    }

    RefCountPtr<GreenNode> GreenTypeSpecifier::clone_internal() const
    {
        return make_ref_counted<GreenTypeSpecifier>(colon_, type_);
    }

    GreenNamedParameter::GreenNamedParameter(GreenPtr<GreenToken> name,
                                             GreenPtr<GreenToken> colon,
                                             DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::named_parameter, std::move(diagnostics)}, name_{std::move(name)},
          colon_{std::move(colon)}
    {
        set_slot_count(2);
        adjust_flags_and_width(*name_);
        adjust_flags_and_width(*colon_);
    }

    GreenNamedParameter::~GreenNamedParameter() = default;

    void GreenNamedParameter::set_name(GreenPtr<GreenToken> value) noexcept
    {
        name_ = std::move(value);
    }

    void GreenNamedParameter::set_colon(GreenPtr<GreenToken> value) noexcept
    {
        colon_ = std::move(value);
    }

    Optional<const GreenNode &> GreenNamedParameter::get_slot(std::size_t index) const
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

    [[nodiscard]] SyntaxNode &GreenNamedParameter::create_red(SyntaxLifetime &lifetime,
                                                              const SyntaxNode *parent,
                                                              std::uint32_t position) const
    {
        return lifetime.add<NamedParameterSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenNamedParameter> GreenNamedParameter::with_name(GreenPtr<GreenToken> name) const
    {
        return update(std::move(name), colon_);
    }

    [[nodiscard]] GreenPtr<GreenNamedParameter> GreenNamedParameter::with_colon(GreenPtr<GreenToken> colon) const
    {
        return update(name_, std::move(colon));
    }

    GreenPtr<GreenNamedParameter> GreenNamedParameter::update(GreenPtr<GreenToken> name,
                                                              GreenPtr<GreenToken> colon) const
    {
        if (name == name_ && colon == colon_)
            return shared_from_this();

        return make_ref_counted<const GreenNamedParameter>(std::move(name), std::move(colon));
    }

    RefCountPtr<GreenNode> GreenNamedParameter::clone_internal() const
    {
        return make_ref_counted<GreenNamedParameter>(name_, colon_);
    }

    GreenArgumentList::GreenArgumentList(GreenPtr<GreenToken> open_paren,
                                         GreenSeparatedList<GreenArgument> arguments,
                                         GreenPtr<GreenToken> close_paren,
                                         DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::argument_list, std::move(diagnostics)}, open_paren_{std::move(open_paren)},
          arguments_{std::move(arguments)}, close_paren_{std::move(close_paren)}
    {
        set_slot_count(3);
        adjust_flags_and_width(*open_paren_);
        adjust_flags_and_width(arguments_);
        adjust_flags_and_width(*close_paren_);
    }

    GreenArgumentList::~GreenArgumentList() = default;

    void GreenArgumentList::set_open_paren(GreenPtr<GreenToken> value) noexcept
    {
        open_paren_ = std::move(value);
    }

    void GreenArgumentList::set_arguments(GreenSeparatedList<GreenArgument> value) noexcept
    {
        arguments_ = std::move(value);
    }

    void GreenArgumentList::set_close_paren(GreenPtr<GreenToken> value) noexcept
    {
        close_paren_ = std::move(value);
    }

    Optional<const GreenNode &> GreenArgumentList::get_slot(std::size_t index) const
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

    [[nodiscard]] SyntaxNode &GreenArgumentList::create_red(SyntaxLifetime &lifetime,
                                                            const SyntaxNode *parent,
                                                            std::uint32_t position) const
    {
        return lifetime.add<ArgumentListSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenArgumentList> GreenArgumentList::with_open_paren(GreenPtr<GreenToken> open_paren) const
    {
        return update(std::move(open_paren), arguments_, close_paren_);
    }

    [[nodiscard]] GreenPtr<GreenArgumentList> GreenArgumentList::with_arguments(
        GreenSeparatedList<GreenArgument> arguments) const
    {
        return update(open_paren_, std::move(arguments), close_paren_);
    }

    [[nodiscard]] GreenPtr<GreenArgumentList> GreenArgumentList::with_close_paren(
        GreenPtr<GreenToken> close_paren) const
    {
        return update(open_paren_, arguments_, std::move(close_paren));
    }

    GreenPtr<GreenArgumentList> GreenArgumentList::update(GreenPtr<GreenToken> open_paren,
                                                          GreenSeparatedList<GreenArgument> arguments,
                                                          GreenPtr<GreenToken> close_paren) const
    {
        if (open_paren == open_paren_ && arguments == arguments_ && close_paren == close_paren_)
            return shared_from_this();

        return make_ref_counted<const GreenArgumentList>(std::move(open_paren),
                                                         std::move(arguments),
                                                         std::move(close_paren));
    }

    RefCountPtr<GreenNode> GreenArgumentList::clone_internal() const
    {
        return make_ref_counted<GreenArgumentList>(open_paren_, arguments_, close_paren_);
    }

    GreenArgument::GreenArgument(GreenPtr<GreenNamedParameter> name,
                                 GreenPtr<GreenExpression> value,
                                 DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::argument, std::move(diagnostics)}, name_{std::move(name)}, value_{std::move(value)}
    {
        set_slot_count(2);
        if (name_ != nullptr)
            adjust_flags_and_width(*name_);
        adjust_flags_and_width(*value_);
    }

    GreenArgument::~GreenArgument() = default;

    void GreenArgument::set_name(GreenPtr<GreenNamedParameter> value) noexcept
    {
        name_ = std::move(value);
    }

    void GreenArgument::set_value(GreenPtr<GreenExpression> value) noexcept
    {
        value_ = std::move(value);
    }

    Optional<const GreenNode &> GreenArgument::get_slot(std::size_t index) const
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

    [[nodiscard]] SyntaxNode &GreenArgument::create_red(SyntaxLifetime &lifetime,
                                                        const SyntaxNode *parent,
                                                        std::uint32_t position) const
    {
        return lifetime.add<ArgumentSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenArgument> GreenArgument::with_name(GreenPtr<GreenNamedParameter> name) const
    {
        return update(std::move(name), value_);
    }

    [[nodiscard]] GreenPtr<GreenArgument> GreenArgument::with_value(GreenPtr<GreenExpression> value) const
    {
        return update(name_, std::move(value));
    }

    GreenPtr<GreenArgument> GreenArgument::update(GreenPtr<GreenNamedParameter> name,
                                                  GreenPtr<GreenExpression> value) const
    {
        if (name == name_ && value == value_)
            return shared_from_this();

        return make_ref_counted<const GreenArgument>(std::move(name), std::move(value));
    }

    RefCountPtr<GreenNode> GreenArgument::clone_internal() const
    {
        return make_ref_counted<GreenArgument>(name_, value_);
    }

    GreenParameterList::GreenParameterList(GreenPtr<GreenToken> open_paren,
                                           GreenSeparatedList<GreenParameter> parameters,
                                           GreenPtr<GreenToken> close_paren,
                                           DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::parameter_list, std::move(diagnostics)}, open_paren_{std::move(open_paren)},
          parameters_{std::move(parameters)}, close_paren_{std::move(close_paren)}
    {
        set_slot_count(3);
        adjust_flags_and_width(*open_paren_);
        adjust_flags_and_width(parameters_);
        adjust_flags_and_width(*close_paren_);
    }

    GreenParameterList::~GreenParameterList() = default;

    void GreenParameterList::set_open_paren(GreenPtr<GreenToken> value) noexcept
    {
        open_paren_ = std::move(value);
    }

    void GreenParameterList::set_parameters(GreenSeparatedList<GreenParameter> value) noexcept
    {
        parameters_ = std::move(value);
    }

    void GreenParameterList::set_close_paren(GreenPtr<GreenToken> value) noexcept
    {
        close_paren_ = std::move(value);
    }

    Optional<const GreenNode &> GreenParameterList::get_slot(std::size_t index) const
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

    [[nodiscard]] SyntaxNode &GreenParameterList::create_red(SyntaxLifetime &lifetime,
                                                             const SyntaxNode *parent,
                                                             std::uint32_t position) const
    {
        return lifetime.add<ParameterListSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenParameterList> GreenParameterList::with_open_paren(
        GreenPtr<GreenToken> open_paren) const
    {
        return update(std::move(open_paren), parameters_, close_paren_);
    }

    [[nodiscard]] GreenPtr<GreenParameterList> GreenParameterList::with_parameters(
        GreenSeparatedList<GreenParameter> parameters) const
    {
        return update(open_paren_, std::move(parameters), close_paren_);
    }

    [[nodiscard]] GreenPtr<GreenParameterList> GreenParameterList::with_close_paren(
        GreenPtr<GreenToken> close_paren) const
    {
        return update(open_paren_, parameters_, std::move(close_paren));
    }

    GreenPtr<GreenParameterList> GreenParameterList::update(GreenPtr<GreenToken> open_paren,
                                                            GreenSeparatedList<GreenParameter> parameters,
                                                            GreenPtr<GreenToken> close_paren) const
    {
        if (open_paren == open_paren_ && parameters == parameters_ && close_paren == close_paren_)
            return shared_from_this();

        return make_ref_counted<const GreenParameterList>(std::move(open_paren),
                                                          std::move(parameters),
                                                          std::move(close_paren));
    }

    RefCountPtr<GreenNode> GreenParameterList::clone_internal() const
    {
        return make_ref_counted<GreenParameterList>(open_paren_, parameters_, close_paren_);
    }

    GreenParameter::GreenParameter(GreenPtr<GreenToken> mut_keyword,
                                   GreenPtr<GreenToken> name,
                                   GreenPtr<GreenTypeSpecifier> type_specifier,
                                   GreenPtr<GreenInitializer> default_value,
                                   DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::parameter, std::move(diagnostics)}, mut_keyword_{std::move(mut_keyword)},
          name_{std::move(name)}, type_specifier_{std::move(type_specifier)}, default_value_{std::move(default_value)}
    {
        set_slot_count(4);
        if (mut_keyword_ != nullptr)
            adjust_flags_and_width(*mut_keyword_);
        adjust_flags_and_width(*name_);
        adjust_flags_and_width(*type_specifier_);
        if (default_value_ != nullptr)
            adjust_flags_and_width(*default_value_);
    }

    GreenParameter::~GreenParameter() = default;

    void GreenParameter::set_mut_keyword(GreenPtr<GreenToken> value) noexcept
    {
        mut_keyword_ = std::move(value);
    }

    void GreenParameter::set_name(GreenPtr<GreenToken> value) noexcept
    {
        name_ = std::move(value);
    }

    void GreenParameter::set_type_specifier(GreenPtr<GreenTypeSpecifier> value) noexcept
    {
        type_specifier_ = std::move(value);
    }

    void GreenParameter::set_default_value(GreenPtr<GreenInitializer> value) noexcept
    {
        default_value_ = std::move(value);
    }

    Optional<const GreenNode &> GreenParameter::get_slot(std::size_t index) const
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

    [[nodiscard]] SyntaxNode &GreenParameter::create_red(SyntaxLifetime &lifetime,
                                                         const SyntaxNode *parent,
                                                         std::uint32_t position) const
    {
        return lifetime.add<ParameterSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenParameter> GreenParameter::with_mut_keyword(GreenPtr<GreenToken> mut_keyword) const
    {
        return update(std::move(mut_keyword), name_, type_specifier_, default_value_);
    }

    [[nodiscard]] GreenPtr<GreenParameter> GreenParameter::with_name(GreenPtr<GreenToken> name) const
    {
        return update(mut_keyword_, std::move(name), type_specifier_, default_value_);
    }

    [[nodiscard]] GreenPtr<GreenParameter> GreenParameter::with_type_specifier(
        GreenPtr<GreenTypeSpecifier> type_specifier) const
    {
        return update(mut_keyword_, name_, std::move(type_specifier), default_value_);
    }

    [[nodiscard]] GreenPtr<GreenParameter> GreenParameter::with_default_value(
        GreenPtr<GreenInitializer> default_value) const
    {
        return update(mut_keyword_, name_, type_specifier_, std::move(default_value));
    }

    GreenPtr<GreenParameter> GreenParameter::update(GreenPtr<GreenToken> mut_keyword,
                                                    GreenPtr<GreenToken> name,
                                                    GreenPtr<GreenTypeSpecifier> type_specifier,
                                                    GreenPtr<GreenInitializer> default_value) const
    {
        if (mut_keyword == mut_keyword_ && name == name_ && type_specifier == type_specifier_ &&
            default_value == default_value_)
            return shared_from_this();

        return make_ref_counted<const GreenParameter>(std::move(mut_keyword),
                                                      std::move(name),
                                                      std::move(type_specifier),
                                                      std::move(default_value));
    }

    RefCountPtr<GreenNode> GreenParameter::clone_internal() const
    {
        return make_ref_counted<GreenParameter>(mut_keyword_, name_, type_specifier_, default_value_);
    }

    GreenExpressionBody::GreenExpressionBody(GreenPtr<GreenToken> arrow,
                                             GreenPtr<GreenExpression> expression,
                                             DiagnosticInfoList diagnostics)
        : GreenNode{SyntaxKind::expression_body, std::move(diagnostics)}, arrow_{std::move(arrow)},
          expression_{std::move(expression)}
    {
        set_slot_count(2);
        adjust_flags_and_width(*arrow_);
        adjust_flags_and_width(*expression_);
    }

    GreenExpressionBody::~GreenExpressionBody() = default;

    void GreenExpressionBody::set_arrow(GreenPtr<GreenToken> value) noexcept
    {
        arrow_ = std::move(value);
    }

    void GreenExpressionBody::set_expression(GreenPtr<GreenExpression> value) noexcept
    {
        expression_ = std::move(value);
    }

    Optional<const GreenNode &> GreenExpressionBody::get_slot(std::size_t index) const
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

    [[nodiscard]] SyntaxNode &GreenExpressionBody::create_red(SyntaxLifetime &lifetime,
                                                              const SyntaxNode *parent,
                                                              std::uint32_t position) const
    {
        return lifetime.add<ExpressionBodySyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenExpressionBody> GreenExpressionBody::with_arrow(GreenPtr<GreenToken> arrow) const
    {
        return update(std::move(arrow), expression_);
    }

    [[nodiscard]] GreenPtr<GreenExpressionBody> GreenExpressionBody::with_expression(
        GreenPtr<GreenExpression> expression) const
    {
        return update(arrow_, std::move(expression));
    }

    GreenPtr<GreenExpressionBody> GreenExpressionBody::update(GreenPtr<GreenToken> arrow,
                                                              GreenPtr<GreenExpression> expression) const
    {
        if (arrow == arrow_ && expression == expression_)
            return shared_from_this();

        return make_ref_counted<const GreenExpressionBody>(std::move(arrow), std::move(expression));
    }

    RefCountPtr<GreenNode> GreenExpressionBody::clone_internal() const
    {
        return make_ref_counted<GreenExpressionBody>(arrow_, expression_);
    }
} // namespace prism
