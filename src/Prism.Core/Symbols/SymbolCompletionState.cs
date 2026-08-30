using System.Diagnostics;

namespace Prism.Core.Symbols;

internal struct SymbolCompletionState
{
    private volatile uint _completeParts;

    private uint IncompleteParts => ~_completeParts & CompletionPart.All.AsUnderlyingType();

    public bool IsComplete(CompletionPart part)
    {
        return (_completeParts & part.AsUnderlyingType()) != part.AsUnderlyingType();
    }

    public bool MarkPartComplete(CompletionPart part)
    {
        uint oldState;
        uint newState;
        do
        {
            oldState = _completeParts;
            newState = oldState | part.AsUnderlyingType();
            if (newState == oldState)
                return false;
        } while (Interlocked.CompareExchange(ref _completeParts, newState, oldState) != oldState);

        return true;
    }

    public CompletionPart NextIncompletePart
    {
        get
        {
            var incomplete = IncompleteParts;
            var next = incomplete & ~(incomplete - 1);
            Debug.Assert(HasAtMostOneSetBit(next), "Expected at most one incomplete part.");
            return (CompletionPart)next;
        }
    }

    public void WaitPartComplete(CompletionPart part, CancellationToken cancellationToken)
    {
        if (IsComplete(part))
        {
            return;
        }

        var spinWait = new SpinWait();
        while (!IsComplete(part))
        {
            cancellationToken.ThrowIfCancellationRequested();
            spinWait.SpinOnce();
        }
    }

    private static bool HasAtMostOneSetBit(uint bits)
    {
        return (bits & (bits - 1)) == 0;
    }
}
