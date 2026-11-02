using Prism.Core.Metadata;

namespace Prism.Core.Symbols;

public closed class AssemblySymbol : Symbol
{
    private protected AssemblySymbol(string name, TargetInfo targetInfo)
        : base(name)
    {
        TargetInfo = targetInfo;
    }

    public abstract AssemblyIdentity Identity { get; }

    public TargetInfo TargetInfo { get; }

    public sealed override AssemblySymbol? ContainingAssembly => null;

    public sealed override NamespaceSymbol? ContainingNamespace => null;

    public sealed override TypeSymbol? ContainingType => null;

    public abstract NamespaceSymbol GlobalNamespace { get; }

    public sealed override void WriteDisplayString(TextWriter writer)
    {
        writer.Write(Name);
    }
}
