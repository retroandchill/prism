// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal abstract class GreenName : GreenNode
{
    protected GreenName(SyntaxKind kind)
        : base(kind) { }
}

internal sealed class GreenSimpleName : GreenName
{
    public GreenSimpleName(GreenToken identifier)
        : base(SyntaxKind.SimpleName)
    {
        Identifier = identifier;
    }

    public GreenToken Identifier { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index == 0 ? Identifier : null;
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new SimpleNameSyntax(this, parent, position);
    }

    public GreenSimpleName WithIdentifier(GreenToken identifier)
    {
        if (Identifier == identifier)
            return this;

        return new GreenSimpleName(identifier) { Diagnostics = Diagnostics };
    }

    public override GreenSimpleName WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenSimpleName(Identifier) { Diagnostics = diagnostics };
    }

    public GreenSimpleName Update(GreenToken identifier)
    {
        if (Identifier == identifier)
        {
            return this;
        }

        return new GreenSimpleName(identifier) { Diagnostics = Diagnostics };
    }
}

internal sealed class GreenQualifiedName : GreenName
{
    public GreenQualifiedName(GreenName left, GreenToken separator, GreenSimpleName right)
        : base(SyntaxKind.QualifiedName)
    {
        Left = left;
        Separator = separator;
        Right = right;
    }

    public GreenName Left { get; }
    public GreenToken Separator { get; }
    public GreenSimpleName Right { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Left,
            1 => Separator,
            2 => Right,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new QualifiedNameSyntax(this, parent, position);
    }

    public GreenQualifiedName WithLeft(GreenName left)
    {
        if (Left == left)
            return this;

        return new GreenQualifiedName(left, Separator, Right) { Diagnostics = Diagnostics };
    }

    public GreenQualifiedName WithSeparator(GreenToken separator)
    {
        if (Separator == separator)
            return this;

        return new GreenQualifiedName(Left, separator, Right) { Diagnostics = Diagnostics };
    }

    public GreenQualifiedName WithRight(GreenSimpleName right)
    {
        if (Right == right)
            return this;

        return new GreenQualifiedName(Left, Separator, right) { Diagnostics = Diagnostics };
    }

    public override GreenQualifiedName WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenQualifiedName(Left, Separator, Right) { Diagnostics = diagnostics };
    }

    public GreenQualifiedName Update(GreenName left, GreenToken separator, GreenSimpleName right)
    {
        if (Left == left && Separator == separator && Right == right)
        {
            return this;
        }

        return new GreenQualifiedName(left, separator, right) { Diagnostics = Diagnostics };
    }
}
