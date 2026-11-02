namespace Prism.Core.Symbols;

[Flags]
public enum TypeCompareKind : uint
{
    ConsiderEverything = 0,
    IgnoreMutability = 1 << 0,
}
