// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public sealed class InitializerSyntax : SyntaxNode
{
    internal InitializerSyntax(GreenInitializer node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken EqualSign
    {
        get { return new SyntaxToken(((GreenInitializer)Green).EqualSign, this, Position); }
    }
    private ExpressionSyntax? _value;
    public ExpressionSyntax Value
    {
        get { return GetRed(ref _value, 1); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 1 ? GetRed(ref _value) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 1 ? _value : null;
    }
};

public sealed class TypeSpecifierSyntax : SyntaxNode
{
    internal TypeSpecifierSyntax(GreenTypeSpecifier node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken Colon
    {
        get { return new SyntaxToken(((GreenTypeSpecifier)Green).Colon, this, Position); }
    }
    private TypeSyntax? _type;
    public TypeSyntax Type
    {
        get { return GetRed(ref _type, 1); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 1 ? GetRed(ref _type) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 1 ? _type : null;
    }
};

public sealed class NamedParameterSyntax : SyntaxNode
{
    internal NamedParameterSyntax(GreenNamedParameter node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken Name
    {
        get { return new SyntaxToken(((GreenNamedParameter)Green).Name, this, Position); }
    }
    public SyntaxToken Colon
    {
        get
        {
            return new SyntaxToken(((GreenNamedParameter)Green).Colon, this, GetSlotPosition(1));
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

public sealed class ArgumentListSyntax : SyntaxNode
{
    internal ArgumentListSyntax(GreenArgumentList node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken OpenParen
    {
        get { return new SyntaxToken(((GreenArgumentList)Green).OpenParen, this, Position); }
    }
    private SyntaxNode? _arguments;
    public SeparatedSyntaxList<ArgumentSyntax> Arguments
    {
        get
        {
            var red = GetRed(ref _arguments, 1);
            return new SeparatedSyntaxList<ArgumentSyntax>(red);
        }
    }
    public SyntaxToken CloseParen
    {
        get
        {
            return new SyntaxToken(((GreenArgumentList)Green).CloseParen, this, GetSlotPosition(2));
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 1 ? GetRed(ref _arguments) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 1 ? _arguments : null;
    }
};

public sealed class ArgumentSyntax : SyntaxNode
{
    internal ArgumentSyntax(GreenArgument node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    private NamedParameterSyntax? _name;
    public NamedParameterSyntax? Name
    {
        get { return GetRed(ref _name); }
    }
    private ExpressionSyntax? _value;
    public ExpressionSyntax Value
    {
        get { return GetRed(ref _value, 1); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            0 => GetRed(ref _name),
            1 => GetRed(ref _value, 1),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            0 => _name,
            1 => _value,
            _ => null,
        };
    }
};

public sealed class ParameterListSyntax : SyntaxNode
{
    internal ParameterListSyntax(GreenParameterList node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken OpenParen
    {
        get { return new SyntaxToken(((GreenParameterList)Green).OpenParen, this, Position); }
    }
    private SyntaxNode? _parameters;
    public SeparatedSyntaxList<ParameterSyntax> Parameters
    {
        get
        {
            var red = GetRed(ref _parameters, 1);
            return new SeparatedSyntaxList<ParameterSyntax>(red);
        }
    }
    public SyntaxToken CloseParen
    {
        get
        {
            return new SyntaxToken(
                ((GreenParameterList)Green).CloseParen,
                this,
                GetSlotPosition(2)
            );
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 1 ? GetRed(ref _parameters) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 1 ? _parameters : null;
    }
};

public sealed class ParameterSyntax : SyntaxNode
{
    internal ParameterSyntax(GreenParameter node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken? MutableKeyword
    {
        get
        {
            var green = ((GreenParameter)Green).MutableKeyword;
            return green is not null ? new SyntaxToken(green, this, Position) : null;
        }
    }
    public SyntaxToken Name
    {
        get { return new SyntaxToken(((GreenParameter)Green).Name, this, GetSlotPosition(1)); }
    }
    private TypeSpecifierSyntax? _typeSpecifier;
    public TypeSpecifierSyntax? TypeSpecifier
    {
        get { return GetRed(ref _typeSpecifier, 2); }
    }
    private InitializerSyntax? _defaultValue;
    public InitializerSyntax? DefaultValue
    {
        get { return GetRed(ref _defaultValue, 3); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            2 => GetRed(ref _typeSpecifier, 2),
            3 => GetRed(ref _defaultValue, 3),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            2 => _typeSpecifier,
            3 => _defaultValue,
            _ => null,
        };
    }
};

public sealed class ExpressionBodySyntax : SyntaxNode
{
    internal ExpressionBodySyntax(GreenExpressionBody node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken Arrow
    {
        get { return new SyntaxToken(((GreenExpressionBody)Green).Arrow, this, Position); }
    }
    private ExpressionSyntax? _expression;
    public ExpressionSyntax Expression
    {
        get { return GetRed(ref _expression, 1); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 1 ? GetRed(ref _expression) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 1 ? _expression : null;
    }
};

public sealed class ElseClauseSyntax : SyntaxNode
{
    internal ElseClauseSyntax(GreenElseClause node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken ElseKeyword
    {
        get { return new SyntaxToken(((GreenElseClause)Green).ElseKeyword, this, Position); }
    }
    private StatementSyntax? _statement;
    public StatementSyntax Statement
    {
        get { return GetRed(ref _statement, 1); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 1 ? GetRed(ref _statement) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 1 ? _statement : null;
    }
};
