// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal abstract class GreenType : GreenNode
{
    protected GreenType(SyntaxKind kind)
        : base(kind) { }
}

internal sealed class GreenPredefinedType : GreenType
{
    public GreenPredefinedType(GreenToken keyword)
        : base(SyntaxKind.PredefinedType)
    {
        SlotCount = 1;
        Keyword = keyword;
        AdjustFlagsAndWidth(Keyword);
    }

    public GreenToken Keyword { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index == 0 ? Keyword : null;
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new PredefinedTypeSyntax(this, parent, position);
    }

    public GreenPredefinedType WithKeyword(GreenToken keyword)
    {
        if (Keyword == keyword)
            return this;

        return new GreenPredefinedType(keyword) { Diagnostics = Diagnostics };
    }

    public override GreenPredefinedType WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenPredefinedType(Keyword) { Diagnostics = diagnostics };
    }

    public GreenPredefinedType Update(GreenToken keyword)
    {
        if (Keyword == keyword)
        {
            return this;
        }

        return new GreenPredefinedType(keyword) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenNamedType : GreenType
{
    public GreenNamedType(GreenName identifier)
        : base(SyntaxKind.NamedType)
    {
        SlotCount = 1;
        Identifier = identifier;
        AdjustFlagsAndWidth(Identifier);
    }

    public GreenName Identifier { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index == 0 ? Identifier : null;
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new NamedTypeSyntax(this, parent, position);
    }

    public GreenNamedType WithIdentifier(GreenName identifier)
    {
        if (Identifier == identifier)
            return this;

        return new GreenNamedType(identifier) { Diagnostics = Diagnostics };
    }

    public override GreenNamedType WithDiagnostics(ImmutableArray<SyntaxDiagnosticInfo> diagnostics)
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenNamedType(Identifier) { Diagnostics = diagnostics };
    }

    public GreenNamedType Update(GreenName identifier)
    {
        if (Identifier == identifier)
        {
            return this;
        }

        return new GreenNamedType(identifier) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenArrayType : GreenType
{
    public GreenArrayType(
        GreenType elementType,
        GreenToken openBrace,
        GreenExpression? size,
        GreenToken closingBrace
    )
        : base(SyntaxKind.ArrayType)
    {
        SlotCount = 4;
        ElementType = elementType;
        AdjustFlagsAndWidth(ElementType);
        OpenBrace = openBrace;
        AdjustFlagsAndWidth(OpenBrace);
        Size = size;
        if (Size is not null)
            AdjustFlagsAndWidth(Size);
        ClosingBrace = closingBrace;
        AdjustFlagsAndWidth(ClosingBrace);
    }

    public GreenType ElementType { get; }
    public GreenToken OpenBrace { get; }
    public GreenExpression? Size { get; }
    public GreenToken ClosingBrace { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => ElementType,
            1 => OpenBrace,
            2 => Size,
            3 => ClosingBrace,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new ArrayTypeSyntax(this, parent, position);
    }

    public GreenArrayType WithElementType(GreenType elementType)
    {
        if (ElementType == elementType)
            return this;

        return new GreenArrayType(elementType, OpenBrace, Size, ClosingBrace)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenArrayType WithOpenBrace(GreenToken openBrace)
    {
        if (OpenBrace == openBrace)
            return this;

        return new GreenArrayType(ElementType, openBrace, Size, ClosingBrace)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenArrayType WithSize(GreenExpression? size)
    {
        if (Size == size)
            return this;

        return new GreenArrayType(ElementType, OpenBrace, size, ClosingBrace)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenArrayType WithClosingBrace(GreenToken closingBrace)
    {
        if (ClosingBrace == closingBrace)
            return this;

        return new GreenArrayType(ElementType, OpenBrace, Size, closingBrace)
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenArrayType WithDiagnostics(ImmutableArray<SyntaxDiagnosticInfo> diagnostics)
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenArrayType(ElementType, OpenBrace, Size, ClosingBrace)
        {
            Diagnostics = diagnostics,
        };
    }

    public GreenArrayType Update(
        GreenType elementType,
        GreenToken openBrace,
        GreenExpression? size,
        GreenToken closingBrace
    )
    {
        if (
            ElementType == elementType
            && OpenBrace == openBrace
            && Size == size
            && ClosingBrace == closingBrace
        )
        {
            return this;
        }

        return new GreenArrayType(elementType, openBrace, size, closingBrace)
        {
            Diagnostics = Diagnostics,
        };
    }
}

internal sealed class GreenReferenceType : GreenType
{
    public GreenReferenceType(
        GreenType referencedType,
        GreenToken? mutableKeyword,
        GreenToken ampersand
    )
        : base(SyntaxKind.ReferenceType)
    {
        SlotCount = 3;
        ReferencedType = referencedType;
        AdjustFlagsAndWidth(ReferencedType);
        MutableKeyword = mutableKeyword;
        if (MutableKeyword is not null)
            AdjustFlagsAndWidth(MutableKeyword);
        Ampersand = ampersand;
        AdjustFlagsAndWidth(Ampersand);
    }

    public GreenType ReferencedType { get; }
    public GreenToken? MutableKeyword { get; }
    public GreenToken Ampersand { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => ReferencedType,
            1 => MutableKeyword,
            2 => Ampersand,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new ReferenceTypeSyntax(this, parent, position);
    }

    public GreenReferenceType WithReferencedType(GreenType referencedType)
    {
        if (ReferencedType == referencedType)
            return this;

        return new GreenReferenceType(referencedType, MutableKeyword, Ampersand)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenReferenceType WithMutableKeyword(GreenToken? mutableKeyword)
    {
        if (MutableKeyword == mutableKeyword)
            return this;

        return new GreenReferenceType(ReferencedType, mutableKeyword, Ampersand)
        {
            Diagnostics = Diagnostics,
        };
    }

    public GreenReferenceType WithAmpersand(GreenToken ampersand)
    {
        if (Ampersand == ampersand)
            return this;

        return new GreenReferenceType(ReferencedType, MutableKeyword, ampersand)
        {
            Diagnostics = Diagnostics,
        };
    }

    public override GreenReferenceType WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenReferenceType(ReferencedType, MutableKeyword, Ampersand)
        {
            Diagnostics = diagnostics,
        };
    }

    public GreenReferenceType Update(
        GreenType referencedType,
        GreenToken? mutableKeyword,
        GreenToken ampersand
    )
    {
        if (
            ReferencedType == referencedType
            && MutableKeyword == mutableKeyword
            && Ampersand == ampersand
        )
        {
            return this;
        }

        return new GreenReferenceType(referencedType, mutableKeyword, ampersand)
        {
            Diagnostics = Diagnostics,
        };
    }
}
