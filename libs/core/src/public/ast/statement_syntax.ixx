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
import prism.core.source.source_file;

namespace prism
{
    export struct VariableDeclarationSyntax;
    export struct FunctionDeclarationSyntax;
    export struct ReturnStatementSyntax;
    export struct ExpressionStatementSyntax;
    export struct BlockSyntax;

    export using DeclarationSyntax =
        std::variant<VariableDeclarationSyntax, FunctionDeclarationSyntax, EmptySyntax, ErrorSyntax>;

    export using StatementSyntax = std::
        variant<VariableDeclarationSyntax, ExpressionStatementSyntax, ReturnStatementSyntax, BlockSyntax, EmptySyntax>;

    struct BlockSyntax
    {
        std::vector<StatementSyntax> statements;
    };

    struct VariableDeclarationSyntax
    {
        IdentifierSyntax name;
        std::optional<TypeSyntax> type;
        bool is_mutable = false;
        Modifiers modifiers;
        std::unique_ptr<ExpressionSyntax> initializer;
    };

    export struct ParameterDeclarationSyntax
    {
        IdentifierSyntax name;
        TypeSyntax type;
        bool is_mutable = false;
    };

    using FunctionBodySyntax = std::variant<EmptySyntax, BlockSyntax, ExpressionSyntax>;

    struct FunctionDeclarationSyntax
    {
        IdentifierSyntax name;
        std::optional<TypeSyntax> return_type;
        std::vector<ParameterDeclarationSyntax> parameters;
        FunctionBodySyntax body;
        Modifiers modifiers;
    };

    struct ReturnStatementSyntax
    {
        ExpressionSyntax expression;
    };

    struct ExpressionStatementSyntax
    {
        ExpressionSyntax expression;
    };

    export struct CompilationUnitSyntax
    {
        std::vector<DeclarationSyntax> declarations;
    };

    export constexpr SourceRange get_range(const DeclarationSyntax &syntax)
    {
        return std::visit(
            Overload{
                [](const VariableDeclarationSyntax &variable) { return get_range(variable.name); },
                [](const FunctionDeclarationSyntax &function) { return get_range(function.name); },
                [](const EmptySyntax &empty) { return empty.range; },
                [](const ErrorSyntax &error) { return error.range; },
            },
            syntax);
    }
} // namespace prism
