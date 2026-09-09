// @file CompilerDriver.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Binding;
using Prism.Core.BoundTree;
using Prism.Core.FlowAnalysis;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Symbols.Source;
using Prism.Core.Symbols.Synthesized;
using ZLinq;

namespace Prism.Core.Compiling;

internal static class CompilerDriver
{
    public static BoundVariableInitializer BindVariableInitializer(
        Compilation compilation,
        VariableSymbol variable
    )
    {
        if (variable is not SourceGlobalVariableSymbol { Syntax: { Initializer: not null } syntax })
            return new BoundVariableInitializer(variable, []);
        var binderFactory = compilation.GetBinderFactory(syntax.SyntaxTree);
        var binder = binderFactory.GetBinder(syntax);

        using var context = BindingContext.Create();
        var initializer = binder.BindInitializer(variable, syntax.Initializer, context);
        if (context.HasErrors)
        {
            initializer = initializer with { HasErrors = true };
        }

        return new BoundVariableInitializer(variable, initializer, context.CollectDiagnostics());
    }

    public static BoundFunctionBody BindFunctionBody(
        Compilation compilation,
        FunctionSymbol function
    )
    {
        BoundStatement? body;
        using var context = BindingContext.Create();
        switch (function)
        {
            case SourceFunctionSymbol { Syntax: var syntax }:
            {
                var binderFactory = compilation.GetBinderFactory(syntax.SyntaxTree);

                if (syntax.Body is not null)
                {
                    var binder = binderFactory.GetBinder(syntax.Body);
                    body = binder.BindStatement(syntax.Body, function.ReturnType, context);
                }
                else if (syntax.ExpressionBody is not null)
                {
                    var binder = binderFactory.GetBinder(syntax.ExpressionBody);
                    body = binder.BindExpressionBody(
                        syntax.ExpressionBody,
                        function.ReturnType,
                        context
                    );
                }
                else
                {
                    body = null;
                }

                if (context.HasErrors && body is not null)
                {
                    body = body with { HasErrors = true };
                }

                break;
            }
            case SynthesizedGlobalConstructor:
            {
                ImmutableArray<BoundStatement>.Builder? builder = null;
                using var globals = compilation
                    .GetGlobalVariables()
                    .AsValueEnumerable()
                    .Select(compilation.GetBoundInitializer)
                    .ToArrayPool();

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                var hasErrors = false;
                foreach (var global in globals.Span)
                {
                    if (!global.HasInitializer || global.ConstantValue is not null)
                        continue;

                    builder ??= ImmutableArray.CreateBuilder<BoundStatement>();

                    hasErrors |= global.HasErrors;

                    var reference = new BoundVariableAccess(
                        global.Initializer.Syntax,
                        global.Variable
                    );
                    var assignment = new BoundAssignmentOperation(
                        global.Initializer.Syntax,
                        compilation.GetSpecialType(SpecialType.Void),
                        reference,
                        global.Initializer,
                        AssignmentOperation.Simple
                    );
                    builder.Add(
                        new BoundExpressionStatement(global.Initializer.Syntax, assignment)
                    );
                }

                if (builder is not null)
                {
                    body = new BoundBlock(
                        compilation.SyntaxTrees[0].Root,
                        builder.DrainToImmutable()
                    )
                    {
                        HasErrors = hasErrors,
                    };
                }
                else
                {
                    body = null;
                }

                break;
            }
            default:
                body = null;
                break;
        }

        if (body is null)
            return new BoundFunctionBody(function, context.CollectDiagnostics());

        var analysis = FunctionAnalysisBuilder.Build(body);
        return new BoundFunctionBody(function, body, analysis, context.CollectDiagnostics());
    }
}
