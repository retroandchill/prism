// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal abstract class GreenDeclaration : GreenNode
{
    protected GreenDeclaration(SyntaxKind kind)
        : base(kind) { }

    public abstract GreenSyntaxList<GreenToken> Modifiers { get; }

    public abstract GreenDeclaration WithModifiers(GreenSyntaxList<GreenToken> value);
}

internal sealed class GreenIncompleteDeclaration : GreenDeclaration
{
    public GreenIncompleteDeclaration(GreenSyntaxList<GreenToken> modifiers)
        : base(SyntaxKind.IncompleteDeclaration)
    {
        Modifiers = modifiers;
    }

    public override GreenSyntaxList<GreenToken> Modifiers { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index == 0 ? Modifiers.Node : null;
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public override GreenIncompleteDeclaration WithModifiers(GreenSyntaxList<GreenToken> modifiers)
    {
        if (Modifiers == modifiers)
            return this;

        return new GreenIncompleteDeclaration(modifiers) { Diagnostics = Diagnostics };
    }

    public override GreenIncompleteDeclaration WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenIncompleteDeclaration(Modifiers) { Diagnostics = diagnostics };
    }

    public GreenIncompleteDeclaration Update(GreenSyntaxList<GreenToken> modifiers)
    {
        if (Modifiers == modifiers)
        {
            return this;
        }

        return new GreenIncompleteDeclaration(modifiers) { Diagnostics = Diagnostics };
    }
}

internal abstract class GreenNamespaceDeclaration : GreenDeclaration
{
    protected GreenNamespaceDeclaration(SyntaxKind kind)
        : base(kind) { }

    public abstract GreenToken NamespaceToken { get; }

    public abstract GreenNamespaceDeclaration WithNamespaceToken(GreenToken value);

    public abstract GreenName Name { get; }

    public abstract GreenNamespaceDeclaration WithName(GreenName value);

    public abstract GreenSyntaxList<GreenUsingDirective> Usings { get; }

    public abstract GreenNamespaceDeclaration WithUsings(
        GreenSyntaxList<GreenUsingDirective> value
    );

    public abstract GreenSyntaxList<GreenDeclaration> Members { get; }

    public abstract GreenNamespaceDeclaration WithMembers(GreenSyntaxList<GreenDeclaration> value);
}

internal sealed class GreenBlockNamespaceDeclaration : GreenNamespaceDeclaration
{
    public GreenBlockNamespaceDeclaration(
        GreenSyntaxList<GreenToken> modifiers,
        GreenToken namespaceToken,
        GreenName name,
        GreenToken openBrace,
        GreenSyntaxList<GreenUsingDirective> usings,
        GreenSyntaxList<GreenDeclaration> members,
        GreenToken closeBrace
    )
        : base(SyntaxKind.BlockNamespaceDeclaration)
    {
        Modifiers = modifiers;
        NamespaceToken = namespaceToken;
        Name = name;
        OpenBrace = openBrace;
        Usings = usings;
        Members = members;
        CloseBrace = closeBrace;
    }

    public override GreenSyntaxList<GreenToken> Modifiers { get; }
    public override GreenToken NamespaceToken { get; }
    public override GreenName Name { get; }
    public GreenToken OpenBrace { get; }
    public override GreenSyntaxList<GreenUsingDirective> Usings { get; }
    public override GreenSyntaxList<GreenDeclaration> Members { get; }
    public GreenToken CloseBrace { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Modifiers.Node,
            1 => NamespaceToken,
            2 => Name,
            3 => OpenBrace,
            4 => Usings.Node,
            5 => Members.Node,
            6 => CloseBrace,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public override GreenBlockNamespaceDeclaration WithModifiers(
        GreenSyntaxList<GreenToken> modifiers
    )
    {
        if (Modifiers == modifiers)
            return this;

        return new GreenBlockNamespaceDeclaration(
            modifiers,
            NamespaceToken,
            Name,
            OpenBrace,
            Usings,
            Members,
            CloseBrace
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenBlockNamespaceDeclaration WithNamespaceToken(GreenToken namespaceToken)
    {
        if (NamespaceToken == namespaceToken)
            return this;

        return new GreenBlockNamespaceDeclaration(
            Modifiers,
            namespaceToken,
            Name,
            OpenBrace,
            Usings,
            Members,
            CloseBrace
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenBlockNamespaceDeclaration WithName(GreenName name)
    {
        if (Name == name)
            return this;

        return new GreenBlockNamespaceDeclaration(
            Modifiers,
            NamespaceToken,
            name,
            OpenBrace,
            Usings,
            Members,
            CloseBrace
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenBlockNamespaceDeclaration WithOpenBrace(GreenToken openBrace)
    {
        if (OpenBrace == openBrace)
            return this;

        return new GreenBlockNamespaceDeclaration(
            Modifiers,
            NamespaceToken,
            Name,
            openBrace,
            Usings,
            Members,
            CloseBrace
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenBlockNamespaceDeclaration WithUsings(
        GreenSyntaxList<GreenUsingDirective> usings
    )
    {
        if (Usings == usings)
            return this;

        return new GreenBlockNamespaceDeclaration(
            Modifiers,
            NamespaceToken,
            Name,
            OpenBrace,
            usings,
            Members,
            CloseBrace
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenBlockNamespaceDeclaration WithMembers(
        GreenSyntaxList<GreenDeclaration> members
    )
    {
        if (Members == members)
            return this;

        return new GreenBlockNamespaceDeclaration(
            Modifiers,
            NamespaceToken,
            Name,
            OpenBrace,
            Usings,
            members,
            CloseBrace
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenBlockNamespaceDeclaration WithCloseBrace(GreenToken closeBrace)
    {
        if (CloseBrace == closeBrace)
            return this;

        return new GreenBlockNamespaceDeclaration(
            Modifiers,
            NamespaceToken,
            Name,
            OpenBrace,
            Usings,
            Members,
            closeBrace
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenBlockNamespaceDeclaration WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenBlockNamespaceDeclaration(
            Modifiers,
            NamespaceToken,
            Name,
            OpenBrace,
            Usings,
            Members,
            CloseBrace
        )
        {
            Diagnostics = diagnostics,
        };
    }

    public GreenBlockNamespaceDeclaration Update(
        GreenSyntaxList<GreenToken> modifiers,
        GreenToken namespaceToken,
        GreenName name,
        GreenToken openBrace,
        GreenSyntaxList<GreenUsingDirective> usings,
        GreenSyntaxList<GreenDeclaration> members,
        GreenToken closeBrace
    )
    {
        if (
            Modifiers == modifiers
            && NamespaceToken == namespaceToken
            && Name == name
            && OpenBrace == openBrace
            && Usings == usings
            && Members == members
            && CloseBrace == closeBrace
        )
        {
            return this;
        }

        return new GreenBlockNamespaceDeclaration(
            modifiers,
            namespaceToken,
            name,
            openBrace,
            usings,
            members,
            closeBrace
        )
        {
            Diagnostics = Diagnostics,
        };
    }
}

internal sealed class GreenFileScopedNamespaceDeclaration : GreenNamespaceDeclaration
{
    public GreenFileScopedNamespaceDeclaration(
        GreenSyntaxList<GreenToken> modifiers,
        GreenToken namespaceToken,
        GreenName name,
        GreenToken semicolon,
        GreenSyntaxList<GreenUsingDirective> usings,
        GreenSyntaxList<GreenDeclaration> members
    )
        : base(SyntaxKind.FileScopedNamespaceDeclaration)
    {
        Modifiers = modifiers;
        NamespaceToken = namespaceToken;
        Name = name;
        Semicolon = semicolon;
        Usings = usings;
        Members = members;
    }

    public override GreenSyntaxList<GreenToken> Modifiers { get; }
    public override GreenToken NamespaceToken { get; }
    public override GreenName Name { get; }
    public GreenToken Semicolon { get; }
    public override GreenSyntaxList<GreenUsingDirective> Usings { get; }
    public override GreenSyntaxList<GreenDeclaration> Members { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Modifiers.Node,
            1 => NamespaceToken,
            2 => Name,
            3 => Semicolon,
            4 => Usings.Node,
            5 => Members.Node,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public override GreenFileScopedNamespaceDeclaration WithModifiers(
        GreenSyntaxList<GreenToken> modifiers
    )
    {
        if (Modifiers == modifiers)
            return this;

        return new GreenFileScopedNamespaceDeclaration(
            modifiers,
            NamespaceToken,
            Name,
            Semicolon,
            Usings,
            Members
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenFileScopedNamespaceDeclaration WithNamespaceToken(
        GreenToken namespaceToken
    )
    {
        if (NamespaceToken == namespaceToken)
            return this;

        return new GreenFileScopedNamespaceDeclaration(
            Modifiers,
            namespaceToken,
            Name,
            Semicolon,
            Usings,
            Members
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenFileScopedNamespaceDeclaration WithName(GreenName name)
    {
        if (Name == name)
            return this;

        return new GreenFileScopedNamespaceDeclaration(
            Modifiers,
            NamespaceToken,
            name,
            Semicolon,
            Usings,
            Members
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenFileScopedNamespaceDeclaration WithSemicolon(GreenToken semicolon)
    {
        if (Semicolon == semicolon)
            return this;

        return new GreenFileScopedNamespaceDeclaration(
            Modifiers,
            NamespaceToken,
            Name,
            semicolon,
            Usings,
            Members
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenFileScopedNamespaceDeclaration WithUsings(
        GreenSyntaxList<GreenUsingDirective> usings
    )
    {
        if (Usings == usings)
            return this;

        return new GreenFileScopedNamespaceDeclaration(
            Modifiers,
            NamespaceToken,
            Name,
            Semicolon,
            usings,
            Members
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenFileScopedNamespaceDeclaration WithMembers(
        GreenSyntaxList<GreenDeclaration> members
    )
    {
        if (Members == members)
            return this;

        return new GreenFileScopedNamespaceDeclaration(
            Modifiers,
            NamespaceToken,
            Name,
            Semicolon,
            Usings,
            members
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenFileScopedNamespaceDeclaration WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenFileScopedNamespaceDeclaration(
            Modifiers,
            NamespaceToken,
            Name,
            Semicolon,
            Usings,
            Members
        )
        {
            Diagnostics = diagnostics,
        };
    }

    public GreenFileScopedNamespaceDeclaration Update(
        GreenSyntaxList<GreenToken> modifiers,
        GreenToken namespaceToken,
        GreenName name,
        GreenToken semicolon,
        GreenSyntaxList<GreenUsingDirective> usings,
        GreenSyntaxList<GreenDeclaration> members
    )
    {
        if (
            Modifiers == modifiers
            && NamespaceToken == namespaceToken
            && Name == name
            && Semicolon == semicolon
            && Usings == usings
            && Members == members
        )
        {
            return this;
        }

        return new GreenFileScopedNamespaceDeclaration(
            modifiers,
            namespaceToken,
            name,
            semicolon,
            usings,
            members
        )
        {
            Diagnostics = Diagnostics,
        };
    }
}

internal sealed class GreenVariableDeclaration : GreenDeclaration
{
    public GreenVariableDeclaration(
        GreenSyntaxList<GreenToken> modifiers,
        GreenToken varKeyword,
        GreenToken identifier,
        GreenTypeSpecifier? type,
        GreenInitializer? initializer,
        GreenToken semicolon
    )
        : base(SyntaxKind.VariableDeclaration)
    {
        Modifiers = modifiers;
        VarKeyword = varKeyword;
        Identifier = identifier;
        Type = type;
        Initializer = initializer;
        Semicolon = semicolon;
    }

    public override GreenSyntaxList<GreenToken> Modifiers { get; }
    public GreenToken VarKeyword { get; }
    public GreenToken Identifier { get; }
    public GreenTypeSpecifier? Type { get; }
    public GreenInitializer? Initializer { get; }
    public GreenToken Semicolon { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Modifiers.Node,
            1 => VarKeyword,
            2 => Identifier,
            3 => Type,
            4 => Initializer,
            5 => Semicolon,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public override GreenVariableDeclaration WithModifiers(GreenSyntaxList<GreenToken> modifiers)
    {
        if (Modifiers == modifiers)
            return this;

        return new GreenVariableDeclaration(
            modifiers,
            VarKeyword,
            Identifier,
            Type,
            Initializer,
            Semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenVariableDeclaration WithVarKeyword(GreenToken varKeyword)
    {
        if (VarKeyword == varKeyword)
            return this;

        return new GreenVariableDeclaration(
            Modifiers,
            varKeyword,
            Identifier,
            Type,
            Initializer,
            Semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenVariableDeclaration WithIdentifier(GreenToken identifier)
    {
        if (Identifier == identifier)
            return this;

        return new GreenVariableDeclaration(
            Modifiers,
            VarKeyword,
            identifier,
            Type,
            Initializer,
            Semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenVariableDeclaration WithType(GreenTypeSpecifier? type)
    {
        if (Type == type)
            return this;

        return new GreenVariableDeclaration(
            Modifiers,
            VarKeyword,
            Identifier,
            type,
            Initializer,
            Semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenVariableDeclaration WithInitializer(GreenInitializer? initializer)
    {
        if (Initializer == initializer)
            return this;

        return new GreenVariableDeclaration(
            Modifiers,
            VarKeyword,
            Identifier,
            Type,
            initializer,
            Semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenVariableDeclaration WithSemicolon(GreenToken semicolon)
    {
        if (Semicolon == semicolon)
            return this;

        return new GreenVariableDeclaration(
            Modifiers,
            VarKeyword,
            Identifier,
            Type,
            Initializer,
            semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenVariableDeclaration WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenVariableDeclaration(
            Modifiers,
            VarKeyword,
            Identifier,
            Type,
            Initializer,
            Semicolon
        )
        {
            Diagnostics = diagnostics,
        };
    }

    public GreenVariableDeclaration Update(
        GreenSyntaxList<GreenToken> modifiers,
        GreenToken varKeyword,
        GreenToken identifier,
        GreenTypeSpecifier? type,
        GreenInitializer? initializer,
        GreenToken semicolon
    )
    {
        if (
            Modifiers == modifiers
            && VarKeyword == varKeyword
            && Identifier == identifier
            && Type == type
            && Initializer == initializer
            && Semicolon == semicolon
        )
        {
            return this;
        }

        return new GreenVariableDeclaration(
            modifiers,
            varKeyword,
            identifier,
            type,
            initializer,
            semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }
}

internal sealed class GreenFunctionDeclaration : GreenDeclaration
{
    public GreenFunctionDeclaration(
        GreenSyntaxList<GreenToken> modifiers,
        GreenToken funcKeyword,
        GreenToken identifier,
        GreenParameterList parameters,
        GreenTypeSpecifier? returnType,
        GreenBlock? body,
        GreenExpressionBody? expressionBody,
        GreenToken? semicolon
    )
        : base(SyntaxKind.FunctionDeclaration)
    {
        Modifiers = modifiers;
        FuncKeyword = funcKeyword;
        Identifier = identifier;
        Parameters = parameters;
        ReturnType = returnType;
        Body = body;
        ExpressionBody = expressionBody;
        Semicolon = semicolon;
    }

    public override GreenSyntaxList<GreenToken> Modifiers { get; }
    public GreenToken FuncKeyword { get; }
    public GreenToken Identifier { get; }
    public GreenParameterList Parameters { get; }
    public GreenTypeSpecifier? ReturnType { get; }
    public GreenBlock? Body { get; }
    public GreenExpressionBody? ExpressionBody { get; }
    public GreenToken? Semicolon { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Modifiers.Node,
            1 => FuncKeyword,
            2 => Identifier,
            3 => Parameters,
            4 => ReturnType,
            5 => Body,
            6 => ExpressionBody,
            7 => Semicolon,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        throw new NotImplementedException();
    }

    public override GreenFunctionDeclaration WithModifiers(GreenSyntaxList<GreenToken> modifiers)
    {
        if (Modifiers == modifiers)
            return this;

        return new GreenFunctionDeclaration(
            modifiers,
            FuncKeyword,
            Identifier,
            Parameters,
            ReturnType,
            Body,
            ExpressionBody,
            Semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenFunctionDeclaration WithFuncKeyword(GreenToken funcKeyword)
    {
        if (FuncKeyword == funcKeyword)
            return this;

        return new GreenFunctionDeclaration(
            Modifiers,
            funcKeyword,
            Identifier,
            Parameters,
            ReturnType,
            Body,
            ExpressionBody,
            Semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenFunctionDeclaration WithIdentifier(GreenToken identifier)
    {
        if (Identifier == identifier)
            return this;

        return new GreenFunctionDeclaration(
            Modifiers,
            FuncKeyword,
            identifier,
            Parameters,
            ReturnType,
            Body,
            ExpressionBody,
            Semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenFunctionDeclaration WithParameters(GreenParameterList parameters)
    {
        if (Parameters == parameters)
            return this;

        return new GreenFunctionDeclaration(
            Modifiers,
            FuncKeyword,
            Identifier,
            parameters,
            ReturnType,
            Body,
            ExpressionBody,
            Semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenFunctionDeclaration WithReturnType(GreenTypeSpecifier? returnType)
    {
        if (ReturnType == returnType)
            return this;

        return new GreenFunctionDeclaration(
            Modifiers,
            FuncKeyword,
            Identifier,
            Parameters,
            returnType,
            Body,
            ExpressionBody,
            Semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenFunctionDeclaration WithBody(GreenBlock? body)
    {
        if (Body == body)
            return this;

        return new GreenFunctionDeclaration(
            Modifiers,
            FuncKeyword,
            Identifier,
            Parameters,
            ReturnType,
            body,
            ExpressionBody,
            Semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenFunctionDeclaration WithExpressionBody(GreenExpressionBody? expressionBody)
    {
        if (ExpressionBody == expressionBody)
            return this;

        return new GreenFunctionDeclaration(
            Modifiers,
            FuncKeyword,
            Identifier,
            Parameters,
            ReturnType,
            Body,
            expressionBody,
            Semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenFunctionDeclaration WithSemicolon(GreenToken? semicolon)
    {
        if (Semicolon == semicolon)
            return this;

        return new GreenFunctionDeclaration(
            Modifiers,
            FuncKeyword,
            Identifier,
            Parameters,
            ReturnType,
            Body,
            ExpressionBody,
            semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenFunctionDeclaration WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenFunctionDeclaration(
            Modifiers,
            FuncKeyword,
            Identifier,
            Parameters,
            ReturnType,
            Body,
            ExpressionBody,
            Semicolon
        )
        {
            Diagnostics = diagnostics,
        };
    }

    public GreenFunctionDeclaration Update(
        GreenSyntaxList<GreenToken> modifiers,
        GreenToken funcKeyword,
        GreenToken identifier,
        GreenParameterList parameters,
        GreenTypeSpecifier? returnType,
        GreenBlock? body,
        GreenExpressionBody? expressionBody,
        GreenToken? semicolon
    )
    {
        if (
            Modifiers == modifiers
            && FuncKeyword == funcKeyword
            && Identifier == identifier
            && Parameters == parameters
            && ReturnType == returnType
            && Body == body
            && ExpressionBody == expressionBody
            && Semicolon == semicolon
        )
        {
            return this;
        }

        return new GreenFunctionDeclaration(
            modifiers,
            funcKeyword,
            identifier,
            parameters,
            returnType,
            body,
            expressionBody,
            semicolon
        )
        {
            Diagnostics = Diagnostics,
        };
    }
}
