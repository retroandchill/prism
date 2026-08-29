// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public abstract class DeclarationSyntax : SyntaxNode
{
    private protected DeclarationSyntax(GreenDeclaration node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public abstract SyntaxTokenList Modifiers { get; }
};

public sealed class IncompleteDeclarationSyntax : DeclarationSyntax
{
    internal IncompleteDeclarationSyntax(
        GreenIncompleteDeclaration node,
        SyntaxNode? parent,
        int position
    )
        : base(node, parent, position) { }

    public override SyntaxTokenList Modifiers
    {
        get
        {
            return new SyntaxTokenList(
                this,
                ((GreenIncompleteDeclaration)Green).Modifiers,
                Position
            );
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return null;
    }
};

public abstract class NamespaceDeclarationSyntax : DeclarationSyntax
{
    private protected NamespaceDeclarationSyntax(
        GreenNamespaceDeclaration node,
        SyntaxNode? parent,
        int position
    )
        : base(node, parent, position) { }

    public abstract SyntaxToken NamespaceToken { get; }
    public abstract NameSyntax Name { get; }
    public abstract SyntaxList<UsingDirectiveSyntax> Usings { get; }
    public abstract SyntaxList<DeclarationSyntax> Members { get; }
};

public sealed class BlockNamespaceDeclarationSyntax : NamespaceDeclarationSyntax
{
    internal BlockNamespaceDeclarationSyntax(
        GreenBlockNamespaceDeclaration node,
        SyntaxNode? parent,
        int position
    )
        : base(node, parent, position) { }

    public override SyntaxTokenList Modifiers
    {
        get
        {
            return new SyntaxTokenList(
                this,
                ((GreenBlockNamespaceDeclaration)Green).Modifiers,
                Position
            );
        }
    }
    public override SyntaxToken NamespaceToken
    {
        get
        {
            return new SyntaxToken(
                ((GreenBlockNamespaceDeclaration)Green).NamespaceToken,
                this,
                GetSlotPosition(1)
            );
        }
    }
    private NameSyntax? _name;
    public override NameSyntax Name
    {
        get { return GetRed(ref _name, 2); }
    }
    public SyntaxToken OpenBrace
    {
        get
        {
            return new SyntaxToken(
                ((GreenBlockNamespaceDeclaration)Green).OpenBrace,
                this,
                GetSlotPosition(3)
            );
        }
    }
    private SyntaxNode? _usings;
    public override SyntaxList<UsingDirectiveSyntax> Usings
    {
        get
        {
            var red = GetRed(ref _usings, 4);
            return new SyntaxList<UsingDirectiveSyntax>(red);
        }
    }
    private SyntaxNode? _members;
    public override SyntaxList<DeclarationSyntax> Members
    {
        get
        {
            var red = GetRed(ref _members, 5);
            return new SyntaxList<DeclarationSyntax>(red);
        }
    }
    public SyntaxToken CloseBrace
    {
        get
        {
            return new SyntaxToken(
                ((GreenBlockNamespaceDeclaration)Green).CloseBrace,
                this,
                GetSlotPosition(6)
            );
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            2 => GetRed(ref _name, 2),
            4 => GetRed(ref _usings, 4),
            5 => GetRed(ref _members, 5),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            2 => _name,
            4 => _usings,
            5 => _members,
            _ => null,
        };
    }
};

public sealed class FileScopedNamespaceDeclarationSyntax : NamespaceDeclarationSyntax
{
    internal FileScopedNamespaceDeclarationSyntax(
        GreenFileScopedNamespaceDeclaration node,
        SyntaxNode? parent,
        int position
    )
        : base(node, parent, position) { }

    public override SyntaxTokenList Modifiers
    {
        get
        {
            return new SyntaxTokenList(
                this,
                ((GreenFileScopedNamespaceDeclaration)Green).Modifiers,
                Position
            );
        }
    }
    public override SyntaxToken NamespaceToken
    {
        get
        {
            return new SyntaxToken(
                ((GreenFileScopedNamespaceDeclaration)Green).NamespaceToken,
                this,
                GetSlotPosition(1)
            );
        }
    }
    private NameSyntax? _name;
    public override NameSyntax Name
    {
        get { return GetRed(ref _name, 2); }
    }
    public SyntaxToken Semicolon
    {
        get
        {
            return new SyntaxToken(
                ((GreenFileScopedNamespaceDeclaration)Green).Semicolon,
                this,
                GetSlotPosition(3)
            );
        }
    }
    private SyntaxNode? _usings;
    public override SyntaxList<UsingDirectiveSyntax> Usings
    {
        get
        {
            var red = GetRed(ref _usings, 4);
            return new SyntaxList<UsingDirectiveSyntax>(red);
        }
    }
    private SyntaxNode? _members;
    public override SyntaxList<DeclarationSyntax> Members
    {
        get
        {
            var red = GetRed(ref _members, 5);
            return new SyntaxList<DeclarationSyntax>(red);
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            2 => GetRed(ref _name, 2),
            4 => GetRed(ref _usings, 4),
            5 => GetRed(ref _members, 5),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            2 => _name,
            4 => _usings,
            5 => _members,
            _ => null,
        };
    }
};

public sealed class VariableDeclarationSyntax : DeclarationSyntax
{
    internal VariableDeclarationSyntax(
        GreenVariableDeclaration node,
        SyntaxNode? parent,
        int position
    )
        : base(node, parent, position) { }

    public override SyntaxTokenList Modifiers
    {
        get
        {
            return new SyntaxTokenList(this, ((GreenVariableDeclaration)Green).Modifiers, Position);
        }
    }
    public SyntaxToken VarKeyword
    {
        get
        {
            return new SyntaxToken(
                ((GreenVariableDeclaration)Green).VarKeyword,
                this,
                GetSlotPosition(1)
            );
        }
    }
    public SyntaxToken Identifier
    {
        get
        {
            return new SyntaxToken(
                ((GreenVariableDeclaration)Green).Identifier,
                this,
                GetSlotPosition(2)
            );
        }
    }
    private TypeSpecifierSyntax? _type;
    public TypeSpecifierSyntax? Type
    {
        get { return GetRed(ref _type, 3); }
    }
    private InitializerSyntax? _initializer;
    public InitializerSyntax? Initializer
    {
        get { return GetRed(ref _initializer, 4); }
    }
    public SyntaxToken Semicolon
    {
        get
        {
            return new SyntaxToken(
                ((GreenVariableDeclaration)Green).Semicolon,
                this,
                GetSlotPosition(5)
            );
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            3 => GetRed(ref _type, 3),
            4 => GetRed(ref _initializer, 4),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            3 => _type,
            4 => _initializer,
            _ => null,
        };
    }
};

public sealed class FunctionDeclarationSyntax : DeclarationSyntax
{
    internal FunctionDeclarationSyntax(
        GreenFunctionDeclaration node,
        SyntaxNode? parent,
        int position
    )
        : base(node, parent, position) { }

    public override SyntaxTokenList Modifiers
    {
        get
        {
            return new SyntaxTokenList(this, ((GreenFunctionDeclaration)Green).Modifiers, Position);
        }
    }
    public SyntaxToken FuncKeyword
    {
        get
        {
            return new SyntaxToken(
                ((GreenFunctionDeclaration)Green).FuncKeyword,
                this,
                GetSlotPosition(1)
            );
        }
    }
    public SyntaxToken Identifier
    {
        get
        {
            return new SyntaxToken(
                ((GreenFunctionDeclaration)Green).Identifier,
                this,
                GetSlotPosition(2)
            );
        }
    }
    private ParameterListSyntax? _parameters;
    public ParameterListSyntax Parameters
    {
        get { return GetRed(ref _parameters, 3); }
    }
    private TypeSpecifierSyntax? _returnType;
    public TypeSpecifierSyntax? ReturnType
    {
        get { return GetRed(ref _returnType, 4); }
    }
    private BlockSyntax? _body;
    public BlockSyntax? Body
    {
        get { return GetRed(ref _body, 5); }
    }
    private ExpressionBodySyntax? _expressionBody;
    public ExpressionBodySyntax? ExpressionBody
    {
        get { return GetRed(ref _expressionBody, 6); }
    }
    public SyntaxToken? Semicolon
    {
        get
        {
            var green = ((GreenFunctionDeclaration)Green).Semicolon;
            return green is not null ? new SyntaxToken(green, this, GetSlotPosition(7)) : null;
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            3 => GetRed(ref _parameters, 3),
            4 => GetRed(ref _returnType, 4),
            5 => GetRed(ref _body, 5),
            6 => GetRed(ref _expressionBody, 6),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            3 => _parameters,
            4 => _returnType,
            5 => _body,
            6 => _expressionBody,
            _ => null,
        };
    }
};
