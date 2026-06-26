/**
 * @file statement_syntax.ixx
 * @author Francesco Corso
 * @date 6/24/2026
 * @brief
 */
export module prism.core.ast.statement_syntax;

import std;
import prism.core.ast.expression_syntax;
import prism.core.ast.type_syntax;
import prism.core.util;
import prism.core.lexer.token;
import prism.core.ast.common_syntax;

namespace prism
{
    export struct VariableDeclarationSyntax
    {
        IdentifierSyntax name;
        std::optional<TypeSyntax> type;
        bool is_mutable = false;
        std::unique_ptr<ExpressionSyntax> initializer;
    };

    export struct ParameterDeclarationSyntax
    {
        std::string name;
        TypeSyntax type;
        bool is_mutable = false;
    };

    export struct FunctionDeclarationSyntax
    {
        std::string name;
        std::optional<TypeSyntax> return_type;
        std::vector<ParameterDeclarationSyntax> parameters;
    };

    export struct ReturnStatementSyntax
    {
        std::unique_ptr<ExpressionSyntax> expression;
    };

    export struct ExpressionStatementSyntax
    {
        std::unique_ptr<ExpressionSyntax> expression;
    };

    export struct EmptySyntax
    {
    };

    export constexpr EmptySyntax empty_syntax{};

    export using DeclarationSyntax =
        std::variant<VariableDeclarationSyntax, FunctionDeclarationSyntax, EmptySyntax, ErrorSyntax>;

    export using StatementSyntax =
        std::variant<VariableDeclarationSyntax, ExpressionStatementSyntax, ReturnStatementSyntax, EmptySyntax>;

    export struct CompilationUnitSyntax
    {
        std::vector<DeclarationSyntax> declarations;
    };
} // namespace prism
