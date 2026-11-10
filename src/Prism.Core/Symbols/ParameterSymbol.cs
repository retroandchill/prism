using System.Diagnostics.CodeAnalysis;
using Prism.Core.Semantic;
using Prism.Syntax;
using ConstantValue = Prism.Constants.ConstantValue;

namespace Prism.Core.Symbols;

public closed record ParameterDefault(SyntaxNode? Syntax);

public sealed record ConstantParameterDefault(ConstantValue Value, SyntaxNode? Syntax = null)
    : ParameterDefault(Syntax);

public closed class ParameterSymbol : ValueSymbol
{
    public abstract FunctionSymbol? ContainingFunction { get; }

    [MemberNotNullWhen(true, nameof(DefaultValue))]
    public bool HasDefaultValue => DefaultValue is not null;

    public abstract ParameterDefault? DefaultValue { get; }

    public sealed override void WriteDisplayString(TextWriter writer)
    {
        writer.Write(Name);
        writer.Write(": ");
        Type.WriteDisplayString(writer);
    }
}
