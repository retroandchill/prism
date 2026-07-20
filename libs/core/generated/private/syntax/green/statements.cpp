module prism.core:syntax.green.statements.impl;

import :syntax.green.statements;
import :syntax.green.declarations;
import :syntax.green.expressions;

namespace prism
{
    GreenVariableDeclarationStatement::GreenVariableDeclarationStatement(
        RefCountPtr<const GreenVariableDeclaration> declaration,
        DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::variable_declaration_statement, std::move(diagnostics)},
          declaration_{std::move(declaration)}
    {
        set_child_count(1);
        adjust_flags_and_width(*declaration_);
    }

    GreenVariableDeclarationStatement::~GreenVariableDeclarationStatement() = default;

    Optional<const GreenNode &> GreenVariableDeclarationStatement::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *declaration_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] RefCountPtr<const GreenVariableDeclarationStatement> GreenVariableDeclarationStatement::
        with_declaration(RefCountPtr<const GreenVariableDeclaration> declaration) const
    {
        return update(std::move(declaration));
    }

    RefCountPtr<const GreenVariableDeclarationStatement> GreenVariableDeclarationStatement::update(
        RefCountPtr<const GreenVariableDeclaration> declaration) const
    {
        if (declaration == declaration_)
            return shared_from_this();

        return make_ref_counted<const GreenVariableDeclarationStatement>(std::move(declaration));
    }

    GreenBlock::GreenBlock(RefCountPtr<const GreenToken> open_brace,
                           GreenSyntaxList<GreenStatement> statements,
                           RefCountPtr<const GreenToken> close_brace,
                           DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::block, std::move(diagnostics)}, open_brace_{std::move(open_brace)},
          statements_{std::move(statements)}, close_brace_{std::move(close_brace)}
    {
        set_child_count(3);
        adjust_flags_and_width(*open_brace_);
        adjust_flags_and_width(statements_);
        adjust_flags_and_width(*close_brace_);
    }

    GreenBlock::~GreenBlock() = default;

    Optional<const GreenNode &> GreenBlock::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *open_brace_;
            case 1:
                return statements_.node();
            case 2:
                return *close_brace_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] RefCountPtr<const GreenBlock> GreenBlock::with_open_brace(
        RefCountPtr<const GreenToken> open_brace) const
    {
        return update(std::move(open_brace), statements_, close_brace_);
    }

    [[nodiscard]] RefCountPtr<const GreenBlock> GreenBlock::with_statements(
        GreenSyntaxList<GreenStatement> statements) const
    {
        return update(open_brace_, std::move(statements), close_brace_);
    }

    [[nodiscard]] RefCountPtr<const GreenBlock> GreenBlock::with_close_brace(
        RefCountPtr<const GreenToken> close_brace) const
    {
        return update(open_brace_, statements_, std::move(close_brace));
    }

    RefCountPtr<const GreenBlock> GreenBlock::update(RefCountPtr<const GreenToken> open_brace,
                                                     GreenSyntaxList<GreenStatement> statements,
                                                     RefCountPtr<const GreenToken> close_brace) const
    {
        if (open_brace == open_brace_ && statements == statements_ && close_brace == close_brace_)
            return shared_from_this();

        return make_ref_counted<const GreenBlock>(std::move(open_brace), std::move(statements), std::move(close_brace));
    }

    GreenReturnStatement::GreenReturnStatement(RefCountPtr<const GreenToken> return_keyword,
                                               RefCountPtr<const GreenExpression> expression,
                                               RefCountPtr<const GreenToken> semicolon,
                                               DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::return_statement, std::move(diagnostics)},
          return_keyword_{std::move(return_keyword)}, expression_{std::move(expression)},
          semicolon_{std::move(semicolon)}
    {
        set_child_count(3);
        adjust_flags_and_width(*return_keyword_);
        if (expression_ != nullptr)
            adjust_flags_and_width(*expression_);
        adjust_flags_and_width(*semicolon_);
    }

    GreenReturnStatement::~GreenReturnStatement() = default;

    Optional<const GreenNode &> GreenReturnStatement::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *return_keyword_;
            case 1:
                return expression_.get();
            case 2:
                return *semicolon_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] RefCountPtr<const GreenReturnStatement> GreenReturnStatement::with_return_keyword(
        RefCountPtr<const GreenToken> return_keyword) const
    {
        return update(std::move(return_keyword), expression_, semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenReturnStatement> GreenReturnStatement::with_expression(
        RefCountPtr<const GreenExpression> expression) const
    {
        return update(return_keyword_, std::move(expression), semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenReturnStatement> GreenReturnStatement::with_semicolon(
        RefCountPtr<const GreenToken> semicolon) const
    {
        return update(return_keyword_, expression_, std::move(semicolon));
    }

    RefCountPtr<const GreenReturnStatement> GreenReturnStatement::update(RefCountPtr<const GreenToken> return_keyword,
                                                                         RefCountPtr<const GreenExpression> expression,
                                                                         RefCountPtr<const GreenToken> semicolon) const
    {
        if (return_keyword == return_keyword_ && expression == expression_ && semicolon == semicolon_)
            return shared_from_this();

        return make_ref_counted<const GreenReturnStatement>(std::move(return_keyword),
                                                            std::move(expression),
                                                            std::move(semicolon));
    }

    GreenExpressionStatement::GreenExpressionStatement(RefCountPtr<const GreenExpression> expression,
                                                       RefCountPtr<const GreenToken> semicolon,
                                                       DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::expression_statement, std::move(diagnostics)}, expression_{std::move(expression)},
          semicolon_{std::move(semicolon)}
    {
        set_child_count(2);
        adjust_flags_and_width(*expression_);
        adjust_flags_and_width(*semicolon_);
    }

    GreenExpressionStatement::~GreenExpressionStatement() = default;

    Optional<const GreenNode &> GreenExpressionStatement::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *expression_;
            case 1:
                return *semicolon_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] RefCountPtr<const GreenExpressionStatement> GreenExpressionStatement::with_expression(
        RefCountPtr<const GreenExpression> expression) const
    {
        return update(std::move(expression), semicolon_);
    }

    [[nodiscard]] RefCountPtr<const GreenExpressionStatement> GreenExpressionStatement::with_semicolon(
        RefCountPtr<const GreenToken> semicolon) const
    {
        return update(expression_, std::move(semicolon));
    }

    RefCountPtr<const GreenExpressionStatement> GreenExpressionStatement::update(
        RefCountPtr<const GreenExpression> expression,
        RefCountPtr<const GreenToken> semicolon) const
    {
        if (expression == expression_ && semicolon == semicolon_)
            return shared_from_this();

        return make_ref_counted<const GreenExpressionStatement>(std::move(expression), std::move(semicolon));
    }

    GreenEmptyStatement::GreenEmptyStatement(RefCountPtr<const GreenToken> semicolon, DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::empty_statement, std::move(diagnostics)}, semicolon_{std::move(semicolon)}
    {
        set_child_count(1);
        adjust_flags_and_width(*semicolon_);
    }

    GreenEmptyStatement::~GreenEmptyStatement() = default;

    Optional<const GreenNode &> GreenEmptyStatement::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *semicolon_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] RefCountPtr<const GreenEmptyStatement> GreenEmptyStatement::with_semicolon(
        RefCountPtr<const GreenToken> semicolon) const
    {
        return update(std::move(semicolon));
    }

    RefCountPtr<const GreenEmptyStatement> GreenEmptyStatement::update(RefCountPtr<const GreenToken> semicolon) const
    {
        if (semicolon == semicolon_)
            return shared_from_this();

        return make_ref_counted<const GreenEmptyStatement>(std::move(semicolon));
    }
} // namespace prism
