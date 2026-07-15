module prism.core:syntax.green.statement.impl;

import :syntax.green.statement;
import :syntax.green.clauses;
import :syntax.green.declaration;
import :syntax.green.expression;
import :syntax.green.type;

namespace prism
{
    GreenStatement::~GreenStatement() = default;

    GreenVariableDeclarationStatement::GreenVariableDeclarationStatement(GreenPtr<GreenVariableDeclaration> declaration,
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

    GreenBlock::GreenBlock(GreenSyntaxList<GreenStatement> statements, DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::block, std::move(diagnostics)}, statements_{std::move(statements)}
    {
        set_child_count(1);
        adjust_flags_and_width(statements_);
    }
    GreenBlock::~GreenBlock() = default;

    Optional<const GreenNode &> GreenBlock::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return statements_.node();
            default:
                return std::nullopt;
        }
    }

    GreenReturnStatement::GreenReturnStatement(GreenPtr<GreenToken> return_keyword,
                                               GreenPtr<GreenExpression> expression,
                                               GreenPtr<GreenToken> semicolon,
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

    GreenExpressionStatement::GreenExpressionStatement(GreenPtr<GreenExpression> expression,
                                                       GreenPtr<GreenToken> semicolon,
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

    GreenEmptyStatement::GreenEmptyStatement(GreenPtr<GreenToken> semicolon, DiagnosticInfoList diagnostics)
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

} // namespace prism
