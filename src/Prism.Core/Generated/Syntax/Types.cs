// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public abstract class TypeSyntax : SyntaxNode
{
    private protected TypeSyntax(GreenType node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }
};

public sealed class PredefinedTypeSyntax : TypeSyntax
{
    internal PredefinedTypeSyntax(GreenPredefinedType node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken Keyword
    {
        get { return new SyntaxToken(((GreenPredefinedType)Green).Keyword, this, Position); }
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

public sealed class NamedTypeSyntax : TypeSyntax
{
    internal NamedTypeSyntax(GreenNamedType node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    private NameSyntax? _identifier;
    public NameSyntax Identifier
    {
        get { return GetRed(ref _identifier); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 0 ? GetRed(ref _identifier) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 0 ? _identifier : null;
    }
};

public sealed class ArrayTypeSyntax : TypeSyntax
{
    internal ArrayTypeSyntax(GreenArrayType node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    private TypeSyntax? _elementType;
    public TypeSyntax ElementType
    {
        get { return GetRed(ref _elementType); }
    }
    public SyntaxToken OpenBrace
    {
        get { return new SyntaxToken(((GreenArrayType)Green).OpenBrace, this, GetSlotPosition(1)); }
    }
    private ExpressionSyntax? _size;
    public ExpressionSyntax? Size
    {
        get { return GetRed(ref _size, 2); }
    }
    public SyntaxToken ClosingBrace
    {
        get
        {
            return new SyntaxToken(((GreenArrayType)Green).ClosingBrace, this, GetSlotPosition(3));
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            0 => GetRed(ref _elementType),
            2 => GetRed(ref _size, 2),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            0 => _elementType,
            2 => _size,
            _ => null,
        };
    }
};

public sealed class ReferenceTypeSyntax : TypeSyntax
{
    internal ReferenceTypeSyntax(GreenReferenceType node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    private TypeSyntax? _referencedType;
    public TypeSyntax ReferencedType
    {
        get { return GetRed(ref _referencedType); }
    }
    public SyntaxToken? MutableKeyword
    {
        get
        {
            var green = ((GreenReferenceType)Green).MutableKeyword;
            return green is not null ? new SyntaxToken(green, this, GetSlotPosition(1)) : null;
        }
    }
    public SyntaxToken Ampersand
    {
        get
        {
            return new SyntaxToken(((GreenReferenceType)Green).Ampersand, this, GetSlotPosition(2));
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 0 ? GetRed(ref _referencedType) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 0 ? _referencedType : null;
    }
};
