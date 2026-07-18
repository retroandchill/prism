module prism.core:syntax.green.declaration.impl;

import :syntax.green.declaration;
import :syntax.green.clauses;
import :syntax.green.expressions;
import :syntax.green.statement;
import :syntax.green.types;

namespace prism
{
    GreenVariableDeclaration::GreenVariableDeclaration(GreenSyntaxList<GreenToken> modifiers,
                                                       GreenPtr<GreenToken> var_keyword,
                                                       GreenPtr<GreenToken> mut_keyword,
                                                       GreenPtr<GreenToken> identifier,
                                                       GreenPtr<GreenTypeSpecifier> type,
                                                       GreenPtr<GreenInitializer> initializer,
                                                       GreenPtr<GreenToken> semicolon,
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
    GreenFunctionDeclaration::GreenFunctionDeclaration(GreenSyntaxList<GreenToken> modifiers,
                                                       GreenPtr<GreenToken> func_keyword,
                                                       GreenPtr<GreenToken> identifier,
                                                       GreenPtr<GreenParameterList> parameters,
                                                       GreenPtr<GreenTypeSpecifier> return_type,
                                                       GreenPtr<GreenBlock> body,
                                                       GreenPtr<GreenExpressionBody> expression_body,
                                                       GreenPtr<GreenToken> semicolon,
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
} // namespace prism
