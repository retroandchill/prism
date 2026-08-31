using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using NetEscapades.EnumGenerators;
using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Mappers;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Symbols.Error;
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

    public Binder? Next { get; }

    public Binder GetNextChecked()
    {
        return Next ?? throw new InvalidOperationException("No next binder");
    }

    public ConversionClassifier ConversionClassifier
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

    public virtual Symbol? ContainingSymbol
    {
        get
        {
            Debug.Assert(Next is not null);
            return Next.ContainingSymbol;
        }
    }

    public virtual SyntaxNode? ScopeDesignator => null;

    public virtual ImmutableArray<VariableSymbol> GetDeclaredLocalVariablesForScope(
        SyntaxNode designator
    )
    {
        Debug.Assert(Next is not null);
        return Next.GetDeclaredLocalVariablesForScope(designator);
    }

    public TypeSymbol ResolveType(TypeSyntax syntax, LookupContext context)
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
            _ => throw new ArgumentOutOfRangeException(nameof(syntax)),
        };
    }

    private TypeSymbol RequireType(LookupResult result, NameSyntax syntax, LookupContext context)
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

    public LookupResult LookupFromSyntax(
        NameSyntax syntax,
        LookupOptions options,
        LookupContext context
    )
    {
        return syntax switch
        {
            SimpleNameSyntax simple => LookupFromSimpleName(simple, options, context),
            QualifiedNameSyntax qualified => LookupFromQualifiedName(qualified, options, context),
            _ => throw new ArgumentOutOfRangeException(nameof(syntax)),
        };
    }

    public LookupResult LookupUnqualifiedName(
        string name,
        LookupOptions options,
        LookupContext context
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

    public LookupResult LookupQualifiedName(
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
        LookupContext context
    )
    {
        return LookupUnqualifiedName(syntax.UnqualifiedName, options, context);
    }

    private LookupResult LookupFromQualifiedName(
        QualifiedNameSyntax syntax,
        LookupOptions options,
        LookupContext context
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
        LookupContext context
    );

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
        var diagnostics = DiagnosticBag.Create();
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

    protected bool VisibleFrom(Symbol symbol)
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

    public BoundStatement BindStatement(
        StatementSyntax syntax,
        TypeSymbol returnType,
        LookupContext context
    )
    {
        return syntax switch
        {
            BlockSyntax blockSyntax => BindBlock(blockSyntax, returnType, context),
            ExpressionStatementSyntax expressionStatementSyntax => BindExpressionStatement(
                expressionStatementSyntax,
                context
            ),
            ReturnStatementSyntax returnStatementSyntax => BindReturnStatement(
                returnStatementSyntax,
                returnType,
                context
            ),
            VariableDeclarationStatementSyntax variableDeclarationStatementSyntax =>
                BindVariableDeclaration(variableDeclarationStatementSyntax, context),
            IfStatementSyntax ifStatementSyntax => BindIfStatement(
                ifStatementSyntax,
                returnType,
                context
            ),
            WhileStatementSyntax whileStatementSyntax => BindWhileStatement(
                whileStatementSyntax,
                returnType,
                context
            ),
            LoopStatementSyntax loopStatementSyntax => BindLoopStatement(
                loopStatementSyntax,
                returnType,
                context
            ),
            ForStatementSyntax forStatementSyntax => BindForStatement(
                forStatementSyntax,
                returnType,
                context
            ),
            BreakStatementSyntax breakStatementSyntax => BindBreakStatement(
                breakStatementSyntax,
                context
            ),
            ContinueStatementSyntax continueStatementSyntax => BindContinueStatement(
                continueStatementSyntax,
                context
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(syntax)),
        };
    }

    public BoundStatement BindExpressionBody(
        ExpressionBodySyntax syntax,
        TypeSymbol returnType,
        LookupContext context
    )
    {
        if (returnType.IsVoid)
        {
            return new BoundExpressionStatement(syntax, BindExpression(syntax.Expression, context));
        }

        return new BoundReturnStatement(
            syntax,
            BindExpression(syntax.Expression, returnType, context)
        );
    }

    private BoundBlock BindBlock(BlockSyntax syntax, TypeSymbol returnType, LookupContext context)
    {
        var builder = ImmutableArray.CreateBuilder<BoundStatement>(syntax.Statements.Count);
        var binder = GetRequiredBinder(syntax);
        foreach (
            var statement in syntax
                .Statements.AsValueEnumerable()
                .Where(statement => statement is not EmptyStatementSyntax)
        )
        {
            builder.Add(binder.BindStatement(statement, returnType, context));
        }

        return new BoundBlock(syntax, builder.DrainToImmutable());
    }

    private BoundVariableDeclaration BindVariableDeclaration(
        VariableDeclarationStatementSyntax syntax,
        LookupContext context
    )
    {
        var semanticModel = Compilation.GetSemanticModel(syntax.SyntaxTree);
        var declaration = syntax.Declaration;
        EnsureLocals();
        var variable =
            semanticModel.GetDeclaredSymbol(declaration)
            ?? throw new InvalidOperationException("Declared variable not found");

        var initializer = declaration.Initializer switch
        {
            not null => declaration.Type is not null
                ? BindExpression(declaration.Initializer.Value, variable.Type, context)
                : semanticModel.GetBoundVariableInitializer(syntax.Declaration, this, context),
            null => null,
        };

        return new BoundVariableDeclaration(syntax, variable, initializer);
    }

    private BoundExpressionStatement BindExpressionStatement(
        ExpressionStatementSyntax syntax,
        LookupContext context
    )
    {
        return new BoundExpressionStatement(syntax, BindExpression(syntax.Expression, context));
    }

    private BoundStatement BindReturnStatement(
        ReturnStatementSyntax syntax,
        TypeSymbol returnType,
        LookupContext context
    )
    {
        var expression = syntax.Expression switch
        {
            not null => BindExpression(syntax.Expression, returnType, context),
            null => null,
        };

        return new BoundReturnStatement(syntax, expression);
    }

    private BoundIfStatement BindIfStatement(
        IfStatementSyntax syntax,
        TypeSymbol returnType,
        LookupContext context
    )
    {
        var condition = BindExpression(
            syntax.Condition,
            Compilation.GetSpecialType(SpecialType.Bool),
            context
        );
        var thenStatement = BindBlock(syntax.Block, returnType, context);
        var elseStatement = syntax.ElseClause switch
        {
            not null => BindStatement(syntax.ElseClause.Statement, returnType, context),
            null => null,
        };
        return new BoundIfStatement(syntax, condition, thenStatement, elseStatement);
    }

    private BoundWhileStatement BindWhileStatement(
        WhileStatementSyntax syntax,
        TypeSymbol returnType,
        LookupContext context
    )
    {
        var condition = BindExpression(
            syntax.Condition,
            Compilation.GetSpecialType(SpecialType.Bool),
            context
        );
        var loopBody = BindBlock(syntax.Block, returnType, context);
        return new BoundWhileStatement(syntax, condition, loopBody);
    }

    private BoundLoopStatement BindLoopStatement(
        LoopStatementSyntax syntax,
        TypeSymbol returnType,
        LookupContext context
    )
    {
        var loopBody = BindBlock(syntax.Block, returnType, context);
        return new BoundLoopStatement(syntax, loopBody);
    }

    private BoundForStatement BindForStatement(
        ForStatementSyntax syntax,
        TypeSymbol returnType,
        LookupContext context
    )
    {
        var binder = GetRequiredBinder(syntax);
        var variable = syntax.Declaration switch
        {
            not null => binder.BindVariableDeclaration(syntax.Declaration, context),
            null => null,
        };
        var initializers = syntax
            .Initializers.AsValueEnumerable()
            .Select(e => binder.BindExpression(e, context))
            .ToImmutableArray();
        var condition = syntax.Condition switch
        {
            not null => binder.BindExpression(
                syntax.Condition,
                Compilation.GetSpecialType(SpecialType.Bool),
                context
            ),
            null => null,
        };

        var incrementors = syntax
            .Incrementors.AsValueEnumerable()
            .Select(e => binder.BindExpression(e, context))
            .ToImmutableArray();

        var loopBody = binder.BindBlock(syntax.Block, returnType, context);
        return new BoundForStatement(
            syntax,
            variable,
            initializers,
            condition,
            incrementors,
            loopBody
        );
    }

    private BoundBreakStatement BindBreakStatement(
        BreakStatementSyntax syntax,
        LookupContext context
    )
    {
        return new BoundBreakStatement(syntax);
    }

    private BoundContinueStatement BindContinueStatement(
        ContinueStatementSyntax syntax,
        LookupContext context
    )
    {
        return new BoundContinueStatement(syntax);
    }

    public BoundExpression BindExpression(ExpressionSyntax syntax, LookupContext context)
    {
        return BindExpression(syntax, null, context);
    }

    public BoundExpression BindExpression(
        ExpressionSyntax syntax,
        TypeSymbol? targetType,
        LookupContext context
    )
    {
        var bound = syntax switch
        {
            LiteralExpressionSyntax literal => BindLiteralExpression(literal, targetType, context),
            IdentifierExpressionSyntax identifier => BindIdentifierExpression(
                identifier,
                targetType,
                context
            ),
            ParenthesizedExpressionSyntax parenthesized => BindExpression(
                parenthesized.Expression,
                targetType,
                context
            ),
            BinaryExpressionSyntax binary => BindBinaryExpression(binary, targetType, context),
            AssignmentExpressionSyntax assignment => BindAssignmentExpression(
                assignment,
                targetType,
                context
            ),
            PrefixExpressionSyntax prefix => BindPrefixExpression(prefix, targetType, context),
            PostfixExpressionSyntax postfix => BindPostfixExpression(postfix, targetType, context),
            TernaryExpressionSyntax ternary => BindTernaryExpression(ternary, targetType, context),
            InvocationExpressionSyntax invocation => BindInvocationExpression(
                invocation,
                targetType,
                context
            ),
            CastExpressionSyntax cast => BindCastExpression(cast, targetType, context),
            _ => throw new ArgumentException("Invalid expression syntax", nameof(syntax)),
        };

        return targetType is not null
            ? AddConversionIfNecessary(bound, targetType, context)
            : bound;
    }

    private BoundLiteral BindLiteralExpression(
        LiteralExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        var token = syntax.Value;
        var value = EvaluateConstantExpression(token, returnType, context);
        var type = Compilation.GetSpecialType(value.SpecialType);
        return new BoundLiteral(syntax, type, value);
    }

    private BoundExpression BindIdentifierExpression(
        IdentifierExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
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
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        var left = BindExpression(syntax.Left, context);
        var right = BindExpression(syntax.Right, context);
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

        return new BoundBinaryOperation(
            syntax,
            conversion?.Type ?? ErrorTypeSymbol.Unnamed,
            left,
            right,
            operation
        );
    }

    private BoundAssignmentOperation BindAssignmentExpression(
        AssignmentExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        var assignee = BindExpression(syntax.Left, context);
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

        var assigned = BindExpression(syntax.Right, context);
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
        LookupContext context
    )
    {
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
                    return new BoundLiteral(
                        syntax,
                        Compilation.GetSpecialType(negated.SpecialType),
                        negated
                    );
                }
            }
        }

        var operand = BindExpression(syntax.Operand, returnType, context);
        return CreateUnaryOperation(syntax, op, operand, context);
    }

    private BoundUnaryOperation BindPostfixExpression(
        PostfixExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        var operand = BindExpression(syntax.Operand, returnType, context);
        var op = syntax.Op.Kind.ToPostfixOperation();
        return CreateUnaryOperation(syntax, op, operand, context);
    }

    private BoundConditional BindTernaryExpression(
        TernaryExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        var condition = AddConversionIfNecessary(
            BindExpression(syntax.Condition, context),
            Compilation.GetSpecialType(SpecialType.Bool),
            context
        );

        var whenTrue = BindExpression(syntax.WhenTrue, context);
        var whenFalse = BindExpression(syntax.WhenFalse, context);
        if (returnType is not null)
        {
            whenTrue = AddConversionIfNecessary(whenTrue, returnType, context);
            whenFalse = AddConversionIfNecessary(whenFalse, returnType, context);
        }
        else
        {
            whenFalse = AddConversionIfNecessary(
                whenFalse,
                Compilation.GetSpecialType(SpecialType.Void),
                context
            );
            returnType = whenTrue.Type;
        }

        return new BoundConditional(syntax, returnType, condition, whenTrue, whenFalse);
    }

    private BoundExpression BindInvocationExpression(
        InvocationExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        var arguments = new BoundExpression[syntax.Arguments.Arguments.Count];
        foreach (var (i, argumentSyntax) in syntax.Arguments.Arguments.AsValueEnumerable().Index())
        {
            arguments[i] = BindExpression(argumentSyntax.Value, context);
        }

        if (syntax.Callee is IdentifierExpressionSyntax nameSyntax)
        {
            var overloads = LookupFromSyntax(nameSyntax.Value, LookupOptions.Callable, context);
            if (overloads.IsViable)
            {
                var overload = ResolveOverload(
                    overloads,
                    arguments,
                    syntax.Callee.Location,
                    context
                );
                return new BoundInvocation(
                    syntax,
                    overload,
                    ImmutableCollectionsMarshal.AsImmutableArray(arguments)
                );
            }
        }

        var callee = BindExpression(syntax.Callee, context);

        context.ReportDiagnostic(
            Diagnostic.NoCallOperatorDefined(syntax.Callee.Location, callee.Type.ToDisplayString())
        );
        return new BoundInvocation(syntax, ErrorFunctionSymbol.Unnamed, [callee, .. arguments]);
    }

    private BoundExpression BindCastExpression(
        CastExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        var operand = BindExpression(syntax.Operand, context);
        var targetType = ResolveType(syntax.Type, context);
        return AddConversionIfNecessary(operand, targetType, context, isExplicit: true);
    }

    private BoundExpression AddConversionIfNecessary(
        BoundExpression expression,
        TypeSymbol type,
        LookupContext context,
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
        LookupContext context,
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

    private ConstantValue EvaluateConstantExpression(
        SyntaxToken token,
        TypeSymbol? returnType,
        LookupContext context
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
        LookupContext context,
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
        LookupContext context,
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
        LookupContext context
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

    private FunctionSymbol ResolveOverload(
        LookupResult result,
        BoundExpression[] arguments,
        Location location,
        LookupContext context
    )
    {
        // TODO: We need to eventually actually resolve named/default parameters
        Debug.Assert(result.IsViable);
        if (result.Symbols.Length == 1)
        {
            var symbol = (FunctionSymbol)result.Symbol;
            if (symbol.Parameters.Length != arguments.Length)
            {
                context.ReportDiagnostic(
                    Diagnostic.NoOverloadMatchingArgCount(location, arguments.Length)
                );
                return symbol;
            }

            if (!TryMatchOverload(symbol, arguments, context))
            {
                context.ReportDiagnostic(
                    Diagnostic.NoOverloadForArgTypes(location, GetTypeNames(arguments))
                );
            }

            return symbol;
        }

        var matchesArgSize = false;
        foreach (var symbol in result.Symbols.AsValueEnumerable().Cast<FunctionSymbol>())
        {
            if (symbol.Parameters.Length == arguments.Length)
            {
                matchesArgSize = true;
            }

            if (!TryMatchOverload(symbol, arguments, context))
                continue;

            // TODO: We need to figure out disambiguating cases where we get multiple overloads via implicit conversions
            return symbol;
        }

        if (matchesArgSize)
        {
            context.ReportDiagnostic(
                Diagnostic.NoOverloadForArgTypes(location, GetTypeNames(arguments))
            );
        }
        else
        {
            context.ReportDiagnostic(
                Diagnostic.NoOverloadMatchingArgCount(location, arguments.Length)
            );
        }

        return (FunctionSymbol)result.Symbols[0];
    }

    private bool TryMatchOverload(
        FunctionSymbol overload,
        BoundExpression[] arguments,
        LookupContext context
    )
    {
        Debug.Assert(overload.Parameters.Length == arguments.Length);
        var conversions = new Conversion[overload.Parameters.Length];

        var classifier = ConversionClassifier;
        for (var i = 0; i < overload.Parameters.Length; i++)
        {
            var param = overload.Parameters[i];
            var arg = arguments[i];
            var conversion = classifier.ClassifyConversion(arg.Type, param.Type);
            if (!conversion.IsImplicit)
                return false;

            conversions[i] = conversion;
        }

        for (var i = 0; i < overload.Parameters.Length; i++)
        {
            var param = overload.Parameters[i];
            ref var arg = ref arguments[i];
            var conversion = conversions[i];

            arg = AddConversionIfNecessary(arg, param.Type, conversion, context);
        }

        return true;
    }

    private static string GetTypeNames(BoundExpression[] arguments)
    {
        return string.Join(", ", arguments.Select(a => a.Type.Name));
    }
}
