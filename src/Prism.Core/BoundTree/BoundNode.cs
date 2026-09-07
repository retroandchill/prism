using System.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.BoundTree;

internal abstract record BoundNode
{
    [Flags]
    private enum BoundNodeFlags : ushort
    {
        None = 0,
        HasErrors = 1 << 0,
        CompilerGenerated = 1 << 1,
    }

    // ReSharper disable once ConvertToPrimaryConstructor
    protected BoundNode(SyntaxNode syntax)
    {
        Syntax = syntax;
        _flags = BoundNodeFlags.None;
    }

    public SyntaxNode Syntax { get; }
    private readonly BoundNodeFlags _flags;

    public bool HasAnyErrors
    {
        get
        {
            if (HasErrors || Syntax is { HasErrors: true })
            {
                return true;
            }

            return this is BoundExpression { Type: { IsErrorType: true } };
        }
    }

    public bool HasErrors
    {
        get => _flags.HasFlag(BoundNodeFlags.HasErrors);
        init
        {
            if (value)
            {
                _flags |= BoundNodeFlags.HasErrors;
            }
            else
            {
                Debug.Assert(
                    !_flags.HasFlag(BoundNodeFlags.HasErrors),
                    "Can't remove the HasErrors flag"
                );
            }
        }
    }

    public bool WasCompilerGenerated
    {
        get => _flags.HasFlag(BoundNodeFlags.CompilerGenerated);
        init
        {
            if (value)
            {
                _flags |= BoundNodeFlags.CompilerGenerated;
            }
            else
            {
                Debug.Assert(
                    !_flags.HasFlag(BoundNodeFlags.CompilerGenerated),
                    "Can't remove the CompilerGenerated flag"
                );
            }
        }
    }
}
