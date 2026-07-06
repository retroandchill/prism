using Prism.Core.Ast;

namespace Prism.Core.Semantic;

public enum ConversionKind
{
    None,
    Identity,
    Implicit,
    Explicit,
}

public readonly record struct ConversionInfo(
    ConversionKind Kind,
    BuiltInType Source,
    BuiltInType Target
)
{
    public bool Exists => Kind != ConversionKind.None;
    public bool IsImplicit => Kind is ConversionKind.Identity or ConversionKind.Implicit;
}
