using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public readonly struct SyntaxNodeOrToken
{
    private readonly SyntaxNode? _nodeOrParent;
    private readonly GreenToken? _token;

    public SyntaxNodeOrToken(SyntaxNode node)
    {
        Debug.Assert(!node.Green.IsList, "Node cannot be a list");
        Position = node.Position;
        _nodeOrParent = node;
    }

    public SyntaxNodeOrToken(SyntaxToken token)
    {
        _nodeOrParent = token.Parent;
        _token = token.Green;
        Position = token.Position;
    }

    public SyntaxKind Kind => _token?.Kind ?? _nodeOrParent!.Kind;

    public bool IsMissing => _token?.IsMissing ?? _nodeOrParent!.IsMissing;

    public SyntaxNode? Parent => _token is not null ? _nodeOrParent : _nodeOrParent?.Parent;

    internal GreenNode? UnderlyingNode => _token ?? _nodeOrParent?.Green;

    internal int Position { get; }

    internal int Width => _token?.Width ?? _nodeOrParent!.Width;

    public int EndPosition => Position + Width;

    [MemberNotNullWhen(true, nameof(_token))]
    public bool IsToken => !IsNode;

    [MemberNotNullWhen(false, nameof(_token))]
    public bool IsNode => _token is null;

    public object Value
    {
        get
        {
            if (IsToken)
            {
                return new SyntaxToken(_token, _nodeOrParent, Position);
            }

            return IsNode
                ? _nodeOrParent!
                : throw new InvalidOperationException("SyntaxNodeOrToken is in default state");
        }
    }

    public SyntaxNode AsSyntaxNode()
    {
        return IsNode
            ? _nodeOrParent!
            : throw new InvalidOperationException("SyntaxNodeOrToken is not a node");
    }

    public bool TryGetValue([NotNullWhen(true)] out SyntaxNode? value)
    {
        if (IsNode)
        {
            value = _nodeOrParent!;
            return true;
        }

        value = null;
        return false;
    }

    public SyntaxToken AsSyntaxToken()
    {
        return IsToken
            ? new SyntaxToken(_token, _nodeOrParent, Position)
            : throw new InvalidOperationException("SyntaxNodeOrToken is not a token");
    }

    public bool TryGetValue(out SyntaxToken value)
    {
        if (IsToken)
        {
            value = new SyntaxToken(_token, _nodeOrParent, Position);
            return true;
        }

        value = default;
        return false;
    }

    public static implicit operator SyntaxNodeOrToken(SyntaxNode node) => new(node);

    public static implicit operator SyntaxNodeOrToken(SyntaxToken token) => new(token);
}
