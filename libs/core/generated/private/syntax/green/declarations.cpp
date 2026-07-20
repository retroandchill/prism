module prism.core:syntax.green.declarations.impl;

import :syntax.green.declarations;
import :syntax.green.clauses;
import :syntax.green.statements;

namespace prism
{
    GreenVariableDeclaration::GreenVariableDeclaration(GreenSyntaxList<GreenToken> modifiers,
                                                       RefCountPtr<const GreenToken> var_keyword,
                                                       RefCountPtr<const GreenToken> mut_keyword,
                                                       RefCountPtr<const GreenToken> identifier,
                                                       RefCountPtr<const GreenTypeSpecifier> type,
                                                       RefCountPtr<const GreenInitializer> initializer,
                                                       RefCountPtr<const GreenToken> semicolon,
                                                       DiagnosticInfoList diagnostics)
        : GreenDeclaration{SyntaxKind::variable_declaration, std::move(diagnostics)}, modifiers_{std::move(modifiers)},
          var_keyword_{std::move(var_keyword)}, mut_keyword_{std::move(mut_keyword)},
          identifier_{std::move(identifier)}, type_{std::move(type)}, initializer_{std::move(initializer)},
          semicolon_{std::move(semicolon)}
    {
        set_child_count(7);
        adjust_flags_and_width(modifiers_);
        adjust_flags_and_width(*var_keyword_);
        if (mut_keyword_ != nullptr)
            adjust_flags_and_width(*mut_keyword_);
        adjust_flags_and_width(*identifier_);
        adjust_flags_and_width(*type_);
        adjust_flags_and_width(*initializer_);
        adjust_flags_and_width(*semicolon_);
    }

    GreenVariableDeclaration::~GreenVariableDeclaration() = default;

    Optional<const GreenNode &> GreenVariableDeclaration::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return modifiers_.node();
            case 1:
                return *var_keyword_;
            case 2:
                return mut_keyword_.get();
            case 3:
                return *identifier_;
            case 4:
                return *type_;
            case 5:
                return *initializer_;
            case 6:
                return *semicolon_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] RefCountPtr<const GreenDeclaration> GreenVariableDeclaration::with_modifiers_core(
        GreenSyntaxList<GreenToken> modifiers) const
    {
        return update(std::move(modifiers), var_keyword_, mut_keyword_, identifier_, type_, initializer_, semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenVariableDeclaration> GreenVariableDeclaration::with_var_keyword(
        RefCountPtr<const GreenToken> var_keyword) const
    {
        return update(modifiers_, std::move(var_keyword), mut_keyword_, identifier_, type_, initializer_, semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenVariableDeclaration> GreenVariableDeclaration::with_mut_keyword(
        RefCountPtr<const GreenToken> mut_keyword) const
    {
        return update(modifiers_, var_keyword_, std::move(mut_keyword), identifier_, type_, initializer_, semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenVariableDeclaration> GreenVariableDeclaration::with_identifier(
        RefCountPtr<const GreenToken> identifier) const
    {
        return update(modifiers_, var_keyword_, mut_keyword_, std::move(identifier), type_, initializer_, semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenVariableDeclaration> GreenVariableDeclaration::with_type(
        RefCountPtr<const GreenTypeSpecifier> type) const
    {
        return update(modifiers_, var_keyword_, mut_keyword_, identifier_, std::move(type), initializer_, semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenVariableDeclaration> GreenVariableDeclaration::with_initializer(
        RefCountPtr<const GreenInitializer> initializer) const
    {
        return update(modifiers_, var_keyword_, mut_keyword_, identifier_, type_, std::move(initializer), semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenVariableDeclaration> GreenVariableDeclaration::with_semicolon(
        RefCountPtr<const GreenToken> semicolon) const
    {
        return update(modifiers_, var_keyword_, mut_keyword_, identifier_, type_, initializer_, std::move(semicolon));
    }

    RefCountPtr<const GreenVariableDeclaration> GreenVariableDeclaration::update(
        GreenSyntaxList<GreenToken> modifiers,
        RefCountPtr<const GreenToken> var_keyword,
        RefCountPtr<const GreenToken> mut_keyword,
        RefCountPtr<const GreenToken> identifier,
        RefCountPtr<const GreenTypeSpecifier> type,
        RefCountPtr<const GreenInitializer> initializer,
        RefCountPtr<const GreenToken> semicolon) const
    {
        if (modifiers == modifiers_ && var_keyword == var_keyword_ && mut_keyword == mut_keyword_ &&
            identifier == identifier_ && type == type_ && initializer == initializer_ && semicolon == semicolon_)
            return shared_from_this();

        return make_ref_counted<const GreenVariableDeclaration>(std::move(modifiers),
                                                                std::move(var_keyword),
                                                                std::move(mut_keyword),
                                                                std::move(identifier),
                                                                std::move(type),
                                                                std::move(initializer),
                                                                std::move(semicolon));
    }

    GreenFunctionDeclaration::GreenFunctionDeclaration(GreenSyntaxList<GreenToken> modifiers,
                                                       RefCountPtr<const GreenToken> func_keyword,
                                                       RefCountPtr<const GreenToken> identifier,
                                                       RefCountPtr<const GreenParameterList> parameters,
                                                       RefCountPtr<const GreenTypeSpecifier> return_type,
                                                       RefCountPtr<const GreenBlock> body,
                                                       RefCountPtr<const GreenExpressionBody> expression_body,
                                                       RefCountPtr<const GreenToken> semicolon,
                                                       DiagnosticInfoList diagnostics)
        : GreenDeclaration{SyntaxKind::function_declaration, std::move(diagnostics)}, modifiers_{std::move(modifiers)},
          func_keyword_{std::move(func_keyword)}, identifier_{std::move(identifier)},
          parameters_{std::move(parameters)}, return_type_{std::move(return_type)}, body_{std::move(body)},
          expression_body_{std::move(expression_body)}, semicolon_{std::move(semicolon)}
    {
        set_child_count(8);
        adjust_flags_and_width(modifiers_);
        adjust_flags_and_width(*func_keyword_);
        adjust_flags_and_width(*identifier_);
        adjust_flags_and_width(*parameters_);
        if (return_type_ != nullptr)
            adjust_flags_and_width(*return_type_);
        if (body_ != nullptr)
            adjust_flags_and_width(*body_);
        if (expression_body_ != nullptr)
            adjust_flags_and_width(*expression_body_);
        if (semicolon_ != nullptr)
            adjust_flags_and_width(*semicolon_);
    }

    GreenFunctionDeclaration::~GreenFunctionDeclaration() = default;

    Optional<const GreenNode &> GreenFunctionDeclaration::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return modifiers_.node();
            case 1:
                return *func_keyword_;
            case 2:
                return *identifier_;
            case 3:
                return *parameters_;
            case 4:
                return return_type_.get();
            case 5:
                return body_.get();
            case 6:
                return expression_body_.get();
            case 7:
                return semicolon_.get();
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] RefCountPtr<const GreenDeclaration> GreenFunctionDeclaration::with_modifiers_core(
        GreenSyntaxList<GreenToken> modifiers) const
    {
        return update(std::move(modifiers),
                      func_keyword_,
                      identifier_,
                      parameters_,
                      return_type_,
                      body_,
                      expression_body_,
                      semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenFunctionDeclaration> GreenFunctionDeclaration::with_func_keyword(
        RefCountPtr<const GreenToken> func_keyword) const
    {
        return update(modifiers_,
                      std::move(func_keyword),
                      identifier_,
                      parameters_,
                      return_type_,
                      body_,
                      expression_body_,
                      semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenFunctionDeclaration> GreenFunctionDeclaration::with_identifier(
        RefCountPtr<const GreenToken> identifier) const
    {
        return update(modifiers_,
                      func_keyword_,
                      std::move(identifier),
                      parameters_,
                      return_type_,
                      body_,
                      expression_body_,
                      semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenFunctionDeclaration> GreenFunctionDeclaration::with_parameters(
        RefCountPtr<const GreenParameterList> parameters) const
    {
        return update(modifiers_,
                      func_keyword_,
                      identifier_,
                      std::move(parameters),
                      return_type_,
                      body_,
                      expression_body_,
                      semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenFunctionDeclaration> GreenFunctionDeclaration::with_return_type(
        RefCountPtr<const GreenTypeSpecifier> return_type) const
    {
        return update(modifiers_,
                      func_keyword_,
                      identifier_,
                      parameters_,
                      std::move(return_type),
                      body_,
                      expression_body_,
                      semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenFunctionDeclaration> GreenFunctionDeclaration::with_body(
        RefCountPtr<const GreenBlock> body) const
    {
        return update(modifiers_,
                      func_keyword_,
                      identifier_,
                      parameters_,
                      return_type_,
                      std::move(body),
                      expression_body_,
                      semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenFunctionDeclaration> GreenFunctionDeclaration::with_expression_body(
        RefCountPtr<const GreenExpressionBody> expression_body) const
    {
        return update(modifiers_,
                      func_keyword_,
                      identifier_,
                      parameters_,
                      return_type_,
                      body_,
                      std::move(expression_body),
                      semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenFunctionDeclaration> GreenFunctionDeclaration::with_semicolon(
        RefCountPtr<const GreenToken> semicolon) const
    {
        return update(modifiers_,
                      func_keyword_,
                      identifier_,
                      parameters_,
                      return_type_,
                      body_,
                      expression_body_,
                      std::move(semicolon));
    }

    RefCountPtr<const GreenFunctionDeclaration> GreenFunctionDeclaration::update(
        GreenSyntaxList<GreenToken> modifiers,
        RefCountPtr<const GreenToken> func_keyword,
        RefCountPtr<const GreenToken> identifier,
        RefCountPtr<const GreenParameterList> parameters,
        RefCountPtr<const GreenTypeSpecifier> return_type,
        RefCountPtr<const GreenBlock> body,
        RefCountPtr<const GreenExpressionBody> expression_body,
        RefCountPtr<const GreenToken> semicolon) const
    {
        if (modifiers == modifiers_ && func_keyword == func_keyword_ && identifier == identifier_ &&
            parameters == parameters_ && return_type == return_type_ && body == body_ &&
            expression_body == expression_body_ && semicolon == semicolon_)
            return shared_from_this();

        return make_ref_counted<const GreenFunctionDeclaration>(std::move(modifiers),
                                                                std::move(func_keyword),
                                                                std::move(identifier),
                                                                std::move(parameters),
                                                                std::move(return_type),
                                                                std::move(body),
                                                                std::move(expression_body),
                                                                std::move(semicolon));
    }
} // namespace prism
