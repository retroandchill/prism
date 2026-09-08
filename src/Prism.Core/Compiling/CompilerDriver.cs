// @file CompilerDriver.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Binding;
using Prism.Core.BoundTree;
using Prism.Core.Diagnostics;
using Prism.Core.FlowAnalysis;
using Prism.Core.Symbols;
using Prism.Core.Symbols.Source;

namespace Prism.Core.Compiling;

internal readonly record struct BoundResult<TBound>(
    TBound? Bound,
    ImmutableArray<Diagnostic> Diagnostics
)
    where TBound : BoundNode;

internal static class CompilerDriver
{
    public static BoundResult<BoundExpression> BindVariableInitializer(
        Compilation compilation,
        VariableSymbol variable
    )
    {
        if (variable is not SourceGlobalVariableSymbol { Syntax: { Initializer: not null } syntax })
            return new BoundResult<BoundExpression>(null, []);

        var binderFactory = compilation.GetBinderFactory(syntax.SyntaxTree);
        var binder = binderFactory.GetBinder(syntax);

        using var context = BindingContext.Create();
        var initializer = binder.BindInitializer(variable, syntax.Initializer, context);
        if (context.HasErrors)
        {
            initializer = initializer with { HasErrors = true };
        }
        return new BoundResult<BoundExpression>(initializer, context.CollectDiagnostics());
    }

    public static BoundFunctionBody BindFunctionBody(
        Compilation compilation,
        FunctionSymbol function
    )
    {
        if (function is not SourceFunctionSymbol { Syntax: var syntax })
            return new BoundFunctionBody(function, []);

        var binderFactory = compilation.GetBinderFactory(syntax.SyntaxTree);

        using var context = BindingContext.Create();
        BoundStatement? body;
        if (syntax.Body is not null)
        {
            var binder = binderFactory.GetBinder(syntax.Body);
            body = binder.BindStatement(syntax.Body, function.ReturnType, context);
        }
        else if (syntax.ExpressionBody is not null)
        {
            var binder = binderFactory.GetBinder(syntax.ExpressionBody);
            body = binder.BindExpressionBody(syntax.ExpressionBody, function.ReturnType, context);
        }
        else
        {
            body = null;
        }

        if (context.HasErrors && body is not null)
        {
            body = body with { HasErrors = true };
        }

        if (body is null)
            return new BoundFunctionBody(function, context.CollectDiagnostics());

        var analysis = FunctionBodyAnalysis.Create(body);
        return new BoundFunctionBody(function, body, analysis, context.CollectDiagnostics());
    }
}
