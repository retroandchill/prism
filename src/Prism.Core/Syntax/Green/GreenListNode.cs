using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal sealed class GreenListNode : GreenNode
{
    private readonly ImmutableArray<GreenNode> _children;

    public GreenListNode(ImmutableArray<GreenNode> children)
        : base(SyntaxKind.List)
    {
        _children = children;
        SlotCount = children.Length;
        foreach (var child in children)
        {
            Debug.Assert(child is not null);
            AdjustFlagsAndWidth(child);
        }
    }

    public override GreenNode? GetSlot(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        return index < _children.Length ? _children[index] : null;
    }

    public GreenListNode WithSlot(int index, GreenNode newSlot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfLessThan(index, _children.Length);
        return new GreenListNode(_children.SetItem(index, newSlot)) { Diagnostics = Diagnostics };
    }

    public override GreenListNode WithDiagnostics(ImmutableArray<SyntaxDiagnosticInfo> diagnostics)
    {
        if (diagnostics == Diagnostics)
            return this;

        return new GreenListNode(_children) { Diagnostics = diagnostics };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent, int positon = 0)
    {
        throw new NotImplementedException();
    }

    public readonly struct Builder()
    {
        private readonly ImmutableArray<GreenNode>.Builder _children =
            ImmutableArray.CreateBuilder<GreenNode>();

        public int Count => _children.Count;

        public void Add(GreenNode node)
        {
            _children.Add(node);
        }

        public void AddRange(ReadOnlySpan<GreenNode> nodes)
        {
            _children.AddRange(nodes);
        }

        public GreenListNode Build()
        {
            return new GreenListNode(_children.ToImmutable());
        }

        public GreenListNode BuildAndClear()
        {
            return new GreenListNode(_children.DrainToImmutable());
        }
    }
}
