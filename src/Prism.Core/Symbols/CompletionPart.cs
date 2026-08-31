using NetEscapades.EnumGenerators;

namespace Prism.Core.Symbols;

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
    BaseTypesStarted = 1 << 5,
    BaseTypesCompleted = 1 << 6,

    // optional later if/when needed
    InterfacesStarted = 1 << 7,
    InterfacesCompleted = 1 << 8,

    // single-shot published/computed phases
    Members = 1 << 9,
    ConstantValue = 1 << 10,
    StartChecks = 1 << 11,
    FinishChecks = 1 << 12,

    // terminal bit
    MembersCompleted = 1 << 13,

    All = (1 << 14) - 1,

    AssemblyAll = Members,
    NamespaceAll = Members | MembersCompleted,
    VariableAll = Type | ConstantValue,
    FunctionAll = Parameters | Type | StartChecks | FinishChecks,
    ParameterAll = Type,
}
