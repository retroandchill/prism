// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal sealed class GreenUsingDirective : GreenNode
{
    public GreenUsingDirective(GreenToken usingKeyword, GreenName name, GreenToken semicolon)
        : base(SyntaxKind.UsingDirective)
    {
        SlotCount = 3;
        UsingKeyword = usingKeyword;
        AdjustFlagsAndWidth(UsingKeyword);
        Name = name;
        AdjustFlagsAndWidth(Name);
        Semicolon = semicolon;
        AdjustFlagsAndWidth(Semicolon);
    }

    public GreenToken UsingKeyword { get; }
    public GreenName Name { get; }
    public GreenToken Semicolon { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => UsingKeyword,
            1 => Name,
            2 => Semicolon,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new UsingDirectiveSyntax(this, parent, position);
    }

    public GreenUsingDirective WithUsingKeyword(GreenToken usingKeyword)
    {
        if (UsingKeyword == usingKeyword)
            return this;

        return new GreenUsingDirective(usingKeyword, Name, Semicolon) { Diagnostics = Diagnostics };
    }

    public GreenUsingDirective WithName(GreenName name)
    {
        if (Name == name)
            return this;

        return new GreenUsingDirective(UsingKeyword, name, Semicolon) { Diagnostics = Diagnostics };
    }

    public GreenUsingDirective WithSemicolon(GreenToken semicolon)
    {
        if (Semicolon == semicolon)
            return this;

        return new GreenUsingDirective(UsingKeyword, Name, semicolon) { Diagnostics = Diagnostics };
    }

    public override GreenUsingDirective WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenUsingDirective(UsingKeyword, Name, Semicolon) { Diagnostics = diagnostics };
    }

    public GreenUsingDirective Update(GreenToken usingKeyword, GreenName name, GreenToken semicolon)
    {
        if (UsingKeyword == usingKeyword && Name == name && Semicolon == semicolon)
        {
            return this;
        }

        return new GreenUsingDirective(usingKeyword, name, semicolon) { Diagnostics = Diagnostics };
    }
}
