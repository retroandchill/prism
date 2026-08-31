using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NetEscapades.EnumGenerators;
using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Mappers;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Syntax;
using ZLinq;

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

    public Binder GetBinderChecked(SyntaxNode node)
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

        BindingHelpers.DiagnoseLookupFailure(result, syntax, LookupOptions.Type, context);
        var names = BindingHelpers.CollectNames(syntax);
        return BindingHelpers.CreateErrorTypeSymbol(
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

    private static LookupResult MakeLookupResult(
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
        var names = BindingHelpers.CollectNames(syntax);
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
        var binder = GetBinder(syntax) ?? this;
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
        var binder = GetBinder(syntax) ?? this;
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
        throw new NotImplementedException();
    }

    private BoundExpression BindLiteralExpression(
        LiteralExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        throw new NotImplementedException();
    }

    private BoundExpression BindIdentifierExpression(
        IdentifierExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        throw new NotImplementedException();
    }

    private BoundExpression BindBinaryExpression(
        BinaryExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        throw new NotImplementedException();
    }

    private BoundExpression BindAssignmentExpression(
        AssignmentExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        throw new NotImplementedException();
    }

    private BoundExpression BindPrefixExpression(
        PrefixExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        throw new NotImplementedException();
    }

    private BoundExpression BindPostfixExpression(
        PostfixExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        throw new NotImplementedException();
    }

    private BoundExpression BindTernaryExpression(
        TernaryExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        throw new NotImplementedException();
    }

    private BoundExpression BindInvocationExpression(
        InvocationExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        throw new NotImplementedException();
    }

    private BoundExpression BindCastExpression(
        CastExpressionSyntax syntax,
        TypeSymbol? returnType,
        LookupContext context
    )
    {
        throw new NotImplementedException();
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
        IntegerLiteralData data,
        TypeSymbol? returnType,
        Location location,
        LookupContext context,
        bool isNegative = false
    )
    {
        throw new NotImplementedException();
    }

    private ConstantValue EvaluateNumericExpression(
        FloatLiteralData data,
        TypeSymbol? returnType,
        Location location,
        LookupContext context,
        bool isNegative = false
    )
    {
        throw new NotImplementedException();
    }
}
