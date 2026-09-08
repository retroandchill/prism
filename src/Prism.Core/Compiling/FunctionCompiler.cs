// @file CompilerDriver.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Binding;
using Prism.Core.BoundTree;
using Prism.Core.Codegen;
using Prism.Core.Diagnostics;
using Prism.Core.Symbols;
using Prism.Core.Symbols.Source;

namespace Prism.Core.Compiling;

internal readonly record struct BoundResult<TBound>(
    TBound? Bound,
    ImmutableArray<Diagnostic> Diagnostics
)
    where TBound : BoundNode;

internal readonly record struct BoundVariableInitializer(
    VariableSymbol VariableSymbol,
    BoundExpression Initializer
);

internal sealed class FunctionCompiler
{
    private readonly Compilation _compilation;
    private readonly BindingContext _bindingContext;
    private readonly LlvmModuleBuilder? _llvmModuleBuilder;
    private readonly Predicate<Symbol>? _symbolFilter;
    private readonly CancellationToken _cancellationToken;

    private readonly List<BoundVariableInitializer> _variableInitializers = [];

    private FunctionCompiler(
        Compilation compilation,
        LlvmModuleBuilder? llvmModuleBuilder,
        Predicate<Symbol>? symbolFilter,
        BindingContext bindingContext,
        CancellationToken cancellationToken
    )
    {
        _compilation = compilation;
        _bindingContext = bindingContext;
        _llvmModuleBuilder = llvmModuleBuilder;
        _symbolFilter = symbolFilter;
        _cancellationToken = cancellationToken;
    }

    public static void CompileFunctionBodies(
        Compilation compilation,
        LlvmModuleBuilder? llvmModuleBuilder,
        Predicate<Symbol>? symbolFilter,
        BindingContext bindingContext,
        CancellationToken cancellationToken
    )
    {
        var functionCompiler = new FunctionCompiler(
            compilation,
            llvmModuleBuilder,
            symbolFilter,
            bindingContext,
            cancellationToken
        );
        functionCompiler.CompileGlobalVariables(compilation.Assembly.GlobalNamespace);
    }

    private void CompileGlobalVariables(MemberContainerSymbol container)
    {
        foreach (var member in container.GetMembers())
        {
            switch (member)
            {
                case VariableSymbol variable:
                {
                    var (initializer, diagnostics) =
                        _compilation.GetBoundInitializerWithDiagnostics(variable);
                    if (initializer is not null)
                    {
                        _variableInitializers.Add(
                            new BoundVariableInitializer(variable, initializer)
                        );
                    }
                    _llvmModuleBuilder?.AddGlobalVariable(variable, initializer);
                    _bindingContext.ReportDiagnostics(diagnostics);
                    break;
                }
                case MemberContainerSymbol subContainer:
                    CompileGlobalVariables(subContainer);
                    break;
            }
        }
    }

    private void CompileFunctions(MemberContainerSymbol container)
    {
        foreach (var member in container.GetMembers())
        {
            switch (member)
            {
                case MemberContainerSymbol subContainer:
                    CompileFunctions(subContainer);
                    break;
                case FunctionSymbol functionSymbol:
                    CompileFunction(functionSymbol);
                    break;
            }
        }
    }

    private void CompileFunction(FunctionSymbol functionSymbol) { }

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

    public static BoundResult<BoundStatement> BindFunctionBody(
        Compilation compilation,
        FunctionSymbol function
    )
    {
        if (function is not SourceFunctionSymbol { Syntax: var syntax })
            return new BoundResult<BoundStatement>(null, []);

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

        return new BoundResult<BoundStatement>(body, context.CollectDiagnostics());
    }
}
