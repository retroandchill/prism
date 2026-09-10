using Prism.Core.Configuration;
using Prism.Core.Semantic;

namespace Prism.Core.Symbols;

public abstract class TypeSymbol : MemberContainerSymbol
{
    private protected TypeSymbol(
        string name,
        Symbol? containingSymbol = null,
        SpecialType specialType = SpecialType.None
    )
        : base(name, containingSymbol)
    {
        SpecialType = specialType;
    }

    public SpecialType SpecialType { get; }

    public bool IsErrorType { get; private protected init; }

    public bool IsVoid => SpecialType == SpecialType.Void;

    public abstract bool IsDynamicallySized { get; }

    public abstract ulong GetSizeInBytes(CompilationSettings settings);
}
