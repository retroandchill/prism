using NetEscapades.EnumGenerators;

namespace Prism.Semantic.Symbols;

[Flags]
[EnumExtensions]
internal enum CompletionPart : uint
{
    None = 0,

    // common
    Attributes = 1 << 0,
    Parameters = 1 << 1,
    Type = 1 << 2,
    TypeParameters = 1 << 3,

    // explicit staged graph work
    BaseTypeStarted = 1 << 5,
    BaseTypeCompleted = 1 << 6,

    // optional later if/when needed
    InterfacesStarted = 1 << 7,
    InterfacesCompleted = 1 << 8,

    // single-shot published/computed phases
    EnumUnderlyingType = 1 << 9,
    Members = 1 << 10,
    ConstantValue = 1 << 11,
    StartChecks = 1 << 12,
    FinishChecks = 1 << 13,

    // terminal bit
    MembersCompleted = 1 << 14,

    All = (1 << 15) - 1,

    AssemblyAll = Members,
    NamespaceAll = Members | StartChecks | FinishChecks | MembersCompleted,
    VariableAll = Attributes | Type | ConstantValue,
    FunctionAll = Attributes | Parameters | Type | StartChecks | FinishChecks,
    ParameterAll = Attributes | Type | ConstantValue,
    TypeAll =
        Attributes | EnumUnderlyingType | Members | StartChecks | FinishChecks | MembersCompleted,
}
