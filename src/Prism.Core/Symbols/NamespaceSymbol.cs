using Prism.Core.Compiling;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Symbols;

public enum NamespaceKind
{
    Assembly,
    Compilation,
}

public abstract class NamespaceSymbol : MemberContainerSymbol
{
    private protected NamespaceSymbol(string name, Symbol? containingSymbol)
        : base(name, containingSymbol) { }

    public sealed override TypeSymbol? ContainingType => null;

    public bool IsGlobal => Name == CommonNames.Global;

    public sealed override bool IsImplicitlyDeclared => IsGlobal;

    public abstract NamespaceKind NamespaceKind { get; }

    public abstract Compilation? ContainingCompilation { get; }

    public NamespaceSymbol? GetNestedNamespace(string name)
    {
        return GetMembers(name).AsValueEnumerable().OfType<NamespaceSymbol>().FirstOrDefault();
    }

    public sealed override void WriteDisplayString(TextWriter writer)
    {
        if (ContainingNamespace is { IsGlobal: false } owner)
        {
            owner.WriteDisplayString(writer);
            writer.Write("::");
        }

        writer.Write(Name);
    }
}
