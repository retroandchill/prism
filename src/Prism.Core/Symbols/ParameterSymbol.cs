using System.Diagnostics.CodeAnalysis;
using Prism.Core.Semantic;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols;

public closed record ParameterDefault(SyntaxNode? Syntax);

public sealed record ConstantParameterDefault(ConstantValue Value, SyntaxNode? Syntax = null)
    : ParameterDefault(Syntax);

public closed class ParameterSymbol : ValueSymbol
{
    private protected ParameterSymbol(string name, Symbol? containingSymbol)
        : base(name, containingSymbol) { }

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
