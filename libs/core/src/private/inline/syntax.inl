#ifndef SYNTAX_CATEGORY
#define SYNTAX_CATEGORY(name, class_name)
#endif

#ifndef SYNTAX_KIND
#define SYNTAX_KIND(name, class_name)
#endif

#ifndef DERIVED_SYNTAX_KIND
#define DERIVED_SYNTAX_KIND(name, class_name, base) SYNTAX_KIND(name, class_name)
#endif

#ifndef TYPE_SYNTAX
#define TYPE_SYNTAX(name, class_name) DERIVED_SYNTAX_KIND(name, class_name, TypeSyntax)
#endif

#ifndef EXPRESSION_SYNTAX
#define EXPRESSION_SYNTAX(name, class_name) DERIVED_SYNTAX_KIND(name, class_name, ExpressionSyntax)
#endif

#ifndef STATEMENT_SYNTAX
#define STATEMENT_SYNTAX(name, class_name) DERIVED_SYNTAX_KIND(name, class_name, StatementSyntax)
#endif

#ifndef DECLARATION_SYNTAX
#define DECLARATION_SYNTAX(name, class_name) DERIVED_SYNTAX_KIND(name, class_name, DeclarationSyntax)
#endif

SYNTAX_CATEGORY(type, TypeSyntax)
SYNTAX_CATEGORY(expression, ExpressionSyntax)
SYNTAX_CATEGORY(statement, StatementSyntax)
SYNTAX_CATEGORY(declaration, DeclarationSyntax)

TYPE_SYNTAX(built_in_type, BuiltInTypeSyntax)
TYPE_SYNTAX(named_type, NamedTypeSyntax)

EXPRESSION_SYNTAX(literal_expression, LiteralExpressionSyntax)
EXPRESSION_SYNTAX(identifier_expression, IdentifierExpressionSyntax)
EXPRESSION_SYNTAX(binary_expression, BinaryExpressionSyntax)
EXPRESSION_SYNTAX(unary_expression, UnaryExpressionSyntax)
EXPRESSION_SYNTAX(ternary_expression, TernaryExpressionSyntax)
EXPRESSION_SYNTAX(invocation_expression, InvocationExpressionSyntax)

DECLARATION_SYNTAX(variable_declaration, VariableDeclarationSyntax)
DECLARATION_SYNTAX(function_declaration, FunctionDeclarationSyntax)
DECLARATION_SYNTAX(parameter_declaration, ParameterDeclarationSyntax)
DECLARATION_SYNTAX(empty_declaration, EmptyDeclarationSyntax)

STATEMENT_SYNTAX(variable_declaration_statement, VariableDeclarationStatementSyntax)
STATEMENT_SYNTAX(block_statement, BlockStatementSyntax)
STATEMENT_SYNTAX(expression_statement, ExpressionStatementSyntax)
STATEMENT_SYNTAX(return_statement, ReturnStatementSyntax)
STATEMENT_SYNTAX(empty_statement, EmptyStatementSyntax)

SYNTAX_KIND(compilation_unit, CompilationUnitSyntax)

#undef SYNTAX_CATEGORY
#undef SYNTAX_KIND
#undef DERIVED_SYNTAX_KIND
#undef TYPE_SYNTAX
#undef EXPRESSION_SYNTAX
#undef STATEMENT_SYNTAX
#undef DECLARATION_SYNTAX
