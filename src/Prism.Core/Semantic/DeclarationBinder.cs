// @file DeclarationBinder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;

namespace Prism.Core.Semantic;

public static class DeclarationBinder
{
    public static DeclarationScope ScanDeclarations(this CompilationUnitSyntax syntax)
    {
        var builder = new DeclarationScopeBuilder(syntax.Declarations.Length);
        foreach (var declaration in syntax.Declarations)
        {
            builder.BindDeclaration(declaration);
        }

        return builder.Build();
    }

    extension(DeclarationScopeBuilder builder)
    {
        private void BindDeclaration(DeclarationSyntax declaration)
        {
            switch (declaration)
            {
                case VariableDeclarationSyntax variable:
                    builder.BindDeclaration(variable);
                    break;
                case FunctionDeclarationSyntax function:
                    builder.BindDeclaration(function);
                    break;
                case ParameterDeclarationSyntax parameter:
                    builder.BindDeclaration(parameter);
                    break;
                case EmptyDeclarationSyntax:
                    // Not a valid declaration
                    break;
            }
        }

        private void BindDeclaration(VariableDeclarationSyntax variable)
        {
            builder.AddDeclaration(variable.Identifier.Name, variable);
        }

        private void BindDeclaration(FunctionDeclarationSyntax function)
        {
            var childBuilder = new DeclarationScopeBuilder(function.Parameters.Length);
            foreach (var declaration in function.Parameters)
            {
                childBuilder.BindDeclaration(declaration);
            }
            builder.AddDeclaration(function.Identifier.Name, function, childBuilder);
        }

        private void BindDeclaration(ParameterDeclarationSyntax parameter)
        {
            builder.AddDeclaration(parameter.Identifier.Name, parameter);
        }
    }
}
