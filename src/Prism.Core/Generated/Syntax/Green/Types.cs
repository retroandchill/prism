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
