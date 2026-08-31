namespace Prism.Core.Symbols;

public abstract class AssemblySymbol : Symbol
{
    private protected AssemblySymbol(string name)
        : base(name) { }

    public sealed override AssemblySymbol? ContainingAssembly => null;

    public sealed override NamespaceSymbol? ContainingNamespace => null;

    public sealed override TypeSymbol? ContainingType => null;

    public abstract NamespaceSymbol GlobalNamespace { get; }

    public sealed override void WriteDisplayString(TextWriter writer)
    {
        writer.Write(Name);
    }
}
