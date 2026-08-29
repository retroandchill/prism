// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public sealed class CompilationUnitSyntax : SyntaxNode
{
    internal CompilationUnitSyntax(GreenCompilationUnit node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    private SyntaxNode? _usings;
    public SyntaxList<UsingDirectiveSyntax> Usings
    {
        get
        {
            var red = GetRed(ref _usings);
            return new SyntaxList<UsingDirectiveSyntax>(red);
        }
    }
    private SyntaxNode? _members;
    public SyntaxList<DeclarationSyntax> Members
    {
        get
        {
            var red = GetRed(ref _members, 1);
            return new SyntaxList<DeclarationSyntax>(red);
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            0 => GetRed(ref _usings),
            1 => GetRed(ref _members, 1),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            0 => _usings,
            1 => _members,
            _ => null,
        };
    }
};
