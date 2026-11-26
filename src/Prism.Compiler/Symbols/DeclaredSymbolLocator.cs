using System.Collections.Concurrent;
using System.Diagnostics;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Error;

namespace Prism.Compiler.Symbols;

internal sealed class DeclaredSymbolLocator(AssemblySymbol assembly)
{
    private readonly ConcurrentDictionary<SpecialType, NamedTypeSymbol> _namedTypesBySpecialType =
        new();

    public NamedTypeSymbol GetDeclaredSpecialType(SpecialType specialType)
    {
        return _namedTypesBySpecialType.GetOrAdd(
            specialType,
            static (specialType, @this) =>
            {
                var type = @this.GetTypeByMetadataName(specialType.MetadataName);
                return type ?? new ErrorTypeSymbol(specialType.BuiltInTypeName).PublicSymbol;
            },
            this
        );
    }

    public NamedTypeSymbol? GetTypeByMetadataName(ReadOnlySpan<char> metadataName)
    {
        var ns = assembly.GlobalNamespace;

        var separatorIndex = metadataName.IndexOf("::");
        while (separatorIndex >= 0)
        {
            var name = metadataName[..separatorIndex];
            metadataName = metadataName[(separatorIndex + 2)..];
            ns = ns.GetNestedNamespace(name);

            if (ns is null)
                return null;

            separatorIndex = metadataName.IndexOf("::");
        }

        NamedTypeSymbol? typeSymbol = null;
        foreach (var range in metadataName.Split('+'))
        {
            var name = metadataName[range];
            typeSymbol = ns.GetMembers(name).OfType<NamedTypeSymbol>().SingleOrDefault();
            if (typeSymbol is null)
                return null;
        }

        Debug.Assert(typeSymbol is not null);
        return typeSymbol;
    }
}
