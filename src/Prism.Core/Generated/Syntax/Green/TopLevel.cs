// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal sealed class GreenCompilationUnit : GreenNode
{
    public GreenCompilationUnit(
        GreenSyntaxList<GreenUsingDirective> usings,
        GreenSyntaxList<GreenDeclaration> members
    )
        : base(SyntaxKind.CompilationUnit)
    {
        SlotCount = 2;
        Usings = usings;
        AdjustFlagsAndWidth(Usings);
        Members = members;
        AdjustFlagsAndWidth(Members);
    }

    public GreenSyntaxList<GreenUsingDirective> Usings { get; }
    public GreenSyntaxList<GreenDeclaration> Members { get; }

    public override GreenNode? GetSlot(int index)
    {
        return index switch
        {
            0 => Usings.Node,
            1 => Members.Node,
            _ => null,
        };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int position = 0)
    {
        return new CompilationUnitSyntax(this, parent, position);
    }

    public GreenCompilationUnit WithUsings(GreenSyntaxList<GreenUsingDirective> usings)
    {
        if (Usings == usings)
            return this;

        return new GreenCompilationUnit(usings, Members) { Diagnostics = Diagnostics };
    }

    public GreenCompilationUnit WithMembers(GreenSyntaxList<GreenDeclaration> members)
    {
        if (Members == members)
            return this;

        return new GreenCompilationUnit(Usings, members) { Diagnostics = Diagnostics };
    }

    public override GreenCompilationUnit WithDiagnostics(
        ImmutableArray<SyntaxDiagnosticInfo> diagnostics
    )
    {
        if (Diagnostics == diagnostics)
            return this;

        return new GreenCompilationUnit(Usings, Members) { Diagnostics = diagnostics };
    }

    public GreenCompilationUnit Update(
        GreenSyntaxList<GreenUsingDirective> usings,
        GreenSyntaxList<GreenDeclaration> members
    )
    {
        if (Usings == usings && Members == members)
        {
            return this;
        }

        return new GreenCompilationUnit(usings, members) { Diagnostics = Diagnostics };
    }
}
