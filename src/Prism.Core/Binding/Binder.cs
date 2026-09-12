using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Mappers;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Symbols.Error;
using Prism.Core.Symbols.Source;
using Prism.Core.Syntax;
using Prism.Core.Utils;
using ZLinq;
using static Prism.Core.Binding.BindingHelpers;

namespace Prism.Core.Binding;

internal abstract class Binder
{
    protected Binder(Compilation compilation)
    {
        Compilation = compilation;
    }

    protected Binder(Binder next, ConversionClassifier? conversionClassifier = null)
    {
        Compilation = next.Compilation;
        Next = next;
        ConversionClassifier = conversionClassifier;
    }

    public Compilation Compilation { get; }

    protected Binder? Next { get; }

    public Binder GetNextChecked()
    {
        return Next ?? throw new InvalidOperationException("No next binder");
    }

    private ConversionClassifier ConversionClassifier
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, new ConversionClassifier(this), null);
            return field;
        }
    }

    public virtual Binder? GetBinder(SyntaxNode node)
    {
        Debug.Assert(Next is not null);
        return Next.GetBinder(node);
    }

    public Binder GetRequiredBinder(SyntaxNode node)
    {
        return GetBinder(node) ?? throw new InvalidOperationException("No binder");
    }

    protected virtual Symbol? ContainingSymbol
    {
        get
        {
            Debug.Assert(Next is not null);
            return Next.ContainingSymbol;
        }
    }

    protected virtual SyntaxNode? ScopeDesignator => null;

    public TypeSymbol ResolveType(TypeSyntax syntax, BindingContext context)
    {
        return syntax switch
        {
            NamedTypeSyntax namedTypeSyntax => RequireType(
                LookupFromSyntax(namedTypeSyntax.Identifier, LookupOptions.Type, context),
                namedTypeSyntax.Identifier,
                context
            ),
            PredefinedTypeSyntax predefinedTypeSyntax => Compilation.GetSpecialType(
                predefinedTypeSyntax.Keyword.Kind.ToSpecialType()
            ),
            ArrayTypeSyntax arrayTypeSyntax => Compilation.CreateArrayTypeSymbol(
                ResolveType(arrayTypeSyntax.ElementType, context),
                GetArraySize(arrayTypeSyntax, context)
            ),
            ReferenceTypeSyntax referenceTypeSyntax => Compilation.CreateReferenceTypeSymbol(
                ResolveType(referenceTypeSyntax.ReferencedType, context),
                referenceTypeSyntax.MutableKeyword is not null
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(syntax)),
        };
    }

    private ulong? GetArraySize(ArrayTypeSyntax syntax, BindingContext context)
    {
        if (syntax.Size is null)
            return null;

        var boundSize = BindExpression(
            syntax.Size,
            Compilation.GetSpecialType(SpecialType.USize),
            context,
            CancellationToken.None
        );
        if (
            boundSize.ConstantValue
            is not { IsUnsignedInteger: true, Kind: not ConstantKind.U128 } constant
        )
            return null;

        return constant.AsUInt64();
    }

    private TypeSymbol RequireType(LookupResult result, NameSyntax syntax, BindingContext context)
    {
        if (result is { IsViable: true, Symbol: TypeSymbol typeSymbol })
        {
            return typeSymbol;
        }

        DiagnoseLookupFailure(result, syntax, LookupOptions.Type, context);
        var names = CollectNames(syntax);
        return CreateErrorTypeSymbol(
            ContainingSymbol,
            Compilation,
            CollectionsMarshal.AsSpan(names)
        );
    }

    private LookupResult LookupFromSyntax(
        NameSyntax syntax,
        LookupOptions options,
        BindingContext context
    )
    {
        return syntax switch
        {
            SimpleNameSyntax simple => LookupFromSimpleName(simple, options, context),
            QualifiedNameSyntax qualified => LookupFromQualifiedName(qualified, options, context),
            _ => throw new ArgumentOutOfRangeException(nameof(syntax)),
        };
    }

    private LookupResult LookupUnqualifiedName(
        string name,
        LookupOptions options,
        BindingContext context
    )
    {
        if (options.HasFlag(LookupOptions.Callable))
        {
            var symbols = ImmutableArray.CreateBuilder<Symbol>();
            for (var binder = this; binder is not null; binder = binder.Next)
            {
                if (binder.LookupLocal(name, options, context) is not { Found: true } localResult)
                    continue;

                if (!localResult.IsViable || localResult.Symbols.All(s => s is FunctionSymbol))
                    return localResult;

                symbols.AddRange(localResult.Symbols);
            }

            return MakeLookupResult(symbols.DrainToImmutable(), options);
        }

        for (var binder = this; binder is not null; binder = binder.Next)
        {
            if (binder.LookupLocal(name, options, context) is { Found: true } localResult)
                return localResult;
        }

        return LookupResult.NotFound();
    }

    protected LookupResult LookupQualifiedName(
        string name,
        MemberContainerSymbol container,
        LookupOptions options
    )
    {
        var symbols = ImmutableArray.CreateBuilder<Symbol>();
        foreach (var symbol in container.GetMembers(name).Where(VisibleFrom))
        {
            switch (symbol)
            {
                case VariableSymbol variable:
                    if (options.HasFlag(LookupOptions.Value))
                    {
                        symbols.Add(variable);
                    }

                    break;
                case FunctionSymbol function:
                    if (options.HasFlag(LookupOptions.Callable))
                    {
                        symbols.Add(function);
                    }

                    break;
                case NamespaceSymbol namespaceSymbol:
                    if (options.HasFlag(LookupOptions.Namespace))
                    {
                        symbols.Add(namespaceSymbol);
                    }

                    break;
                case TypeSymbol type:
                    if (options.HasFlag(LookupOptions.Type))
                    {
                        symbols.Add(type);
                    }

                    break;
            }
        }

        return MakeLookupResult(symbols.DrainToImmutable(), options);
    }

    private LookupResult LookupFromSimpleName(
        SimpleNameSyntax syntax,
        LookupOptions options,
        BindingContext context
    )
    {
        return LookupUnqualifiedName(syntax.UnqualifiedName, options, context);
    }

    private LookupResult LookupFromQualifiedName(
        QualifiedNameSyntax syntax,
        LookupOptions options,
        BindingContext context
    )
    {
        var lookupResult = LookupFromSyntax(syntax.Left, LookupOptions.NamespaceOrType, context);
        if (!lookupResult.IsViable)
            return lookupResult;

        return lookupResult.Symbol is not MemberContainerSymbol memberContainer
            ? LookupResult.WrongKind(lookupResult.Symbols)
            : LookupQualifiedName(syntax.UnqualifiedName, memberContainer, options);
    }

    protected virtual void EnsureLocals()
    {
        Debug.Assert(Next is not null);
        Next.EnsureLocals();
    }

    protected abstract LookupResult LookupLocal(
        string name,
        LookupOptions options,
        BindingContext context
    );

    public abstract LabelSymbol? LookupLoopLabel(string name, BindingContext context);

    protected static LookupResult MakeLookupResult(
        ImmutableArray<Symbol> symbols,
        LookupOptions options
    )
    {
        if (symbols.IsEmpty)
            return LookupResult.NotFound();

        if (options.HasFlag(LookupOptions.Callable) || symbols.Length == 1)
            return LookupResult.Viable(symbols);

        return LookupResult.Ambiguous(symbols);
    }

    protected ImmutableArray<NamespaceSymbol> BuildUsingNamespaces(
        SyntaxList<UsingDirectiveSyntax> usings
    )
    {
        var result = ImmutableArray.CreateBuilder<NamespaceSymbol>(usings.Count);
        var seen = new HashSet<NamespaceSymbol>(ReferenceEqualityComparer.Instance);
        seen.EnsureCapacity(result.Capacity);

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var directive in usings)
        {
            var resolved = ResolveUsingNamespace(directive.Name);
            if (!seen.Add(resolved))
                continue;

            result.Add(resolved);
        }

        return result.DrainToImmutable();
    }

    private NamespaceSymbol ResolveUsingNamespace(NameSyntax syntax)
    {
        using var diagnostics = DiagnosticBag.Create();
        var names = CollectNames(syntax);
        var current = Compilation.GlobalNamespace;
        var isError = false;
        foreach (var simple in names)
        {
            var name = simple.UnqualifiedName;
            var result = LookupQualifiedName(name, current, LookupOptions.Namespace);
            if (result.IsViable)
            {
                current = (NamespaceSymbol)result.Symbol;
            }
            else
            {
                current = Compilation.CreateErrorNamespaceSymbol(current, name);
                if (isError)
                    continue;
                diagnostics.Add(Diagnostic.UnresolvedSymbol(simple.Location, name));
                isError = true;
            }
        }

        if (isError)
        {
            Compilation.DeclarationDiagnostics.AddRange(diagnostics);
        }

        return current;
    }

    private bool VisibleFrom(Symbol symbol)
    {
        switch (symbol.DeclaredVisibility)
        {
            case DeclaredVisibility.NotApplicable:
            case DeclaredVisibility.Public:
                return true;
            case DeclaredVisibility.Internal:
                return symbol.ContainingAssembly == Compilation.Assembly;
            case DeclaredVisibility.File:
            {
                var designator = ScopeDesignator;
                return designator is not null
                    && symbol.DeclaringSyntaxReferences.All(r =>
                        ReferenceEquals(r.SyntaxTree, designator.SyntaxTree)
                    );
            }
            default:
                throw new ArgumentException("Unexpected declared visibility", nameof(symbol));
        }
    }

    public BoundExpression BindInitializer(
        VariableSymbol variable,
        InitializerSyntax initializer,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        Debug.Assert(variable.IsGlobal);
        var expression = BindExpression(
            initializer.Value,
            variable.Type,
            context,
            cancellationToken
        );
        return AddConversionIfNecessary(expression, variable.Type, context);
    }

    public BoundStatement BindStatement(
        StatementSyntax syntax,
        TypeSymbol returnType,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        return syntax switch
        {
            BlockSyntax blockSyntax => BindBlock(
                blockSyntax,
                returnType,
                context,
                cancellationToken
            ),
            ExpressionStatementSyntax expressionStatementSyntax => BindExpressionStatement(
                expressionStatementSyntax,
                context,
                cancellationToken
            ),
            ReturnStatementSyntax returnStatementSyntax => BindReturnStatement(
                returnStatementSyntax,
                returnType,
                context,
                cancellationToken
            ),
            VariableDeclarationStatementSyntax variableDeclarationStatementSyntax =>
                BindVariableDeclaration(
                    variableDeclarationStatementSyntax,
                    context,
                    cancellationToken
                ),
            IfStatementSyntax ifStatementSyntax => BindIfStatement(
                ifStatementSyntax,
                returnType,
                context,
                cancellationToken
            ),
            WhileStatementSyntax whileStatementSyntax => BindWhileStatement(
                whileStatementSyntax,
                returnType,
                context,
                cancellationToken
            ),
            LoopStatementSyntax loopStatementSyntax => BindLoopStatement(
                loopStatementSyntax,
                returnType,
                context,
                cancellationToken
            ),
            ForStatementSyntax forStatementSyntax => BindForStatement(
                forStatementSyntax,
                returnType,
                context,
                cancellationToken
            ),
            BreakStatementSyntax breakStatementSyntax => BindBreakStatement(
                breakStatementSyntax,
                context
            ),
            ContinueStatementSyntax continueStatementSyntax => BindContinueStatement(
                continueStatementSyntax,
                context
            ),
            LabeledStatementSyntax labeledStatementSyntax => BindLabelStatement(
                labeledStatementSyntax,
                returnType,
                context,
                cancellationToken
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(syntax)),
        };
    }

    public BoundStatement BindExpressionBody(
        ExpressionBodySyntax syntax,
        TypeSymbol returnType,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        if (returnType.IsVoid)
        {
            return new BoundExpressionStatement(
                syntax,
                BindExpression(syntax.Expression, context, cancellationToken)
            );
        }

        return new BoundReturnStatement(
            syntax,
            BindExpression(syntax.Expression, returnType, context, cancellationToken)
        );
    }

    private BoundBlock BindBlock(
        BlockSyntax syntax,
        TypeSymbol returnType,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var builder = ImmutableArray.CreateBuilder<BoundStatement>(syntax.Statements.Count);
        var binder = GetRequiredBinder(syntax);
        foreach (
            var statement in syntax
                .Statements.AsValueEnumerable()
                .Where(statement => statement is not EmptyStatementSyntax)
        )
        {
            builder.Add(binder.BindStatement(statement, returnType, context, cancellationToken));
        }

        return new BoundBlock(syntax, builder.DrainToImmutable());
    }

    private BoundVariableDeclaration BindVariableDeclaration(
        VariableDeclarationStatementSyntax syntax,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var semanticModel = Compilation.GetSemanticModel(syntax.SyntaxTree);
        var declaration = syntax.Declaration;
        EnsureLocals();
        var variable =
            semanticModel.GetDeclaredSymbol(declaration)
            ?? throw new InvalidOperationException("Declared variable not found");

        var targetType = declaration.Type switch
        {
            { Type: var typeSyntax } => ResolveType(typeSyntax, context),
            null => null,
        };

        var initializer = declaration.Initializer switch
        {
            not null => BindExpression(
                declaration.Initializer.Value,
                targetType,
                context,
                cancellationToken
            ),
            null => null,
        };

        if (initializer is null)
            return new BoundVariableDeclaration(syntax, variable, initializer);
        if (targetType is not null)
        {
            initializer = AddConversionIfNecessary(initializer, targetType, context);
        }

        // We can short-circuit a double type computation here
        if (variable is SourceLocalVariableSymbol sourceSymbol)
        {
            sourceSymbol.ForceSetType(initializer.Type);
        }

        return new BoundVariableDeclaration(syntax, variable, initializer);
    }

    private BoundExpressionStatement BindExpressionStatement(
        ExpressionStatementSyntax syntax,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        return new BoundExpressionStatement(
            syntax,
            BindExpression(syntax.Expression, context, cancellationToken)
        );
    }

    private BoundStatement BindReturnStatement(
        ReturnStatementSyntax syntax,
        TypeSymbol returnType,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var expression = syntax.Expression switch
        {
            not null => BindExpression(syntax.Expression, returnType, context, cancellationToken),
            null => null,
        };

        if (expression is not null)
        {
            if (returnType.IsVoid)
            {
                context.ReportDiagnostic(Diagnostic.CannotReturnValue(expression.Syntax.Location));
            }
            else
            {
                expression = AddConversionIfNecessary(expression, returnType, context);
            }
        }
        else
        {
            if (!returnType.IsVoid)
            {
                context.ReportDiagnostic(Diagnostic.StatementMustReturnValue(syntax.Location));
            }
        }

        return new BoundReturnStatement(syntax, expression);
    }

    private BoundIfStatement BindIfStatement(
        IfStatementSyntax syntax,
        TypeSymbol returnType,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var condition = BindExpression(
            syntax.Condition,
            Compilation.GetSpecialType(SpecialType.Bool),
            context,
            cancellationToken
        );
        var thenStatement = BindBlock(syntax.Block, returnType, context, cancellationToken);
        var elseStatement = syntax.ElseClause switch
        {
            not null => BindStatement(
                syntax.ElseClause.Statement,
                returnType,
                context,
                cancellationToken
            ),
            null => null,
        };
        return new BoundIfStatement(syntax, condition, thenStatement, elseStatement);
    }

    private BoundWhileStatement BindWhileStatement(
        WhileStatementSyntax syntax,
        TypeSymbol returnType,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var binder = GetRequiredBinder(syntax);
        var condition = binder.BindExpression(
            syntax.Condition,
            Compilation.GetSpecialType(SpecialType.Bool),
            context,
            cancellationToken
        );
        var loopBody = binder.BindBlock(syntax.Block, returnType, context, cancellationToken);
        var label = binder.LookupLoopLabel("", context);
        Debug.Assert(label is not null);
        return new BoundWhileStatement(syntax, condition, loopBody, label);
    }

    private BoundLoopStatement BindLoopStatement(
        LoopStatementSyntax syntax,
        TypeSymbol returnType,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var binder = GetRequiredBinder(syntax);
        var loopBody = binder.BindBlock(syntax.Block, returnType, context, cancellationToken);
        var label = binder.LookupLoopLabel("", context);
        Debug.Assert(label is not null);
        return new BoundLoopStatement(syntax, loopBody, label);
    }

    private BoundForStatement BindForStatement(
        ForStatementSyntax syntax,
        TypeSymbol returnType,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var binder = GetRequiredBinder(syntax);
        var variable = syntax.Declaration switch
        {
            not null => binder.BindVariableDeclaration(
                syntax.Declaration,
                context,
                cancellationToken
            ),
            null => null,
        };
        var initializers = syntax
            .Initializers.AsValueEnumerable()
            .Select(e => binder.BindExpression(e, context, cancellationToken))
            .ToImmutableArray();
        var condition = syntax.Condition switch
        {
            not null => binder.BindExpression(
                syntax.Condition,
                Compilation.GetSpecialType(SpecialType.Bool),
                context,
                cancellationToken
            ),
            null => null,
        };

        var incrementors = syntax
            .Incrementors.AsValueEnumerable()
            .Select(e => binder.BindExpression(e, context, cancellationToken))
            .ToImmutableArray();

        var loopBody = binder.BindBlock(syntax.Block, returnType, context, cancellationToken);
        var label = binder.LookupLoopLabel("", context);
        Debug.Assert(label is not null);
        return new BoundForStatement(
            syntax,
            variable,
            initializers,
            condition,
            incrementors,
            loopBody,
            label
        );
    }

    private BoundBreakStatement BindBreakStatement(
        BreakStatementSyntax syntax,
        BindingContext context
    )
    {
        var labelName = syntax.Label?.IdentifierName ?? "";
        var label = LookupLoopLabel(labelName, context);
        if (label is not null)
            return new BoundBreakStatement(syntax, label);

        context.ReportDiagnostic(Diagnostic.InvalidUseOfBreak(syntax.Location));
        var enclosingFunction = ContainingSymbol as FunctionSymbol ?? ErrorFunctionSymbol.Unnamed;
        label = new ErrorLabelSymbol(labelName, enclosingFunction);

        return new BoundBreakStatement(syntax, label);
    }

    private BoundContinueStatement BindContinueStatement(
        ContinueStatementSyntax syntax,
        BindingContext context
    )
    {
        var labelName = syntax.Label?.IdentifierName ?? "";
        var label = LookupLoopLabel(labelName, context);
        if (label is not null)
            return new BoundContinueStatement(syntax, label);

        context.ReportDiagnostic(Diagnostic.InvalidUseOfBreak(syntax.Location));
        var enclosingFunction = ContainingSymbol as FunctionSymbol ?? ErrorFunctionSymbol.Unnamed;
        label = new ErrorLabelSymbol(labelName, enclosingFunction);

        return new BoundContinueStatement(syntax, label);
    }

    private BoundStatement BindLabelStatement(
        LabeledStatementSyntax syntax,
        TypeSymbol returnValue,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        if (
            syntax.Statement
            is not WhileStatementSyntax
                and not LoopStatementSyntax
                and not ForStatementSyntax
        )
        {
            context.ReportDiagnostic(Diagnostic.InvalidUseOfLabel(syntax.Identifier.Location));
        }

        return BindStatement(syntax.Statement, returnValue, context, cancellationToken);
    }

    public BoundExpression BindExpression(
        ExpressionSyntax syntax,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        return BindExpression(syntax, null, context, cancellationToken);
    }

    private BoundExpression BindExpression(
        ExpressionSyntax syntax,
        bool isSpeculative,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        return BindExpression(syntax, null, isSpeculative, context, cancellationToken);
    }

    private BoundExpression BindExpression(
        ExpressionSyntax syntax,
        TypeSymbol? targetType,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        return BindExpression(syntax, targetType, false, context, cancellationToken);
    }

    private BoundExpression BindExpression(
        ExpressionSyntax syntax,
        TypeSymbol? targetType,
        bool isSpeculative,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        return syntax switch
        {
            LiteralExpressionSyntax literal => BindLiteralExpression(
                literal,
                targetType,
                context,
                isSpeculative
            ),
            IdentifierExpressionSyntax identifier => BindIdentifierExpression(identifier, context),
            ParenthesizedExpressionSyntax parenthesized => BindExpression(
                parenthesized.Expression,
                targetType,
                context,
                cancellationToken
            ),
            BinaryExpressionSyntax binary => BindBinaryExpression(
                binary,
                context,
                cancellationToken
            ),
            AssignmentExpressionSyntax assignment => BindAssignmentExpression(
                assignment,
                context,
                cancellationToken
            ),
            PrefixExpressionSyntax prefix => BindPrefixExpression(
                prefix,
                targetType,
                isSpeculative,
                context,
                cancellationToken
            ),
            PostfixExpressionSyntax postfix => BindPostfixExpression(
                postfix,
                targetType,
                context,
                cancellationToken
            ),
            TernaryExpressionSyntax ternary => BindTernaryExpression(
                ternary,
                targetType,
                context,
                isSpeculative,
                cancellationToken
            ),
            InvocationExpressionSyntax invocation => BindInvocationExpression(
                invocation,
                context,
                cancellationToken
            ),
            CastExpressionSyntax cast => BindCastExpression(cast, context, cancellationToken),
            IndexExpressionSyntax index => BindIndexExpression(index, context, cancellationToken),
            _ => throw new ArgumentException("Invalid expression syntax", nameof(syntax)),
        };
    }

    private BoundExpression BindLiteralExpression(
        LiteralExpressionSyntax syntax,
        TypeSymbol? returnType,
        BindingContext context,
        bool isSpeculative
    )
    {
        var token = syntax.Value;
        if (isSpeculative)
        {
            Debug.Assert(returnType is null);
            if (
                token.TryGetValue<IntegerLiteralData>() is
                { Suffix: IntegerSuffix.None } integerValue
            )
            {
                var constant = EvaluateNumericExpression(
                    in integerValue,
                    null,
                    syntax.Location,
                    BindingContext.Discarded
                );
                return new BoundUnfixedIntegerLiteral(
                    syntax,
                    integerValue,
                    Compilation.GetSpecialType(constant.SpecialType)
                );
            }

            if (token.TryGetValue<FloatLiteralData>() is { Suffix: FloatSuffix.None } floatingValue)
            {
                var constant = EvaluateNumericExpression(
                    in floatingValue,
                    null,
                    syntax.Location,
                    BindingContext.Discarded
                );
                return new BoundUnfixedFloatLiteral(
                    syntax,
                    floatingValue,
                    Compilation.GetSpecialType(constant.SpecialType)
                );
            }
        }

        var value = EvaluateConstantExpression(token, returnType, context);
        var type = Compilation.GetSpecialType(value.SpecialType);
        return new BoundLiteral(syntax, type, value);
    }

    private BoundExpression BindIdentifierExpression(
        IdentifierExpressionSyntax syntax,
        BindingContext context
    )
    {
        var result = LookupFromSyntax(syntax.Value, LookupOptions.Value, context);
        if (!result.IsViable)
            return new BoundBadExpression(syntax, ErrorTypeSymbol.Unnamed);

        return result.Symbol switch
        {
            VariableSymbol v => new BoundVariableAccess(syntax, v),
            ParameterSymbol p => new BoundParameterAccess(syntax, p),
            _ => throw new InvalidOperationException(
                "We must have added a symbol type that can hold a value that we haven't accounted for yet."
            ),
        };
    }

    private BoundBinaryOperation BindBinaryExpression(
        BinaryExpressionSyntax syntax,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var left = BindExpression(syntax.Left, context, cancellationToken);
        var right = BindExpression(syntax.Right, context, cancellationToken);
        var operation = syntax.Op.Kind.ToBinaryOperation();
        var conversion = ConversionClassifier.ClassifyBinaryOperand(
            operation,
            left.Type,
            right.Type
        );
        if (conversion is var (leftConversion, rightConversion, commonType))
        {
            left = AddConversionIfNecessary(left, commonType, leftConversion, context);
            right = AddConversionIfNecessary(right, commonType, rightConversion, context);
        }
        else
        {
            context.ReportDiagnostic(
                Diagnostic.BinaryOperatorUndefined(
                    syntax.Location,
                    left.Type.ToDisplayString(),
                    right.Type.ToDisplayString()
                )
            );
        }

        var targetType = operation switch
        {
            BinaryOperation.Addition
            or BinaryOperation.Subtraction
            or BinaryOperation.Multiplication
            or BinaryOperation.Division
            or BinaryOperation.Modulo
            or BinaryOperation.BitwiseAnd
            or BinaryOperation.BitwiseOr
            or BinaryOperation.BitwiseXor
            or BinaryOperation.ShiftLeft
            or BinaryOperation.ShiftRight
            or BinaryOperation.UnsignedShiftRight => conversion?.Type,
            BinaryOperation.LogicalAnd
            or BinaryOperation.LogicalOr
            or BinaryOperation.Equality
            or BinaryOperation.NotEquals
            or BinaryOperation.LessThan
            or BinaryOperation.LessThanOrEquals
            or BinaryOperation.GreaterThan
            or BinaryOperation.GreaterThanOrEquals => Compilation.GetSpecialType(SpecialType.Bool),
            BinaryOperation.ThreeWayComparison => throw new NotImplementedException(),
            _ => throw new InvalidOperationException("Unknown binary operation"),
        };

        return new BoundBinaryOperation(
            syntax,
            targetType ?? ErrorTypeSymbol.Unnamed,
            left,
            right,
            operation
        );
    }

    private BoundAssignmentOperation BindAssignmentExpression(
        AssignmentExpressionSyntax syntax,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var assignee = BindExpression(syntax.Left, context, cancellationToken);
        var operation = syntax.Op.Kind.ToAssignmentOperation();
        if (!IsAssignmentValid(assignee.Type, operation))
        {
            context.ReportDiagnostic(
                Diagnostic.NoCompoundAssignmentOperator(
                    syntax.Location,
                    assignee.Type.ToDisplayString()
                )
            );
        }
        else if (!assignee.IsAssignable)
        {
            context.ReportDiagnostic(Diagnostic.CannotAssignExpression(syntax.Location));
        }

        var assigned = BindExpression(syntax.Right, context, cancellationToken);
        return new BoundAssignmentOperation(
            syntax,
            Compilation.GetSpecialType(SpecialType.Void),
            assignee,
            assigned,
            operation
        );
    }

    private BoundExpression BindPrefixExpression(
        PrefixExpressionSyntax syntax,
        TypeSymbol? returnType,
        bool isSpeculative,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (syntax.Op.Kind)
        {
            case SyntaxKind.AmpToken:
            {
                var inner = BindExpression(syntax.Operand, context, cancellationToken);
                if (!inner.IsAddressable)
                {
                    context.ReportDiagnostic(Diagnostic.CannotTakeAddress(syntax.Operand.Location));
                }

                return new BoundAddressOf(
                    syntax,
                    inner,
                    Compilation.CreateReferenceTypeSymbol(inner.Type, inner.IsAssignable)
                );
            }
            case SyntaxKind.StarToken:
            {
                var inner = BindExpression(syntax.Operand, context, cancellationToken);
                if (
                    inner.Type is ReferenceTypeSymbol
                    {
                        ReferencedType: var referencedType,
                        IsMutable: var isMutable,
                    }
                )
                {
                    if (referencedType.IsDynamicallySized)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.CannotDereferenceVariableLength(
                                syntax.Operand.Location,
                                referencedType.ToDisplayString()
                            )
                        );
                    }

                    return new BoundDereference(syntax, inner, referencedType, isMutable);
                }

                context.ReportDiagnostic(Diagnostic.CannotDereference(syntax.Operand.Location));
                return new BoundDereference(syntax, inner, inner.Type, false);
            }
        }

        var op = syntax.Op.Kind.ToPrefixOperation();

        if (op == UnaryOperation.Negation)
        {
            if (syntax.Operand is LiteralExpressionSyntax literal)
            {
                if (
                    literal.Value.TryGetValue<IntegerLiteralData>() is
                    { Suffix.CanBeSigned: true } intData
                )
                {
                    var negated = EvaluateNumericExpression(
                        in intData,
                        returnType,
                        syntax.Location,
                        context,
                        true
                    );

                    if (intData.Suffix == IntegerSuffix.None && isSpeculative)
                    {
                        return new BoundUnfixedIntegerLiteral(
                            syntax,
                            intData,
                            Compilation.GetSpecialType(negated.SpecialType)
                        )
                        {
                            Negated = true,
                        };
                    }

                    return new BoundLiteral(
                        syntax,
                        Compilation.GetSpecialType(negated.SpecialType),
                        negated
                    );
                }

                if (literal.Value.TryGetValue<FloatLiteralData>() is { } floatData)
                {
                    var negated = EvaluateNumericExpression(
                        in floatData,
                        returnType,
                        syntax.Location,
                        context,
                        true
                    );

                    if (floatData.Suffix == FloatSuffix.None && isSpeculative)
                    {
                        return new BoundUnfixedFloatLiteral(
                            syntax,
                            floatData,
                            Compilation.GetSpecialType(negated.SpecialType)
                        )
                        {
                            Negated = true,
                        };
                    }

                    return new BoundLiteral(
                        syntax,
                        Compilation.GetSpecialType(negated.SpecialType),
                        negated
                    );
                }
            }
        }

        var operand = BindExpression(syntax.Operand, returnType, context, cancellationToken);
        return CreateUnaryOperation(syntax, op, operand, context);
    }

    private BoundUnaryOperation BindPostfixExpression(
        PostfixExpressionSyntax syntax,
        TypeSymbol? returnType,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var operand = BindExpression(syntax.Operand, returnType, context, cancellationToken);
        var op = syntax.Op.Kind.ToPostfixOperation();
        return CreateUnaryOperation(syntax, op, operand, context);
    }

    private BoundExpression BindTernaryExpression(
        TernaryExpressionSyntax syntax,
        TypeSymbol? returnType,
        BindingContext context,
        bool isSpeculative,
        CancellationToken cancellationToken
    )
    {
        var condition = AddConversionIfNecessary(
            BindExpression(syntax.Condition, context, cancellationToken),
            Compilation.GetSpecialType(SpecialType.Bool),
            context
        );

        if (isSpeculative)
        {
            return BindSpeculativeConditional(syntax, condition, context, cancellationToken);
        }

        return BindRegularConditional(syntax, returnType, condition, context, cancellationToken);
    }

    private BoundSpeculativeConditional BindSpeculativeConditional(
        TernaryExpressionSyntax syntax,
        BoundExpression condition,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var whenTrue = BindExpression(syntax.WhenTrue, true, context, cancellationToken);
        var whenFalse = BindExpression(syntax.WhenFalse, true, context, cancellationToken);

        var returnType = whenTrue.Type == whenFalse.Type ? whenTrue.Type : ErrorTypeSymbol.Unnamed;
        return new BoundSpeculativeConditional(syntax, returnType, condition, whenTrue, whenFalse);
    }

    private BoundConditional BindRegularConditional(
        TernaryExpressionSyntax syntax,
        TypeSymbol? returnType,
        BoundExpression condition,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var whenTrue = BindExpression(syntax.WhenTrue, context, cancellationToken);
        var whenFalse = BindExpression(syntax.WhenFalse, context, cancellationToken);
        return ConstructConditional(syntax, returnType, condition, whenTrue, whenFalse, context);
    }

    private BoundConditional ConstructConditional(
        SyntaxNode syntax,
        TypeSymbol? returnType,
        BoundExpression condition,
        BoundExpression whenTrue,
        BoundExpression whenFalse,
        BindingContext context
    )
    {
        if (returnType is not null)
        {
            whenTrue = AddConversionIfNecessary(whenTrue, returnType, context);
            whenFalse = AddConversionIfNecessary(whenFalse, returnType, context);
        }
        else
        {
            whenFalse = AddConversionIfNecessary(whenFalse, whenTrue.Type, context);
            returnType = whenTrue.Type;
        }

        return new BoundConditional(syntax, returnType, condition, whenTrue, whenFalse);
    }

    private BoundInvocation BindInvocationExpression(
        InvocationExpressionSyntax syntax,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        if (syntax.Callee is IdentifierExpressionSyntax nameSyntax)
        {
            var overloads = LookupFromSyntax(nameSyntax.Value, LookupOptions.Callable, context);
            if (overloads.IsViable)
            {
                return overloads.Symbols.Length == 1
                    ? BindUnambiguousOverloadSet(syntax, overloads, context, cancellationToken)
                    : BindAmbiguousOverloadSet(syntax, overloads, context, cancellationToken);
            }
        }

        var callee = BindExpression(syntax.Callee, context, cancellationToken);

        context.ReportDiagnostic(
            Diagnostic.NoCallOperatorDefined(syntax.Callee.Location, callee.Type.ToDisplayString())
        );

        var unknownArguments = new BoundExpression[syntax.Arguments.Arguments.Count + 1];
        unknownArguments[0] = callee;
        foreach (var (i, argumentSyntax) in syntax.Arguments.Arguments.AsValueEnumerable().Index())
        {
            unknownArguments[i + 1] = BindExpression(
                argumentSyntax.Value,
                context,
                cancellationToken
            );
        }

        return new BoundInvocation(
            syntax,
            ErrorFunctionSymbol.Unnamed,
            ImmutableCollectionsMarshal.AsImmutableArray(unknownArguments)
        );
    }

    private BoundInvocation BindUnambiguousOverloadSet(
        InvocationExpressionSyntax syntax,
        LookupResult overloads,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var function = (FunctionSymbol)overloads.Symbols[0];
        var parameters = function.Parameters;
        var arguments = new BoundExpression[syntax.Arguments.Arguments.Count];
        foreach (var (i, argumentSyntax) in syntax.Arguments.Arguments.AsValueEnumerable().Index())
        {
            arguments[i] = BindExpression(
                argumentSyntax.Value,
                parameters[i].Type,
                context,
                cancellationToken
            );
        }

        return new BoundInvocation(
            syntax,
            function,
            ImmutableCollectionsMarshal.AsImmutableArray(arguments)
        );
    }

    private BoundInvocation BindAmbiguousOverloadSet(
        InvocationExpressionSyntax syntax,
        LookupResult overloads,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var arguments = GetOverloadArguments(syntax, context, cancellationToken);

        var (overload, realArgs) = ResolveOverload(
            overloads,
            arguments,
            syntax.Callee.Location,
            context
        );
        return new BoundInvocation(syntax, overload, realArgs);
    }

    private ImmutableArray<BoundExpression> GetOverloadArguments(
        InvocationExpressionSyntax syntax,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var arguments = new BoundExpression[syntax.Arguments.Arguments.Count];
        foreach (var (i, argumentSyntax) in syntax.Arguments.Arguments.AsValueEnumerable().Index())
        {
            arguments[i] = BindExpression(argumentSyntax.Value, true, context, cancellationToken);
        }

        return ImmutableCollectionsMarshal.AsImmutableArray(arguments);
    }

    private BoundExpression BindCastExpression(
        CastExpressionSyntax syntax,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var operand = BindExpression(syntax.Operand, context, cancellationToken);
        var targetType = ResolveType(syntax.Type, context);
        return AddConversionIfNecessary(operand, targetType, context, isExplicit: true);
    }

    private BoundIndex BindIndexExpression(
        IndexExpressionSyntax syntax,
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var operand = AutoDereferenceIfNecessary(
            BindExpression(syntax.Operand, context, cancellationToken)
        );
        var index = BindExpression(syntax.Index, context, cancellationToken);

        TypeSymbol targetType;
        switch (operand.Type)
        {
            case { SpecialType: SpecialType.Str }:
                targetType = Compilation.GetSpecialType(SpecialType.Char);
                index = AddConversionIfNecessary(
                    index,
                    Compilation.GetSpecialType(SpecialType.USize),
                    context
                );
                break;
            case ArrayTypeSymbol { ElementType: var elementType }:
                targetType = elementType;
                index = AddConversionIfNecessary(
                    index,
                    Compilation.GetSpecialType(SpecialType.USize),
                    context
                );
                break;
            default:
                targetType = ErrorTypeSymbol.Unnamed;
                break;
        }

        return new BoundIndex(syntax, operand, index, targetType);
    }

    private BoundExpression AddConversionIfNecessary(
        BoundExpression expression,
        TypeSymbol type,
        BindingContext context,
        bool isExplicit = false
    )
    {
        var conversion = ConversionClassifier.ClassifyConversion(expression.Type, type);
        return AddConversionIfNecessary(expression, type, conversion, context, isExplicit);
    }

    private static BoundExpression AddConversionIfNecessary(
        BoundExpression expression,
        TypeSymbol type,
        Conversion conversion,
        BindingContext context,
        bool isExplicit = false
    )
    {
        var syntax = (ExpressionSyntax)expression.Syntax;
        if (!conversion.Exists)
        {
            context.ReportDiagnostic(
                Diagnostic.NoConversion(
                    syntax.Location,
                    expression.Type.ToDisplayString(),
                    type.ToDisplayString()
                )
            );
        }
        else if (!conversion.IsIdentity)
        {
            if (!conversion.IsImplicit && !isExplicit)
            {
                context.ReportDiagnostic(
                    Diagnostic.ConversionIsExplicit(
                        syntax.Location,
                        expression.Type.ToDisplayString(),
                        type.ToDisplayString()
                    )
                );
            }

            return new BoundConversion(syntax, type, expression, conversion);
        }

        return expression;
    }

    private static BoundExpression AutoDereferenceIfNecessary(BoundExpression expression)
    {
        while (true)
        {
            if (
                expression.Type is ReferenceTypeSymbol
                {
                    ReferencedType: var referenced,
                    IsMutable: var isMutable
                }
            )
            {
                expression = new BoundDereference(
                    expression.Syntax,
                    expression,
                    referenced,
                    isMutable
                );
            }
            else
            {
                return expression;
            }
        }
    }

    private ConstantValue EvaluateConstantExpression(
        SyntaxToken token,
        TypeSymbol? returnType,
        BindingContext context
    )
    {
        if (token.TryGetValue<BoolLiteralData>() is { Value: var boolValue })
        {
            return ConstantValue.Boolean(boolValue);
        }

        if (token.TryGetValue<IntegerLiteralData>() is { } integerValue)
        {
            return EvaluateNumericExpression(integerValue, returnType, token.Location, context);
        }

        if (token.TryGetValue<FloatLiteralData>() is { } floatingValue)
        {
            return EvaluateNumericExpression(floatingValue, returnType, token.Location, context);
        }

        if (
            token.TryGetValue<CharacterLiteralData>() is
            { Encoding: var encoding, Value: var value }
        )
        {
            return encoding switch
            {
                CharacterEncoding.Utf8 => ConstantValue.Character((byte)value.Value),
                CharacterEncoding.Utf16 => ConstantValue.Character16((char)value.Value),
                CharacterEncoding.Utf32 => ConstantValue.Rune(value),
                _ => throw new InvalidOperationException("Invalid character encoding"),
            };
        }

        return token.TryGetValue<StringLiteralData>() is { Value: var stringValue }
            ? ConstantValue.Str(stringValue)
            : throw new InvalidOperationException("Invalid literal");
    }

    private ConstantValue EvaluateNumericExpression(
        in IntegerLiteralData data,
        TypeSymbol? returnType,
        Location location,
        BindingContext context,
        bool isNegative = false
    )
    {
        var targetType = GetIntegerTargetKind(in data, returnType);
        if (!data.Value.FitsIn(targetType, Compilation.Settings))
        {
            context.ReportDiagnostic(Diagnostic.LiteralValueTooBig(location));
        }

        switch (targetType)
        {
            case IntegerTargetKind.I8:
                return ConstantValue.I8((sbyte)MaybeNegative(data.Value, isNegative));
            case IntegerTargetKind.I16:
                return ConstantValue.I16((short)MaybeNegative(data.Value, isNegative));
            case IntegerTargetKind.I32:
                return ConstantValue.I32((int)MaybeNegative(data.Value, isNegative));
            case IntegerTargetKind.I64:
                return ConstantValue.I64((long)MaybeNegative(data.Value, isNegative));
            case IntegerTargetKind.I128:
                return ConstantValue.I128((Int128)MaybeNegative(data.Value, isNegative));
            case IntegerTargetKind.ISize:
                return ConstantValue.ISize((long)MaybeNegative(data.Value, isNegative));
            case IntegerTargetKind.U8:
                ThrowIfNegative(isNegative);
                return ConstantValue.U8((byte)MaybeNegative(data.Value, isNegative));
            case IntegerTargetKind.U16:
                ThrowIfNegative(isNegative);
                return ConstantValue.U16((ushort)MaybeNegative(data.Value, isNegative));
            case IntegerTargetKind.U32:
                ThrowIfNegative(isNegative);
                return ConstantValue.U32((uint)MaybeNegative(data.Value, isNegative));
            case IntegerTargetKind.U64:
                ThrowIfNegative(isNegative);
                return ConstantValue.U64((ulong)MaybeNegative(data.Value, isNegative));
            case IntegerTargetKind.U128:
                ThrowIfNegative(isNegative);
                return ConstantValue.U128((UInt128)MaybeNegative(data.Value, isNegative));
            case IntegerTargetKind.USize:
                ThrowIfNegative(isNegative);
                return ConstantValue.USize((ulong)MaybeNegative(data.Value, isNegative));
            case IntegerTargetKind.F32:
                return ConstantValue.F32((float)MaybeNegative(data.Value, isNegative));
            case IntegerTargetKind.F64:
                return ConstantValue.F64((double)MaybeNegative(data.Value, isNegative));
            case IntegerTargetKind.BestFit:
            {
                var value = MaybeNegative(data.Value, isNegative);
                if (value.FitsIn<int>())
                    return ConstantValue.I32((int)value);

                if (value.FitsIn<uint>())
                    return ConstantValue.U32((uint)value);

                if (value.FitsIn<long>())
                    return ConstantValue.I64((long)value);

                if (value.FitsIn<ulong>())
                    return ConstantValue.U64((ulong)value);

                return value.FitsIn<Int128>()
                    ? ConstantValue.I128((Int128)value)
                    : ConstantValue.U128((UInt128)value);
            }
            default:
                throw new InvalidOperationException("Invalid target type");
        }
    }

    private static ConstantValue EvaluateNumericExpression(
        in FloatLiteralData data,
        TypeSymbol? returnType,
        Location location,
        BindingContext context,
        bool isNegative = false
    )
    {
        var target = data.Suffix switch
        {
            FloatSuffix.None => returnType?.SpecialType ?? SpecialType.F64,
            FloatSuffix.F32 => SpecialType.F32,
            FloatSuffix.F64 => SpecialType.F64,
            _ => throw new ArgumentOutOfRangeException(nameof(data), data.Suffix, null),
        };

        switch (target)
        {
            case SpecialType.F32:
                if (float.FitsInFiniteFloatMagnitude(data.Significand, data.Exponent10))
                {
                    context.ReportDiagnostic(Diagnostic.LiteralValueTooBig(location));
                }

                return ConstantValue.F32(
                    float.ParseDecimalFloat(data.Significand, data.Exponent10, isNegative)
                );
            case SpecialType.F64:
                if (double.FitsInFiniteFloatMagnitude(data.Significand, data.Exponent10))
                {
                    context.ReportDiagnostic(Diagnostic.LiteralValueTooBig(location));
                }

                return ConstantValue.F64(
                    double.ParseDecimalFloat(data.Significand, data.Exponent10, isNegative)
                );
            default:
                throw new InvalidOperationException("Invalid target type");
        }
    }

    private static void ThrowIfNegative(bool isNegative)
    {
        if (isNegative)
            throw new InvalidOperationException("Invalid unary operation");
    }

    private static BigInteger MaybeNegative(BigInteger value, bool isNegative)
    {
        return isNegative ? -value : value;
    }

    private BoundUnaryOperation CreateUnaryOperation(
        ExpressionSyntax syntax,
        UnaryOperation operation,
        BoundExpression operand,
        BindingContext context
    )
    {
        var resultType = ConversionClassifier.ClassifyUnaryOperand(operation, operand.Type);
        if (resultType is var (conversion, type))
        {
            operand = AddConversionIfNecessary(operand, type, conversion, context);
        }
        else
        {
            context.ReportDiagnostic(
                Diagnostic.UnaryOperatorUndefined(syntax.Location, operand.Type.ToDisplayString())
            );
        }

        if (operation.IsAssigning && !operand.IsAssignable)
        {
            context.ReportDiagnostic(Diagnostic.CannotAssignExpression(syntax.Location));
        }

        var finalType = resultType?.Type ?? ErrorTypeSymbol.Unnamed;
        return new BoundUnaryOperation(syntax, finalType, operand, operation);
    }

    private static bool IsAssignmentValid(TypeSymbol type, AssignmentOperation operation)
    {
        return operation switch
        {
            AssignmentOperation.Simple => true,
            AssignmentOperation.Addition
            or AssignmentOperation.Subtraction
            or AssignmentOperation.Multiplication
            or AssignmentOperation.Division
            or AssignmentOperation.Modulo => type.SpecialType.IsNumeric,
            AssignmentOperation.BitwiseAnd
            or AssignmentOperation.BitwiseOr
            or AssignmentOperation.BitwiseXor
            or AssignmentOperation.ShiftLeft
            or AssignmentOperation.ShiftRight
            or AssignmentOperation.UnsignedShiftRight => type.SpecialType.IsInteger,
            AssignmentOperation.LogicalAnd or AssignmentOperation.LogicalOr => type.SpecialType
                == SpecialType.Bool,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
        };
    }

    private readonly record struct ResolvedOverload(
        FunctionSymbol Function,
        ImmutableArray<BoundExpression> Arguments
    );

    private readonly record struct OverloadResolutionResult(ResolvedOverload Match, bool IsExact);

    private ResolvedOverload ResolveOverload(
        LookupResult result,
        ImmutableArray<BoundExpression> arguments,
        Location location,
        BindingContext context
    )
    {
        // TODO: We need to eventually actually resolve named/default parameters
        var matchesArgSize = false;
        var exactMatches = new List<ResolvedOverload>();
        var convertibleMatches = new List<ResolvedOverload>();

        foreach (var overload in result.Symbols.AsValueEnumerable().Cast<FunctionSymbol>())
        {
            if (overload.Parameters.Length != arguments.Length)
                continue;

            matchesArgSize = true;

            if (TryMatchOverload(overload, arguments, context) is not var (resolved, isExact))
            {
                continue;
            }

            if (isExact)
            {
                exactMatches.Add(resolved);
            }
            else
            {
                convertibleMatches.Add(resolved);
            }
        }

        switch (exactMatches.Count)
        {
            case 1:
                return exactMatches[0];
            case > 1:
            {
                var match = exactMatches[0];
                context.ReportDiagnostic(
                    Diagnostic.AmbiguousOverloadDefined(location, GetTypeNames(match.Arguments))
                );
                return match;
            }
        }

        switch (convertibleMatches.Count)
        {
            case 1:
                return convertibleMatches[0];
            case > 1:
                arguments = RealizeSpeculativeBindingForDiagnostics(arguments, context);
                context.ReportDiagnostic(
                    Diagnostic.AmbiguousOverloadDefined(location, GetTypeNames(arguments))
                );
                return convertibleMatches[0];
        }

        arguments = RealizeSpeculativeBindingForDiagnostics(arguments, context);
        context.ReportDiagnostic(
            matchesArgSize
                ? Diagnostic.NoOverloadForArgTypes(location, GetTypeNames(arguments))
                : Diagnostic.NoOverloadMatchingArgCount(location, arguments.Length)
        );

        return new ResolvedOverload((FunctionSymbol)result.Symbols[0], arguments);
    }

    private OverloadResolutionResult? TryMatchOverload(
        FunctionSymbol overload,
        ImmutableArray<BoundExpression> arguments,
        BindingContext context
    )
    {
        BoundExpression[]? remappedArgs = null;
        var isExactMatch = true;
        foreach (var (i, argument) in arguments.AsValueEnumerable().Index())
        {
            var parameter = overload.Parameters[i];

            if (argument.Type == parameter.Type)
                continue;

            BoundExpression newExpression;
            if (argument is BoundSpeculativeExpression speculative)
            {
                isExactMatch &= parameter.Type == speculative.DefaultType;
                newExpression = ApplySpeculativeBinding(speculative, parameter.Type, context);
            }
            else
            {
                isExactMatch = false;
                var conversion = ConversionClassifier.ClassifyConversion(
                    argument.Type,
                    parameter.Type
                );
                if (!conversion.IsImplicit)
                {
                    return null;
                }

                newExpression = AddConversionIfNecessary(
                    argument,
                    parameter.Type,
                    conversion,
                    context
                );
            }

            if (remappedArgs is null)
            {
                remappedArgs = new BoundExpression[arguments.Length];
                arguments.CopyTo(remappedArgs);
            }

            remappedArgs[i] = newExpression;
        }

        return remappedArgs is not null
            ? new OverloadResolutionResult(
                new ResolvedOverload(
                    overload,
                    ImmutableCollectionsMarshal.AsImmutableArray(remappedArgs)
                ),
                isExactMatch
            )
            : new OverloadResolutionResult(new ResolvedOverload(overload, arguments), isExactMatch);
    }

    private static string GetTypeNames(ImmutableArray<BoundExpression> arguments)
    {
        return string.Join(", ", arguments.Select(a => a.Type.Name));
    }

    private ImmutableArray<BoundExpression> RealizeSpeculativeBindingForDiagnostics(
        ImmutableArray<BoundExpression> arguments,
        BindingContext context
    )
    {
        BoundExpression[]? remappedArgs = null;
        foreach (var (i, argument) in arguments.AsValueEnumerable().Index())
        {
            if (argument is not BoundSpeculativeExpression speculative)
            {
                continue;
            }

            var newExpression = ApplySpeculativeBinding(speculative, null, context);

            if (remappedArgs is null)
            {
                remappedArgs = new BoundExpression[arguments.Length];
                arguments.CopyTo(remappedArgs);
            }

            remappedArgs[i] = newExpression;
        }

        return remappedArgs is not null
            ? ImmutableCollectionsMarshal.AsImmutableArray(remappedArgs)
            : arguments;
    }

    private BoundExpression ApplySpeculativeBinding(
        BoundExpression expression,
        TypeSymbol? targetType,
        BindingContext context
    )
    {
        if (expression is not BoundSpeculativeExpression speculative)
            return expression;

        return ApplySpeculativeBinding(speculative, targetType, context);
    }

    private BoundExpression ApplySpeculativeBinding(
        BoundSpeculativeExpression expression,
        TypeSymbol? targetType,
        BindingContext context
    )
    {
        switch (expression)
        {
            case BoundUnfixedIntegerLiteral { Data: var integerLiteral, Negated: var negated }:
            {
                var constant = EvaluateNumericExpression(
                    in integerLiteral,
                    targetType,
                    expression.Syntax.Location,
                    context,
                    negated
                );
                var type = Compilation.GetSpecialType(constant.SpecialType);
                return new BoundLiteral(expression.Syntax, type, constant);
            }
            case BoundUnfixedFloatLiteral { Data: var floatLiteral, Negated: var negated }:
            {
                var constant = EvaluateNumericExpression(
                    in floatLiteral,
                    targetType,
                    expression.Syntax.Location,
                    context,
                    negated
                );
                var type = Compilation.GetSpecialType(constant.SpecialType);
                return new BoundLiteral(expression.Syntax, type, constant);
            }
            case BoundSpeculativeConditional conditional:
                return ApplySpeculativeConditional(conditional, targetType, context);
            default:
                throw new InvalidOperationException("Unknown expression type");
        }
    }

    private BoundConditional ApplySpeculativeConditional(
        BoundSpeculativeConditional conditional,
        TypeSymbol? targetType,
        BindingContext context
    )
    {
        var whenTrue = ApplySpeculativeBinding(conditional.WhenTrue, targetType, context);
        var whenFalse = ApplySpeculativeBinding(conditional.WhenFalse, targetType, context);
        return ConstructConditional(
            conditional.Syntax,
            targetType,
            conditional.Condition,
            whenTrue,
            whenFalse,
            context
        );
    }
}
