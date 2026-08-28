// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal sealed class GreenInitializer : GreenNode
{
    GreenInitializer(GreenToken equalSign, GreenExpression value)
        : base(SyntaxKind.Initializer)
    {
        EqualSign = equalSign;
        Value = value;
    }

    public GreenToken EqualSign { get; }
    public GreenExpression Value { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => EqualSign,
            1 => Value,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenInitializer WithEqualSign(GreenToken equalSign)
    {
        if (EqualSign == equalSign)
            return this;

        return new GreenInitializer(equalSign, Value) { Diagnostics = Diagnostics };
    }

    public GreenInitializer WithValue(GreenExpression value)
    {
        if (Value == value)
            return this;

        return new GreenInitializer(EqualSign, value) { Diagnostics = Diagnostics };
    }

    public override GreenInitializer WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenInitializer(EqualSign, Value) { Diagnostics = diagnostics };
    }

    public GreenInitializer Update(GreenToken equalSign, GreenExpression value)
    {
        if (EqualSign == equalSign && Value == value)
        {
            return this;
        }

        return new GreenInitializer(equalSign, value) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenTypeSpecifier : GreenNode
{
    GreenTypeSpecifier(GreenToken colon, GreenType type)
        : base(SyntaxKind.TypeSpecifier)
    {
        Colon = colon;
        Type = type;
    }

    public GreenToken Colon { get; }
    public GreenType Type { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Colon,
            1 => Type,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenTypeSpecifier WithColon(GreenToken colon)
    {
        if (Colon == colon)
            return this;

        return new GreenTypeSpecifier(colon, Type) { Diagnostics = Diagnostics };
    }

    public GreenTypeSpecifier WithType(GreenType type)
    {
        if (Type == type)
            return this;

        return new GreenTypeSpecifier(Colon, type) { Diagnostics = Diagnostics };
    }

    public override GreenTypeSpecifier WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenTypeSpecifier(Colon, Type) { Diagnostics = diagnostics };
    }

    public GreenTypeSpecifier Update(GreenToken colon, GreenType type)
    {
        if (Colon == colon && Type == type)
        {
            return this;
        }

        return new GreenTypeSpecifier(colon, type) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenNamedParameter : GreenNode
{
    GreenNamedParameter(GreenToken name, GreenToken colon)
        : base(SyntaxKind.NamedParameter)
    {
        Name = name;
        Colon = colon;
    }

    public GreenToken Name { get; }
    public GreenToken Colon { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Name,
            1 => Colon,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenNamedParameter WithName(GreenToken name)
    {
        if (Name == name)
            return this;

        return new GreenNamedParameter(name, Colon) { Diagnostics = Diagnostics };
    }

    public GreenNamedParameter WithColon(GreenToken colon)
    {
        if (Colon == colon)
            return this;

        return new GreenNamedParameter(Name, colon) { Diagnostics = Diagnostics };
    }

    public override GreenNamedParameter WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenNamedParameter(Name, Colon) { Diagnostics = diagnostics };
    }

    public GreenNamedParameter Update(GreenToken name, GreenToken colon)
    {
        if (Name == name && Colon == colon)
        {
            return this;
        }

        return new GreenNamedParameter(name, colon) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenArgumentList : GreenNode
{
    GreenArgumentList(
        GreenToken openParen,
        GreenSeparatedList<GreenArgument> arguments,
        GreenToken closeParen
    )
        : base(SyntaxKind.ArgumentList)
    {
        OpenParen = openParen;
        Arguments = arguments;
        CloseParen = closeParen;
    }

    public GreenToken OpenParen { get; }
    public GreenSeparatedList<GreenArgument> Arguments { get; }
    public GreenToken CloseParen { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => OpenParen,
            1 => Arguments.Node,
            2 => CloseParen,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenArgumentList WithOpenParen(GreenToken openParen)
    {
        if (OpenParen == openParen)
            return this;

        return new GreenArgumentList(openParen, Arguments, CloseParen)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenArgumentList WithArguments(GreenSeparatedList<GreenArgument> arguments)
    {
        if (Arguments == arguments)
            return this;

        return new GreenArgumentList(OpenParen, arguments, CloseParen)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenArgumentList WithCloseParen(GreenToken closeParen)
    {
        if (CloseParen == closeParen)
            return this;

        return new GreenArgumentList(OpenParen, Arguments, closeParen)
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenArgumentList WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenArgumentList(OpenParen, Arguments, CloseParen)
        {
            Diagnostics = diagnostics,
        };
    }

    public GreenArgumentList Update(
        GreenToken openParen,
        GreenSeparatedList<GreenArgument> arguments,
        GreenToken closeParen
    )
    {
        if (OpenParen == openParen && Arguments == arguments && CloseParen == closeParen)
        {
            return this;
        }

        return new GreenArgumentList(openParen, arguments, closeParen)
        {
            Diagnostics = Diagnostics,
        };
    }
}

internal sealed class GreenArgument : GreenNode
{
    GreenArgument(GreenNamedParameter? name, GreenExpression value)
        : base(SyntaxKind.Argument)
    {
        Name = name;
        Value = value;
    }

    public GreenNamedParameter? Name { get; }
    public GreenExpression Value { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Name,
            1 => Value,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenArgument WithName(GreenNamedParameter? name)
    {
        if (Name == name)
            return this;

        return new GreenArgument(name, Value) { Diagnostics = Diagnostics };
    }

    public GreenArgument WithValue(GreenExpression value)
    {
        if (Value == value)
            return this;

        return new GreenArgument(Name, value) { Diagnostics = Diagnostics };
    }

    public override GreenArgument WithDiagnostics(ImmutableArray<SyntaxDiagnosticInfo> diagnostics)
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenArgument(Name, Value) { Diagnostics = diagnostics };
    }

    public GreenArgument Update(GreenNamedParameter? name, GreenExpression value)
    {
        if (Name == name && Value == value)
        {
            return this;
        }

        return new GreenArgument(name, value) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenParameterList : GreenNode
{
    GreenParameterList(
        GreenToken openParen,
        GreenSeparatedList<GreenParameter> parameters,
        GreenToken closeParen
    )
        : base(SyntaxKind.ParameterList)
    {
        OpenParen = openParen;
        Parameters = parameters;
        CloseParen = closeParen;
    }

    public GreenToken OpenParen { get; }
    public GreenSeparatedList<GreenParameter> Parameters { get; }
    public GreenToken CloseParen { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => OpenParen,
            1 => Parameters.Node,
            2 => CloseParen,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenParameterList WithOpenParen(GreenToken openParen)
    {
        if (OpenParen == openParen)
            return this;

        return new GreenParameterList(openParen, Parameters, CloseParen)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenParameterList WithParameters(GreenSeparatedList<GreenParameter> parameters)
    {
        if (Parameters == parameters)
            return this;

        return new GreenParameterList(OpenParen, parameters, CloseParen)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenParameterList WithCloseParen(GreenToken closeParen)
    {
        if (CloseParen == closeParen)
            return this;

        return new GreenParameterList(OpenParen, Parameters, closeParen)
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenParameterList WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenParameterList(OpenParen, Parameters, CloseParen)
        {
            Diagnostics = diagnostics,
        };
    }

    public GreenParameterList Update(
        GreenToken openParen,
        GreenSeparatedList<GreenParameter> parameters,
        GreenToken closeParen
    )
    {
        if (OpenParen == openParen && Parameters == parameters && CloseParen == closeParen)
        {
            return this;
        }

        return new GreenParameterList(openParen, parameters, closeParen)
        {
            Diagnostics = Diagnostics,
        };
    }
}

internal sealed class GreenParameter : GreenNode
{
    GreenParameter(
        GreenToken? mutableKeyword,
        GreenToken name,
        GreenTypeSpecifier? typeSpecifier,
        GreenInitializer? defaultValue
    )
        : base(SyntaxKind.Parameter)
    {
        MutableKeyword = mutableKeyword;
        Name = name;
        TypeSpecifier = typeSpecifier;
        DefaultValue = defaultValue;
    }

    public GreenToken? MutableKeyword { get; }
    public GreenToken Name { get; }
    public GreenTypeSpecifier? TypeSpecifier { get; }
    public GreenInitializer? DefaultValue { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => MutableKeyword,
            1 => Name,
            2 => TypeSpecifier,
            3 => DefaultValue,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenParameter WithMutableKeyword(GreenToken? mutableKeyword)
    {
        if (MutableKeyword == mutableKeyword)
            return this;

        return new GreenParameter(mutableKeyword, Name, TypeSpecifier, DefaultValue)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenParameter WithName(GreenToken name)
    {
        if (Name == name)
            return this;

        return new GreenParameter(MutableKeyword, name, TypeSpecifier, DefaultValue)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenParameter WithTypeSpecifier(GreenTypeSpecifier? typeSpecifier)
    {
        if (TypeSpecifier == typeSpecifier)
            return this;

        return new GreenParameter(MutableKeyword, Name, typeSpecifier, DefaultValue)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenParameter WithDefaultValue(GreenInitializer? defaultValue)
    {
        if (DefaultValue == defaultValue)
            return this;

        return new GreenParameter(MutableKeyword, Name, TypeSpecifier, defaultValue)
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenParameter WithDiagnostics(ImmutableArray<SyntaxDiagnosticInfo> diagnostics)
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenParameter(MutableKeyword, Name, TypeSpecifier, DefaultValue)
        {
            Diagnostics = diagnostics,
        };
    }

    public GreenParameter Update(
        GreenToken? mutableKeyword,
        GreenToken name,
        GreenTypeSpecifier? typeSpecifier,
        GreenInitializer? defaultValue
    )
    {
        if (
            MutableKeyword == mutableKeyword
            && Name == name
            && TypeSpecifier == typeSpecifier
            && DefaultValue == defaultValue
        )
        {
            return this;
        }

        return new GreenParameter(mutableKeyword, name, typeSpecifier, defaultValue)
        {
            Diagnostics = Diagnostics,
        };
    }
}

internal sealed class GreenExpressionBody : GreenNode
{
    GreenExpressionBody(GreenToken arrow, GreenExpression expression)
        : base(SyntaxKind.ExpressionBody)
    {
        Arrow = arrow;
        Expression = expression;
    }

    public GreenToken Arrow { get; }
    public GreenExpression Expression { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Arrow,
            1 => Expression,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenExpressionBody WithArrow(GreenToken arrow)
    {
        if (Arrow == arrow)
            return this;

        return new GreenExpressionBody(arrow, Expression) { Diagnostics = Diagnostics };
    }

    public GreenExpressionBody WithExpression(GreenExpression expression)
    {
        if (Expression == expression)
            return this;

        return new GreenExpressionBody(Arrow, expression) { Diagnostics = Diagnostics };
    }

    public override GreenExpressionBody WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenExpressionBody(Arrow, Expression) { Diagnostics = diagnostics };
    }

    public GreenExpressionBody Update(GreenToken arrow, GreenExpression expression)
    {
        if (Arrow == arrow && Expression == expression)
        {
            return this;
        }

        return new GreenExpressionBody(arrow, expression) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenElseClause : GreenNode
{
    GreenElseClause(GreenToken elseKeyword, GreenStatement statement)
        : base(SyntaxKind.ElseClause)
    {
        ElseKeyword = elseKeyword;
        Statement = statement;
    }

    public GreenToken ElseKeyword { get; }
    public GreenStatement Statement { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => ElseKeyword,
            1 => Statement,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public GreenElseClause WithElseKeyword(GreenToken elseKeyword)
    {
        if (ElseKeyword == elseKeyword)
            return this;

        return new GreenElseClause(elseKeyword, Statement) { Diagnostics = Diagnostics };
    }

    public GreenElseClause WithStatement(GreenStatement statement)
    {
        if (Statement == statement)
            return this;

        return new GreenElseClause(ElseKeyword, statement) { Diagnostics = Diagnostics };
    }

    public override GreenElseClause WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenElseClause(ElseKeyword, Statement) { Diagnostics = diagnostics };
    }

    public GreenElseClause Update(GreenToken elseKeyword, GreenStatement statement)
    {
        if (ElseKeyword == elseKeyword && Statement == statement)
        {
            return this;
        }

        return new GreenElseClause(elseKeyword, statement) { Diagnostics = Diagnostics };
    }
}
