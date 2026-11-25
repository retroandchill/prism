using System.Diagnostics;

namespace Prism.BoundTree;

internal closed record BoundNode
{
    [Flags]
    private enum BoundNodeFlags : ushort
    {
        None = 0,
        HasErrors = 1 << 0,
        CompilerGenerated = 1 << 1,
    }

    // ReSharper disable once ConvertToPrimaryConstructor
    protected BoundNode(IBoundSemanticContext semanticContext, IBoundSource syntax)
    {
        SemanticContext = semanticContext;
        Syntax = syntax;
        _flags = BoundNodeFlags.None;
    }

    public IBoundSemanticContext SemanticContext { get; }

    public IBoundSource Syntax { get; }

    public SourceLocation Location => Syntax.Location;

    private readonly BoundNodeFlags _flags;

    public bool HasAnyErrors
    {
        get
        {
            if (HasErrors || Syntax is { HasErrors: true })
            {
                return true;
            }

            return this is BoundExpression { Type.IsErrorType: true };
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
