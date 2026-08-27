module prism.core:syntax.green.statements.impl;

import :syntax.lifetime;
import :syntax.green.statements;
import :syntax.statements;
import :syntax.green.clauses;
import :syntax.green.declarations;
import :syntax.green.expressions;

namespace prism
{
    GreenEmptyStatement::GreenEmptyStatement(GreenPtr<GreenToken> semicolon, DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::empty_statement, std::move(diagnostics)}, semicolon_{std::move(semicolon)}
    {
        set_slot_count(1);
        adjust_flags_and_width(*semicolon_);
    }

    GreenEmptyStatement::~GreenEmptyStatement() = default;

    void GreenEmptyStatement::set_semicolon(GreenPtr<GreenToken> value) noexcept
    {
        semicolon_ = std::move(value);
    }

    Optional<const GreenNode &> GreenEmptyStatement::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *semicolon_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenEmptyStatement::create_red(SyntaxLifetime &lifetime,
                                                              const SyntaxNode *parent,
                                                              std::uint32_t position) const
    {
        return lifetime.add<EmptyStatementSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenEmptyStatement> GreenEmptyStatement::with_semicolon(
        GreenPtr<GreenToken> semicolon) const
    {
        return update(std::move(semicolon));
    }

    GreenPtr<GreenEmptyStatement> GreenEmptyStatement::update(GreenPtr<GreenToken> semicolon) const
    {
        if (semicolon == semicolon_)
            return shared_from_this();

        return make_ref_counted<const GreenEmptyStatement>(std::move(semicolon));
    }

    RefCountPtr<GreenNode> GreenEmptyStatement::clone_internal() const
    {
        return make_ref_counted<GreenEmptyStatement>(semicolon_);
    }

    GreenVariableDeclarationStatement::GreenVariableDeclarationStatement(GreenPtr<GreenVariableDeclaration> declaration,
                                                                         DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::variable_declaration_statement, std::move(diagnostics)},
          declaration_{std::move(declaration)}
    {
        set_slot_count(1);
        adjust_flags_and_width(*declaration_);
    }

    GreenVariableDeclarationStatement::~GreenVariableDeclarationStatement() = default;

    void GreenVariableDeclarationStatement::set_declaration(GreenPtr<GreenVariableDeclaration> value) noexcept
    {
        declaration_ = std::move(value);
    }

    Optional<const GreenNode &> GreenVariableDeclarationStatement::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *declaration_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenVariableDeclarationStatement::create_red(SyntaxLifetime &lifetime,
                                                                            const SyntaxNode *parent,
                                                                            std::uint32_t position) const
    {
        return lifetime.add<VariableDeclarationStatementSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenVariableDeclarationStatement> GreenVariableDeclarationStatement::with_declaration(
        GreenPtr<GreenVariableDeclaration> declaration) const
    {
        return update(std::move(declaration));
    }

    GreenPtr<GreenVariableDeclarationStatement> GreenVariableDeclarationStatement::update(
        GreenPtr<GreenVariableDeclaration> declaration) const
    {
        if (declaration == declaration_)
            return shared_from_this();

        return make_ref_counted<const GreenVariableDeclarationStatement>(std::move(declaration));
    }

    RefCountPtr<GreenNode> GreenVariableDeclarationStatement::clone_internal() const
    {
        return make_ref_counted<GreenVariableDeclarationStatement>(declaration_);
    }

    GreenBlock::GreenBlock(GreenPtr<GreenToken> open_brace,
                           GreenSyntaxList<GreenStatement> statements,
                           GreenPtr<GreenToken> close_brace,
                           DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::block, std::move(diagnostics)}, open_brace_{std::move(open_brace)},
          statements_{std::move(statements)}, close_brace_{std::move(close_brace)}
    {
        set_slot_count(3);
        adjust_flags_and_width(*open_brace_);
        adjust_flags_and_width(statements_);
        adjust_flags_and_width(*close_brace_);
    }

    GreenBlock::~GreenBlock() = default;

    void GreenBlock::set_open_brace(GreenPtr<GreenToken> value) noexcept
    {
        open_brace_ = std::move(value);
    }

    void GreenBlock::set_statements(GreenSyntaxList<GreenStatement> value) noexcept
    {
        statements_ = std::move(value);
    }

    void GreenBlock::set_close_brace(GreenPtr<GreenToken> value) noexcept
    {
        close_brace_ = std::move(value);
    }

    Optional<const GreenNode &> GreenBlock::get_slot(std::size_t index) const
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

    [[nodiscard]] SyntaxNode &GreenBlock::create_red(SyntaxLifetime &lifetime,
                                                     const SyntaxNode *parent,
                                                     std::uint32_t position) const
    {
        return lifetime.add<BlockSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenBlock> GreenBlock::with_open_brace(GreenPtr<GreenToken> open_brace) const
    {
        return update(std::move(open_brace), statements_, close_brace_);
    }

    [[nodiscard]] GreenPtr<GreenBlock> GreenBlock::with_statements(GreenSyntaxList<GreenStatement> statements) const
    {
        return update(open_brace_, std::move(statements), close_brace_);
    }

    [[nodiscard]] GreenPtr<GreenBlock> GreenBlock::with_close_brace(GreenPtr<GreenToken> close_brace) const
    {
        return update(open_brace_, statements_, std::move(close_brace));
    }

    GreenPtr<GreenBlock> GreenBlock::update(GreenPtr<GreenToken> open_brace,
                                            GreenSyntaxList<GreenStatement> statements,
                                            GreenPtr<GreenToken> close_brace) const
    {
        if (open_brace == open_brace_ && statements == statements_ && close_brace == close_brace_)
            return shared_from_this();

        return make_ref_counted<const GreenBlock>(std::move(open_brace), std::move(statements), std::move(close_brace));
    }

    RefCountPtr<GreenNode> GreenBlock::clone_internal() const
    {
        return make_ref_counted<GreenBlock>(open_brace_, statements_, close_brace_);
    }

    GreenReturnStatement::GreenReturnStatement(GreenPtr<GreenToken> return_keyword,
                                               GreenPtr<GreenExpression> expression,
                                               GreenPtr<GreenToken> semicolon,
                                               DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::return_statement, std::move(diagnostics)},
          return_keyword_{std::move(return_keyword)}, expression_{std::move(expression)},
          semicolon_{std::move(semicolon)}
    {
        set_slot_count(3);
        adjust_flags_and_width(*return_keyword_);
        if (expression_ != nullptr)
            adjust_flags_and_width(*expression_);
        adjust_flags_and_width(*semicolon_);
    }

    GreenReturnStatement::~GreenReturnStatement() = default;

    void GreenReturnStatement::set_return_keyword(GreenPtr<GreenToken> value) noexcept
    {
        return_keyword_ = std::move(value);
    }

    void GreenReturnStatement::set_expression(GreenPtr<GreenExpression> value) noexcept
    {
        expression_ = std::move(value);
    }

    void GreenReturnStatement::set_semicolon(GreenPtr<GreenToken> value) noexcept
    {
        semicolon_ = std::move(value);
    }

    Optional<const GreenNode &> GreenReturnStatement::get_slot(std::size_t index) const
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

    [[nodiscard]] SyntaxNode &GreenReturnStatement::create_red(SyntaxLifetime &lifetime,
                                                               const SyntaxNode *parent,
                                                               std::uint32_t position) const
    {
        return lifetime.add<ReturnStatementSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenReturnStatement> GreenReturnStatement::with_return_keyword(
        GreenPtr<GreenToken> return_keyword) const
    {
        return update(std::move(return_keyword), expression_, semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenReturnStatement> GreenReturnStatement::with_expression(
        GreenPtr<GreenExpression> expression) const
    {
        return update(return_keyword_, std::move(expression), semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenReturnStatement> GreenReturnStatement::with_semicolon(
        GreenPtr<GreenToken> semicolon) const
    {
        return update(return_keyword_, expression_, std::move(semicolon));
    }

    GreenPtr<GreenReturnStatement> GreenReturnStatement::update(GreenPtr<GreenToken> return_keyword,
                                                                GreenPtr<GreenExpression> expression,
                                                                GreenPtr<GreenToken> semicolon) const
    {
        if (return_keyword == return_keyword_ && expression == expression_ && semicolon == semicolon_)
            return shared_from_this();

        return make_ref_counted<const GreenReturnStatement>(std::move(return_keyword),
                                                            std::move(expression),
                                                            std::move(semicolon));
    }

    RefCountPtr<GreenNode> GreenReturnStatement::clone_internal() const
    {
        return make_ref_counted<GreenReturnStatement>(return_keyword_, expression_, semicolon_);
    }

    GreenExpressionStatement::GreenExpressionStatement(GreenPtr<GreenExpression> expression,
                                                       GreenPtr<GreenToken> semicolon,
                                                       DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::expression_statement, std::move(diagnostics)}, expression_{std::move(expression)},
          semicolon_{std::move(semicolon)}
    {
        set_slot_count(2);
        adjust_flags_and_width(*expression_);
        adjust_flags_and_width(*semicolon_);
    }

    GreenExpressionStatement::~GreenExpressionStatement() = default;

    void GreenExpressionStatement::set_expression(GreenPtr<GreenExpression> value) noexcept
    {
        expression_ = std::move(value);
    }

    void GreenExpressionStatement::set_semicolon(GreenPtr<GreenToken> value) noexcept
    {
        semicolon_ = std::move(value);
    }

    Optional<const GreenNode &> GreenExpressionStatement::get_slot(std::size_t index) const
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

    [[nodiscard]] SyntaxNode &GreenExpressionStatement::create_red(SyntaxLifetime &lifetime,
                                                                   const SyntaxNode *parent,
                                                                   std::uint32_t position) const
    {
        return lifetime.add<ExpressionStatementSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenExpressionStatement> GreenExpressionStatement::with_expression(
        GreenPtr<GreenExpression> expression) const
    {
        return update(std::move(expression), semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenExpressionStatement> GreenExpressionStatement::with_semicolon(
        GreenPtr<GreenToken> semicolon) const
    {
        return update(expression_, std::move(semicolon));
    }

    GreenPtr<GreenExpressionStatement> GreenExpressionStatement::update(GreenPtr<GreenExpression> expression,
                                                                        GreenPtr<GreenToken> semicolon) const
    {
        if (expression == expression_ && semicolon == semicolon_)
            return shared_from_this();

        return make_ref_counted<const GreenExpressionStatement>(std::move(expression), std::move(semicolon));
    }

    RefCountPtr<GreenNode> GreenExpressionStatement::clone_internal() const
    {
        return make_ref_counted<GreenExpressionStatement>(expression_, semicolon_);
    }

    GreenIfStatement::GreenIfStatement(GreenPtr<GreenToken> if_keyword,
                                       GreenPtr<GreenToken> open_paren,
                                       GreenPtr<GreenExpression> condition,
                                       GreenPtr<GreenToken> close_paren,
                                       GreenPtr<GreenBlock> block,
                                       GreenPtr<GreenElseClause> else_clause,
                                       DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::if_statement, std::move(diagnostics)}, if_keyword_{std::move(if_keyword)},
          open_paren_{std::move(open_paren)}, condition_{std::move(condition)}, close_paren_{std::move(close_paren)},
          block_{std::move(block)}, else_clause_{std::move(else_clause)}
    {
        set_slot_count(6);
        adjust_flags_and_width(*if_keyword_);
        adjust_flags_and_width(*open_paren_);
        adjust_flags_and_width(*condition_);
        adjust_flags_and_width(*close_paren_);
        adjust_flags_and_width(*block_);
        if (else_clause_ != nullptr)
            adjust_flags_and_width(*else_clause_);
    }

    GreenIfStatement::~GreenIfStatement() = default;

    void GreenIfStatement::set_if_keyword(GreenPtr<GreenToken> value) noexcept
    {
        if_keyword_ = std::move(value);
    }

    void GreenIfStatement::set_open_paren(GreenPtr<GreenToken> value) noexcept
    {
        open_paren_ = std::move(value);
    }

    void GreenIfStatement::set_condition(GreenPtr<GreenExpression> value) noexcept
    {
        condition_ = std::move(value);
    }

    void GreenIfStatement::set_close_paren(GreenPtr<GreenToken> value) noexcept
    {
        close_paren_ = std::move(value);
    }

    void GreenIfStatement::set_block(GreenPtr<GreenBlock> value) noexcept
    {
        block_ = std::move(value);
    }

    void GreenIfStatement::set_else_clause(GreenPtr<GreenElseClause> value) noexcept
    {
        else_clause_ = std::move(value);
    }

    Optional<const GreenNode &> GreenIfStatement::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *if_keyword_;
            case 1:
                return *open_paren_;
            case 2:
                return *condition_;
            case 3:
                return *close_paren_;
            case 4:
                return *block_;
            case 5:
                return else_clause_.get();
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenIfStatement::create_red(SyntaxLifetime &lifetime,
                                                           const SyntaxNode *parent,
                                                           std::uint32_t position) const
    {
        return lifetime.add<IfStatementSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenIfStatement> GreenIfStatement::with_if_keyword(GreenPtr<GreenToken> if_keyword) const
    {
        return update(std::move(if_keyword), open_paren_, condition_, close_paren_, block_, else_clause_);
    }

    [[nodiscard]] GreenPtr<GreenIfStatement> GreenIfStatement::with_open_paren(GreenPtr<GreenToken> open_paren) const
    {
        return update(if_keyword_, std::move(open_paren), condition_, close_paren_, block_, else_clause_);
    }

    [[nodiscard]] GreenPtr<GreenIfStatement> GreenIfStatement::with_condition(GreenPtr<GreenExpression> condition) const
    {
        return update(if_keyword_, open_paren_, std::move(condition), close_paren_, block_, else_clause_);
    }

    [[nodiscard]] GreenPtr<GreenIfStatement> GreenIfStatement::with_close_paren(GreenPtr<GreenToken> close_paren) const
    {
        return update(if_keyword_, open_paren_, condition_, std::move(close_paren), block_, else_clause_);
    }

    [[nodiscard]] GreenPtr<GreenIfStatement> GreenIfStatement::with_block(GreenPtr<GreenBlock> block) const
    {
        return update(if_keyword_, open_paren_, condition_, close_paren_, std::move(block), else_clause_);
    }

    [[nodiscard]] GreenPtr<GreenIfStatement> GreenIfStatement::with_else_clause(
        GreenPtr<GreenElseClause> else_clause) const
    {
        return update(if_keyword_, open_paren_, condition_, close_paren_, block_, std::move(else_clause));
    }

    GreenPtr<GreenIfStatement> GreenIfStatement::update(GreenPtr<GreenToken> if_keyword,
                                                        GreenPtr<GreenToken> open_paren,
                                                        GreenPtr<GreenExpression> condition,
                                                        GreenPtr<GreenToken> close_paren,
                                                        GreenPtr<GreenBlock> block,
                                                        GreenPtr<GreenElseClause> else_clause) const
    {
        if (if_keyword == if_keyword_ && open_paren == open_paren_ && condition == condition_ &&
            close_paren == close_paren_ && block == block_ && else_clause == else_clause_)
            return shared_from_this();

        return make_ref_counted<const GreenIfStatement>(std::move(if_keyword),
                                                        std::move(open_paren),
                                                        std::move(condition),
                                                        std::move(close_paren),
                                                        std::move(block),
                                                        std::move(else_clause));
    }

    RefCountPtr<GreenNode> GreenIfStatement::clone_internal() const
    {
        return make_ref_counted<GreenIfStatement>(if_keyword_,
                                                  open_paren_,
                                                  condition_,
                                                  close_paren_,
                                                  block_,
                                                  else_clause_);
    }

    GreenWhileStatement::GreenWhileStatement(GreenPtr<GreenToken> while_keyword,
                                             GreenPtr<GreenToken> open_paren,
                                             GreenPtr<GreenExpression> condition,
                                             GreenPtr<GreenToken> close_paren,
                                             GreenPtr<GreenBlock> block,
                                             DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::while_statement, std::move(diagnostics)}, while_keyword_{std::move(while_keyword)},
          open_paren_{std::move(open_paren)}, condition_{std::move(condition)}, close_paren_{std::move(close_paren)},
          block_{std::move(block)}
    {
        set_slot_count(5);
        adjust_flags_and_width(*while_keyword_);
        adjust_flags_and_width(*open_paren_);
        adjust_flags_and_width(*condition_);
        adjust_flags_and_width(*close_paren_);
        adjust_flags_and_width(*block_);
    }

    GreenWhileStatement::~GreenWhileStatement() = default;

    void GreenWhileStatement::set_while_keyword(GreenPtr<GreenToken> value) noexcept
    {
        while_keyword_ = std::move(value);
    }

    void GreenWhileStatement::set_open_paren(GreenPtr<GreenToken> value) noexcept
    {
        open_paren_ = std::move(value);
    }

    void GreenWhileStatement::set_condition(GreenPtr<GreenExpression> value) noexcept
    {
        condition_ = std::move(value);
    }

    void GreenWhileStatement::set_close_paren(GreenPtr<GreenToken> value) noexcept
    {
        close_paren_ = std::move(value);
    }

    void GreenWhileStatement::set_block(GreenPtr<GreenBlock> value) noexcept
    {
        block_ = std::move(value);
    }

    Optional<const GreenNode &> GreenWhileStatement::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *while_keyword_;
            case 1:
                return *open_paren_;
            case 2:
                return *condition_;
            case 3:
                return *close_paren_;
            case 4:
                return *block_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenWhileStatement::create_red(SyntaxLifetime &lifetime,
                                                              const SyntaxNode *parent,
                                                              std::uint32_t position) const
    {
        return lifetime.add<WhileStatementSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenWhileStatement> GreenWhileStatement::with_while_keyword(
        GreenPtr<GreenToken> while_keyword) const
    {
        return update(std::move(while_keyword), open_paren_, condition_, close_paren_, block_);
    }

    [[nodiscard]] GreenPtr<GreenWhileStatement> GreenWhileStatement::with_open_paren(
        GreenPtr<GreenToken> open_paren) const
    {
        return update(while_keyword_, std::move(open_paren), condition_, close_paren_, block_);
    }

    [[nodiscard]] GreenPtr<GreenWhileStatement> GreenWhileStatement::with_condition(
        GreenPtr<GreenExpression> condition) const
    {
        return update(while_keyword_, open_paren_, std::move(condition), close_paren_, block_);
    }

    [[nodiscard]] GreenPtr<GreenWhileStatement> GreenWhileStatement::with_close_paren(
        GreenPtr<GreenToken> close_paren) const
    {
        return update(while_keyword_, open_paren_, condition_, std::move(close_paren), block_);
    }

    [[nodiscard]] GreenPtr<GreenWhileStatement> GreenWhileStatement::with_block(GreenPtr<GreenBlock> block) const
    {
        return update(while_keyword_, open_paren_, condition_, close_paren_, std::move(block));
    }

    GreenPtr<GreenWhileStatement> GreenWhileStatement::update(GreenPtr<GreenToken> while_keyword,
                                                              GreenPtr<GreenToken> open_paren,
                                                              GreenPtr<GreenExpression> condition,
                                                              GreenPtr<GreenToken> close_paren,
                                                              GreenPtr<GreenBlock> block) const
    {
        if (while_keyword == while_keyword_ && open_paren == open_paren_ && condition == condition_ &&
            close_paren == close_paren_ && block == block_)
            return shared_from_this();

        return make_ref_counted<const GreenWhileStatement>(std::move(while_keyword),
                                                           std::move(open_paren),
                                                           std::move(condition),
                                                           std::move(close_paren),
                                                           std::move(block));
    }

    RefCountPtr<GreenNode> GreenWhileStatement::clone_internal() const
    {
        return make_ref_counted<GreenWhileStatement>(while_keyword_, open_paren_, condition_, close_paren_, block_);
    }

    GreenLoopStatement::GreenLoopStatement(GreenPtr<GreenToken> loop_keyword,
                                           GreenPtr<GreenBlock> block,
                                           DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::loop_statement, std::move(diagnostics)}, loop_keyword_{std::move(loop_keyword)},
          block_{std::move(block)}
    {
        set_slot_count(2);
        adjust_flags_and_width(*loop_keyword_);
        adjust_flags_and_width(*block_);
    }

    GreenLoopStatement::~GreenLoopStatement() = default;

    void GreenLoopStatement::set_loop_keyword(GreenPtr<GreenToken> value) noexcept
    {
        loop_keyword_ = std::move(value);
    }

    void GreenLoopStatement::set_block(GreenPtr<GreenBlock> value) noexcept
    {
        block_ = std::move(value);
    }

    Optional<const GreenNode &> GreenLoopStatement::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *loop_keyword_;
            case 1:
                return *block_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenLoopStatement::create_red(SyntaxLifetime &lifetime,
                                                             const SyntaxNode *parent,
                                                             std::uint32_t position) const
    {
        return lifetime.add<LoopStatementSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenLoopStatement> GreenLoopStatement::with_loop_keyword(
        GreenPtr<GreenToken> loop_keyword) const
    {
        return update(std::move(loop_keyword), block_);
    }

    [[nodiscard]] GreenPtr<GreenLoopStatement> GreenLoopStatement::with_block(GreenPtr<GreenBlock> block) const
    {
        return update(loop_keyword_, std::move(block));
    }

    GreenPtr<GreenLoopStatement> GreenLoopStatement::update(GreenPtr<GreenToken> loop_keyword,
                                                            GreenPtr<GreenBlock> block) const
    {
        if (loop_keyword == loop_keyword_ && block == block_)
            return shared_from_this();

        return make_ref_counted<const GreenLoopStatement>(std::move(loop_keyword), std::move(block));
    }

    RefCountPtr<GreenNode> GreenLoopStatement::clone_internal() const
    {
        return make_ref_counted<GreenLoopStatement>(loop_keyword_, block_);
    }

    GreenForStatement::GreenForStatement(GreenPtr<GreenToken> for_keyword,
                                         GreenPtr<GreenToken> open_paren,
                                         GreenPtr<GreenVariableDeclarationStatement> declaration,
                                         GreenSeparatedList<GreenExpression> initializers,
                                         GreenPtr<GreenToken> first_semicolon,
                                         GreenPtr<GreenExpression> condition,
                                         GreenPtr<GreenToken> second_semicolon,
                                         GreenSeparatedList<GreenExpression> incrementors,
                                         GreenPtr<GreenToken> close_paren,
                                         GreenPtr<GreenBlock> block,
                                         DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::for_statement, std::move(diagnostics)}, for_keyword_{std::move(for_keyword)},
          open_paren_{std::move(open_paren)}, declaration_{std::move(declaration)},
          initializers_{std::move(initializers)}, first_semicolon_{std::move(first_semicolon)},
          condition_{std::move(condition)}, second_semicolon_{std::move(second_semicolon)},
          incrementors_{std::move(incrementors)}, close_paren_{std::move(close_paren)}, block_{std::move(block)}
    {
        set_slot_count(10);
        adjust_flags_and_width(*for_keyword_);
        adjust_flags_and_width(*open_paren_);
        if (declaration_ != nullptr)
            adjust_flags_and_width(*declaration_);
        adjust_flags_and_width(initializers_);
        adjust_flags_and_width(*first_semicolon_);
        if (condition_ != nullptr)
            adjust_flags_and_width(*condition_);
        adjust_flags_and_width(*second_semicolon_);
        adjust_flags_and_width(incrementors_);
        adjust_flags_and_width(*close_paren_);
        adjust_flags_and_width(*block_);
    }

    GreenForStatement::~GreenForStatement() = default;

    void GreenForStatement::set_for_keyword(GreenPtr<GreenToken> value) noexcept
    {
        for_keyword_ = std::move(value);
    }

    void GreenForStatement::set_open_paren(GreenPtr<GreenToken> value) noexcept
    {
        open_paren_ = std::move(value);
    }

    void GreenForStatement::set_declaration(GreenPtr<GreenVariableDeclarationStatement> value) noexcept
    {
        declaration_ = std::move(value);
    }

    void GreenForStatement::set_initializers(GreenSeparatedList<GreenExpression> value) noexcept
    {
        initializers_ = std::move(value);
    }

    void GreenForStatement::set_first_semicolon(GreenPtr<GreenToken> value) noexcept
    {
        first_semicolon_ = std::move(value);
    }

    void GreenForStatement::set_condition(GreenPtr<GreenExpression> value) noexcept
    {
        condition_ = std::move(value);
    }

    void GreenForStatement::set_second_semicolon(GreenPtr<GreenToken> value) noexcept
    {
        second_semicolon_ = std::move(value);
    }

    void GreenForStatement::set_incrementors(GreenSeparatedList<GreenExpression> value) noexcept
    {
        incrementors_ = std::move(value);
    }

    void GreenForStatement::set_close_paren(GreenPtr<GreenToken> value) noexcept
    {
        close_paren_ = std::move(value);
    }

    void GreenForStatement::set_block(GreenPtr<GreenBlock> value) noexcept
    {
        block_ = std::move(value);
    }

    Optional<const GreenNode &> GreenForStatement::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *for_keyword_;
            case 1:
                return *open_paren_;
            case 2:
                return declaration_.get();
            case 3:
                return initializers_.node();
            case 4:
                return *first_semicolon_;
            case 5:
                return condition_.get();
            case 6:
                return *second_semicolon_;
            case 7:
                return incrementors_.node();
            case 8:
                return *close_paren_;
            case 9:
                return *block_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenForStatement::create_red(SyntaxLifetime &lifetime,
                                                            const SyntaxNode *parent,
                                                            std::uint32_t position) const
    {
        return lifetime.add<ForStatementSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenForStatement> GreenForStatement::with_for_keyword(
        GreenPtr<GreenToken> for_keyword) const
    {
        return update(std::move(for_keyword),
                      open_paren_,
                      declaration_,
                      initializers_,
                      first_semicolon_,
                      condition_,
                      second_semicolon_,
                      incrementors_,
                      close_paren_,
                      block_);
    }

    [[nodiscard]] GreenPtr<GreenForStatement> GreenForStatement::with_open_paren(GreenPtr<GreenToken> open_paren) const
    {
        return update(for_keyword_,
                      std::move(open_paren),
                      declaration_,
                      initializers_,
                      first_semicolon_,
                      condition_,
                      second_semicolon_,
                      incrementors_,
                      close_paren_,
                      block_);
    }

    [[nodiscard]] GreenPtr<GreenForStatement> GreenForStatement::with_declaration(
        GreenPtr<GreenVariableDeclarationStatement> declaration) const
    {
        return update(for_keyword_,
                      open_paren_,
                      std::move(declaration),
                      initializers_,
                      first_semicolon_,
                      condition_,
                      second_semicolon_,
                      incrementors_,
                      close_paren_,
                      block_);
    }

    [[nodiscard]] GreenPtr<GreenForStatement> GreenForStatement::with_initializers(
        GreenSeparatedList<GreenExpression> initializers) const
    {
        return update(for_keyword_,
                      open_paren_,
                      declaration_,
                      std::move(initializers),
                      first_semicolon_,
                      condition_,
                      second_semicolon_,
                      incrementors_,
                      close_paren_,
                      block_);
    }

    [[nodiscard]] GreenPtr<GreenForStatement> GreenForStatement::with_first_semicolon(
        GreenPtr<GreenToken> first_semicolon) const
    {
        return update(for_keyword_,
                      open_paren_,
                      declaration_,
                      initializers_,
                      std::move(first_semicolon),
                      condition_,
                      second_semicolon_,
                      incrementors_,
                      close_paren_,
                      block_);
    }

    [[nodiscard]] GreenPtr<GreenForStatement> GreenForStatement::with_condition(
        GreenPtr<GreenExpression> condition) const
    {
        return update(for_keyword_,
                      open_paren_,
                      declaration_,
                      initializers_,
                      first_semicolon_,
                      std::move(condition),
                      second_semicolon_,
                      incrementors_,
                      close_paren_,
                      block_);
    }

    [[nodiscard]] GreenPtr<GreenForStatement> GreenForStatement::with_second_semicolon(
        GreenPtr<GreenToken> second_semicolon) const
    {
        return update(for_keyword_,
                      open_paren_,
                      declaration_,
                      initializers_,
                      first_semicolon_,
                      condition_,
                      std::move(second_semicolon),
                      incrementors_,
                      close_paren_,
                      block_);
    }

    [[nodiscard]] GreenPtr<GreenForStatement> GreenForStatement::with_incrementors(
        GreenSeparatedList<GreenExpression> incrementors) const
    {
        return update(for_keyword_,
                      open_paren_,
                      declaration_,
                      initializers_,
                      first_semicolon_,
                      condition_,
                      second_semicolon_,
                      std::move(incrementors),
                      close_paren_,
                      block_);
    }

    [[nodiscard]] GreenPtr<GreenForStatement> GreenForStatement::with_close_paren(
        GreenPtr<GreenToken> close_paren) const
    {
        return update(for_keyword_,
                      open_paren_,
                      declaration_,
                      initializers_,
                      first_semicolon_,
                      condition_,
                      second_semicolon_,
                      incrementors_,
                      std::move(close_paren),
                      block_);
    }

    [[nodiscard]] GreenPtr<GreenForStatement> GreenForStatement::with_block(GreenPtr<GreenBlock> block) const
    {
        return update(for_keyword_,
                      open_paren_,
                      declaration_,
                      initializers_,
                      first_semicolon_,
                      condition_,
                      second_semicolon_,
                      incrementors_,
                      close_paren_,
                      std::move(block));
    }

    GreenPtr<GreenForStatement> GreenForStatement::update(GreenPtr<GreenToken> for_keyword,
                                                          GreenPtr<GreenToken> open_paren,
                                                          GreenPtr<GreenVariableDeclarationStatement> declaration,
                                                          GreenSeparatedList<GreenExpression> initializers,
                                                          GreenPtr<GreenToken> first_semicolon,
                                                          GreenPtr<GreenExpression> condition,
                                                          GreenPtr<GreenToken> second_semicolon,
                                                          GreenSeparatedList<GreenExpression> incrementors,
                                                          GreenPtr<GreenToken> close_paren,
                                                          GreenPtr<GreenBlock> block) const
    {
        if (for_keyword == for_keyword_ && open_paren == open_paren_ && declaration == declaration_ &&
            initializers == initializers_ && first_semicolon == first_semicolon_ && condition == condition_ &&
            second_semicolon == second_semicolon_ && incrementors == incrementors_ && close_paren == close_paren_ &&
            block == block_)
            return shared_from_this();

        return make_ref_counted<const GreenForStatement>(std::move(for_keyword),
                                                         std::move(open_paren),
                                                         std::move(declaration),
                                                         std::move(initializers),
                                                         std::move(first_semicolon),
                                                         std::move(condition),
                                                         std::move(second_semicolon),
                                                         std::move(incrementors),
                                                         std::move(close_paren),
                                                         std::move(block));
    }

    RefCountPtr<GreenNode> GreenForStatement::clone_internal() const
    {
        return make_ref_counted<GreenForStatement>(for_keyword_,
                                                   open_paren_,
                                                   declaration_,
                                                   initializers_,
                                                   first_semicolon_,
                                                   condition_,
                                                   second_semicolon_,
                                                   incrementors_,
                                                   close_paren_,
                                                   block_);
    }

    GreenBreakStatement::GreenBreakStatement(GreenPtr<GreenToken> keyword,
                                             GreenPtr<GreenToken> semicolon,
                                             DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::break_statement, std::move(diagnostics)}, keyword_{std::move(keyword)},
          semicolon_{std::move(semicolon)}
    {
        set_slot_count(2);
        adjust_flags_and_width(*keyword_);
        adjust_flags_and_width(*semicolon_);
    }

    GreenBreakStatement::~GreenBreakStatement() = default;

    void GreenBreakStatement::set_keyword(GreenPtr<GreenToken> value) noexcept
    {
        keyword_ = std::move(value);
    }

    void GreenBreakStatement::set_semicolon(GreenPtr<GreenToken> value) noexcept
    {
        semicolon_ = std::move(value);
    }

    Optional<const GreenNode &> GreenBreakStatement::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *keyword_;
            case 1:
                return *semicolon_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenBreakStatement::create_red(SyntaxLifetime &lifetime,
                                                              const SyntaxNode *parent,
                                                              std::uint32_t position) const
    {
        return lifetime.add<BreakStatementSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenBreakStatement> GreenBreakStatement::with_keyword(GreenPtr<GreenToken> keyword) const
    {
        return update(std::move(keyword), semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenBreakStatement> GreenBreakStatement::with_semicolon(
        GreenPtr<GreenToken> semicolon) const
    {
        return update(keyword_, std::move(semicolon));
    }

    GreenPtr<GreenBreakStatement> GreenBreakStatement::update(GreenPtr<GreenToken> keyword,
                                                              GreenPtr<GreenToken> semicolon) const
    {
        if (keyword == keyword_ && semicolon == semicolon_)
            return shared_from_this();

        return make_ref_counted<const GreenBreakStatement>(std::move(keyword), std::move(semicolon));
    }

    RefCountPtr<GreenNode> GreenBreakStatement::clone_internal() const
    {
        return make_ref_counted<GreenBreakStatement>(keyword_, semicolon_);
    }

    GreenContinueStatement::GreenContinueStatement(GreenPtr<GreenToken> keyword,
                                                   GreenPtr<GreenToken> semicolon,
                                                   DiagnosticInfoList diagnostics)
        : GreenStatement{SyntaxKind::continue_statement, std::move(diagnostics)}, keyword_{std::move(keyword)},
          semicolon_{std::move(semicolon)}
    {
        set_slot_count(2);
        adjust_flags_and_width(*keyword_);
        adjust_flags_and_width(*semicolon_);
    }

    GreenContinueStatement::~GreenContinueStatement() = default;

    void GreenContinueStatement::set_keyword(GreenPtr<GreenToken> value) noexcept
    {
        keyword_ = std::move(value);
    }

    void GreenContinueStatement::set_semicolon(GreenPtr<GreenToken> value) noexcept
    {
        semicolon_ = std::move(value);
    }

    Optional<const GreenNode &> GreenContinueStatement::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *keyword_;
            case 1:
                return *semicolon_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenContinueStatement::create_red(SyntaxLifetime &lifetime,
                                                                 const SyntaxNode *parent,
                                                                 std::uint32_t position) const
    {
        return lifetime.add<ContinueStatementSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenContinueStatement> GreenContinueStatement::with_keyword(
        GreenPtr<GreenToken> keyword) const
    {
        return update(std::move(keyword), semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenContinueStatement> GreenContinueStatement::with_semicolon(
        GreenPtr<GreenToken> semicolon) const
    {
        return update(keyword_, std::move(semicolon));
    }

    GreenPtr<GreenContinueStatement> GreenContinueStatement::update(GreenPtr<GreenToken> keyword,
                                                                    GreenPtr<GreenToken> semicolon) const
    {
        if (keyword == keyword_ && semicolon == semicolon_)
            return shared_from_this();

        return make_ref_counted<const GreenContinueStatement>(std::move(keyword), std::move(semicolon));
    }

    RefCountPtr<GreenNode> GreenContinueStatement::clone_internal() const
    {
        return make_ref_counted<GreenContinueStatement>(keyword_, semicolon_);
    }
} // namespace prism
