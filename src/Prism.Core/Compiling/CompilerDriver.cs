// @file CompilerDriver.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Prism.Core.Binding;
using Prism.Core.BoundTree;
using Prism.Core.Diagnostics;
using Prism.Core.FlowAnalysis;
using Prism.Core.Metadata;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Symbols.Source;
using Prism.Core.Symbols.Synthesized;
using Prism.Core.Syntax;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Compiling;

internal static class CompilerDriver
{
    public static BoundVariableInitializer BindVariableInitializer(
        Compilation compilation,
        VariableSymbol variable,
        CancellationToken cancellationToken
    )
    {
        if (
            variable
            is not SourceVariableSymbol { Syntax: { Initializer: not null } syntax, IsLocal: false }
        )
            return new BoundVariableInitializer(variable, []);
        var binderFactory = compilation.GetBinderFactory(syntax.SyntaxTree);
        var binder = binderFactory.GetBinder(syntax);

        using var context = BindingContext.Create();
        var initializer = binder.BindInitializer(
            variable,
            syntax.Initializer,
            context,
            cancellationToken
        );
        if (context.HasErrors)
        {
            initializer = initializer with { HasErrors = true };
        }

        return new BoundVariableInitializer(variable, initializer, context.CollectDiagnostics());
    }

    public static BoundFunctionBody BindFunctionBody(
        Compilation compilation,
        FunctionSymbol function,
        CancellationToken cancellationToken
    )
    {
        BoundStatement? body;
        SourceLocation? location = null;
        using var context = BindingContext.Create();
        switch (function)
        {
            case SourceFunctionSymbol { Syntax: var syntax }:
            {
                location = syntax.Location;
                var binderFactory = compilation.GetBinderFactory(syntax.SyntaxTree);

                if (syntax.Body is not null)
                {
                    var binder = binderFactory.GetBinder(syntax.Body);
                    body = binder.BindStatement(
                        syntax.Body,
                        function.ReturnType,
                        context,
                        cancellationToken
                    );
                }
                else if (syntax.ExpressionBody is not null)
                {
                    var binder = binderFactory.GetBinder(syntax.ExpressionBody);
                    body = binder.BindExpressionBody(
                        syntax.ExpressionBody,
                        function.ReturnType,
                        context,
                        cancellationToken
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
            case SourceConstructorSymbol { Syntax: var syntax }:
            {
                location = syntax.Location;
                var binderFactory = compilation.GetBinderFactory(syntax.SyntaxTree);

                if (syntax.Body is not null)
                {
                    var binder = binderFactory.GetBinder(syntax.Body);
                    body = binder.BindStatement(
                        syntax.Body,
                        function.ReturnType,
                        context,
                        cancellationToken
                    );
                }
                else if (syntax.ExpressionBody is not null)
                {
                    var binder = binderFactory.GetBinder(syntax.ExpressionBody);
                    body = binder.BindExpressionBody(
                        syntax.ExpressionBody,
                        function.ReturnType,
                        context,
                        cancellationToken
                    );
                }
                else
                {
                    Debug.Assert(syntax.DefaultKeyword is not null);
                    body = SynthesizeDefaultConstructor(
                        function.ContainingType.RequireNonNull(),
                        compilation,
                        cancellationToken
                    );
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
                    .Select(v => compilation.GetBoundInitializer(v, cancellationToken))
                    .ToArrayPool();

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                var hasErrors = false;
                foreach (var global in globals.Span)
                {
                    if (!global.HasInitializer || global.ConstantValue is not null)
                        continue;

                    cancellationToken.ThrowIfCancellationRequested();
                    builder ??= ImmutableArray.CreateBuilder<BoundStatement>();

                    hasErrors |= global.HasErrors;

                    var reference = new BoundVariableAccess(
                        compilation,
                        global.Initializer.Syntax,
                        global.Variable
                    );
                    var assignment = new BoundAssignmentOperation(
                        compilation,
                        global.Initializer.Syntax,
                        compilation.GetSpecialType(SpecialType.Void),
                        reference,
                        global.Initializer,
                        AssignmentOperation.Simple
                    );
                    builder.Add(
                        new BoundExpressionStatement(
                            compilation,
                            global.Initializer.Syntax,
                            assignment
                        )
                    );
                }

                if (builder is not null)
                {
                    body = new BoundBlock(
                        compilation,
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
            case SynthesizedDefaultConstructor:
                body = SynthesizeDefaultConstructor(
                    function.ContainingType.RequireNonNull(),
                    compilation,
                    cancellationToken
                );
                break;
            default:
                body = null;
                break;
        }

        if (body is null)
            return new BoundFunctionBody(function, location, context.CollectDiagnostics());

        var analysis = FunctionAnalysisBuilder.Build(function, body, context, cancellationToken);
        return new BoundFunctionBody(
            function,
            location,
            body,
            analysis,
            context.CollectDiagnostics()
        );
    }

    private static BoundBlock? SynthesizeDefaultConstructor(
        TypeSymbol type,
        Compilation compilation,
        CancellationToken cancellationToken
    )
    {
        ImmutableArray<BoundStatement>.Builder? builder = null;

        var hasErrors = false;

        var mutableReference = compilation.CreateReferenceTypeSymbol(type, true);
        var typeSyntax = type.DeclaringSyntaxReferences[0].Syntax;
        var voidType = compilation.GetSpecialType(SpecialType.Void);

        var thisReference = new BoundThisExpression(compilation, typeSyntax, mutableReference);

        foreach (
            var member in type.GetMembers()
                .AsValueEnumerable()
                .OfType<VariableSymbol>()
                .Where(v => v.IsInstance)
        )
        {
            var declaringSyntax = member.DeclaringSyntaxReferences[0].Syntax;

            cancellationToken.ThrowIfCancellationRequested();
            builder ??= ImmutableArray.CreateBuilder<BoundStatement>();

            var initializer = compilation.GetBoundInitializer(member, cancellationToken);
            hasErrors |= initializer.HasErrors;

            var reference = new BoundVariableAccess(
                compilation,
                declaringSyntax,
                thisReference,
                member
            );
            var initialValue =
                initializer.Initializer ?? GetDefaultValue(compilation, declaringSyntax, type);
            var assignment = new BoundAssignmentOperation(
                compilation,
                declaringSyntax,
                voidType,
                reference,
                initialValue,
                AssignmentOperation.Simple
            );
            builder.Add(new BoundExpressionStatement(compilation, declaringSyntax, assignment));
        }

        return new BoundBlock(
            compilation,
            compilation.SyntaxTrees[0].Root,
            builder?.DrainToImmutable() ?? []
        )
        {
            HasErrors = hasErrors,
        };
    }

    private static BoundExpression GetDefaultValue(
        Compilation compilation,
        SyntaxNode syntax,
        TypeSymbol type
    )
    {
        return type switch
        {
            NullableTypeSymbol => new BoundLiteral(compilation, syntax, type, new NullConstant()),
            ArrayTypeSymbol array => GetDefaultForArrayType(compilation, syntax, array),
            _ => GetDefaultForSpecialType(compilation, syntax, type),
        };
    }

    private static BoundCollectionExpression GetDefaultForArrayType(
        Compilation compilation,
        SyntaxNode syntax,
        ArrayTypeSymbol type
    )
    {
        Debug.Assert(type.Size is not null);
        var size = type.Size.Value;

        var defaultValue = GetDefaultValue(compilation, syntax, type.ElementType);
        var elements = new BoundExpression[size];
        for (var i = 0UL; i < size; i++)
        {
            elements[i] = defaultValue;
        }

        return new BoundCollectionExpression(
            compilation,
            syntax,
            type,
            type.ElementType,
            ImmutableCollectionsMarshal.AsImmutableArray(elements)
        );
    }

    private static BoundExpression GetDefaultForSpecialType(
        Compilation compilation,
        SyntaxNode syntax,
        TypeSymbol type
    )
    {
        switch (type.SpecialType)
        {
            case SpecialType.None:
            {
                var defaultCtor = type.GetMembers(CommonNames.Ctor)
                    .AsValueEnumerable()
                    .OfType<FunctionSymbol>()
                    .Single(p => p.Parameters.Length == 0);
                return new BoundNewExpression(compilation, syntax, type, defaultCtor, []);
            }
            case SpecialType.Bool:
                return new BoundLiteral(compilation, syntax, type, new BoolConstant(false));
            case SpecialType.I8:
                return new BoundLiteral(compilation, syntax, type, new I8Constant(0));
            case SpecialType.I16:
                return new BoundLiteral(compilation, syntax, type, new I16Constant(0));
            case SpecialType.I32:
                return new BoundLiteral(compilation, syntax, type, new I32Constant(0));
            case SpecialType.I64:
                return new BoundLiteral(compilation, syntax, type, new I64Constant(0));
            case SpecialType.I128:
                return new BoundLiteral(compilation, syntax, type, new I128Constant(0));
            case SpecialType.ISize:
                return new BoundLiteral(compilation, syntax, type, new ISizeConstant(0));
            case SpecialType.U8:
                return new BoundLiteral(compilation, syntax, type, new U8Constant(0));
            case SpecialType.U16:
                return new BoundLiteral(compilation, syntax, type, new U16Constant(0));
            case SpecialType.U32:
                return new BoundLiteral(compilation, syntax, type, new U32Constant(0));
            case SpecialType.U64:
                return new BoundLiteral(compilation, syntax, type, new U64Constant(0));
            case SpecialType.U128:
                return new BoundLiteral(compilation, syntax, type, new U128Constant(0));
            case SpecialType.USize:
                return new BoundLiteral(compilation, syntax, type, new USizeConstant(0));
            case SpecialType.F32:
                return new BoundLiteral(compilation, syntax, type, new F32Constant(0));
            case SpecialType.F64:
                return new BoundLiteral(compilation, syntax, type, new F64Constant(0));
            case SpecialType.Char:
                return new BoundLiteral(compilation, syntax, type, new Char8Constant(0));
            case SpecialType.Char16:
                return new BoundLiteral(compilation, syntax, type, new Char16Constant('\0'));
            case SpecialType.Rune:
                return new BoundLiteral(
                    compilation,
                    syntax,
                    type,
                    new RuneConstant(new Rune('\0'))
                );
            default:
                throw new InvalidOperationException("Not a default constructible type");
        }
    }

    internal static AssemblyDefinition CreateAssemblyDefinition(Compilation compilation)
    {
        var assembly = compilation.Assembly;
        var assemblyIdentity = new AssemblyIdentity(assembly.Name);
        var targetInfo = new TargetInfo(
            compilation.Settings.Architecture,
            compilation.Settings.OperatingSystem,
            compilation.Settings.Toolchain
        );

        var builder = new AssemblyDefinitionBuilder(assemblyIdentity, targetInfo);

        return builder.Build();
    }
}
