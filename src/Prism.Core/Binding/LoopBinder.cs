using System.Diagnostics;
using Prism.Core.Symbols;

namespace Prism.Core.Binding;

internal abstract class LoopBinder(Binder next, LabelSymbol loopLabel) : LocalScopeBinder(next)
{
    public override LabelSymbol? LookupLoopLabel(string name, LookupContext context)
    {
        Debug.Assert(Next is not null);
        return string.IsNullOrEmpty(name) || name.Equals(loopLabel.Name, StringComparison.Ordinal)
            ? loopLabel
            : Next.LookupLoopLabel(name, context);
    }
}
