using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Prism.Core.Diagnostics;
using Prism.Core.Parser;
using Prism.Core.Syntax.Green;
using Prism.Core.Text;

namespace Prism.Core.Syntax;

public sealed class SyntaxTree
{
    internal SyntaxTree(string path, SourceText text, GreenNode root)
    {
        Path = path;
        Text = text;
        Root = root.CreateRed();
    }

    internal SyntaxTree(SourceText text, GreenNode node)
        : this("", text, node) { }

    internal SyntaxTree(SyntaxNode root)
    {
        Root = root;
    }

    public static SyntaxTree Parse(string text)
    {
        return Parse(new SourceText(text));
    }

    public static SyntaxTree Parse(SourceText text)
    {
        var parser = new LanguageParser(text.Text);
        var root = parser.ParseCompilationUnit();
        return new SyntaxTree(text, root);
    }

    public string Path { get; } = "";

    public SourceText? Text { get; }

    public SyntaxNode Root { get; }

    [MemberNotNull(nameof(Text))]
    public FileSourcePositionSpan GetPositionSpan(TextSpan span)
    {
        return Text is not null
            ? new FileSourcePositionSpan(
                Path,
                Text.PositionOf(span.Start),
                Text.PositionOf(span.End)
            )
            : throw new InvalidOperationException("Text is null");
    }

    public IEnumerable<Diagnostic> GetDiagnostics()
    {
        return GetDiagnostics(Root);
    }

    public IEnumerable<Diagnostic> GetDiagnostics(SyntaxNode node)
    {
        return GetDiagnostics(node.Green, node.Position);
    }

    public IEnumerable<Diagnostic> GetDiagnostics(in SyntaxToken token)
    {
        return GetDiagnostics(token.Green, token.Position);
    }

    public IEnumerable<Diagnostic> GetDiagnostics(in SyntaxTrivia trivia)
    {
        return GetDiagnostics(trivia.Green, trivia.Position);
    }

    public IEnumerable<Diagnostic> GetDiagnostics(in SyntaxNodeOrToken node)
    {
        return node.TryGetValue(out SyntaxNode? syntaxNode)
            ? GetDiagnostics(syntaxNode)
            : GetDiagnostics(node.AsSyntaxToken());
    }

    private readonly record struct NodeIteration(
        GreenNode Node,
        int SlotIndex = 0,
        bool ProcessedDiagnostics = false
    );

    private IEnumerable<Diagnostic> GetDiagnostics(GreenNode root, int position)
    {
        if (!root.ContainsDiagnostics)
        {
            yield break;
        }

        var stack = new List<NodeIteration> { new(root) };

        var fullTreeLength = Root.FullSpan.Length;
        while (stack.Count > 0)
        {
            var topIndex = stack.Count - 1;
            var frame = stack[topIndex];

            if (!frame.ProcessedDiagnostics)
            {
                foreach (var info in frame.Node.Diagnostics)
                {
                    var leadingWidthToAdd = frame.Node.IsToken ? 0 : frame.Node.LeadingTriviaWidth;

                    var spanStart = Math.Min(
                        position + leadingWidthToAdd + info.Offset,
                        fullTreeLength
                    );
                    var spanEnd = Math.Min(spanStart + info.Width, fullTreeLength);
                    yield return new Diagnostic(
                        info.Info,
                        new SourceLocation(this, TextSpan.FromBounds(spanStart, spanEnd))
                    );
                }

                stack[topIndex] = frame with { ProcessedDiagnostics = true };
            }

            ProcessNode(frame.Node, ref position, stack);
        }
    }

    private static void ProcessNode(GreenNode node, ref int position, List<NodeIteration> stack)
    {
        if (node.SlotCount == 0)
        {
            position += node.Width;
            return;
        }

        Debug.Assert(stack.Count > 0, "Stack is empty");
        for (
            var nextSlotIndex = stack[^1].SlotIndex;
            nextSlotIndex < node.SlotCount;
            nextSlotIndex++
        )
        {
            var child = node.GetSlot(nextSlotIndex);
            if (child is null)
                continue;

            if (!child.ContainsDiagnostics)
            {
                position += child.FullWidth;
                continue;
            }

            stack[^1] = stack[^1] with { SlotIndex = nextSlotIndex };
            stack.Add(new NodeIteration(child));
        }
    }
}
