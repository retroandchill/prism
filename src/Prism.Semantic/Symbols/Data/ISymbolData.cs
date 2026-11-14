// @file ISymbolData.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Semantic.Symbols.Data;

internal interface ISymbolData
{
    string Name { get; }

    string MetadataName => Name;

    Symbol? ContainingSymbol { get; }

    AssemblySymbol? ContainingAssembly
    {
        get
        {
            var containing = ContainingSymbol;
            var container = containing as AssemblySymbol;
            return ReferenceEquals(containing, container)
                ? container
                : containing?.ContainingAssembly;
        }
    }

    NamespaceSymbol? ContainingNamespace
    {
        get
        {
            var containing = ContainingSymbol;
            var container = containing as NamespaceSymbol;
            return ReferenceEquals(containing, container)
                ? container
                : containing?.ContainingNamespace;
        }
    }

    TypeSymbol? ContainingType
    {
        get
        {
            var containing = ContainingSymbol;
            var container = containing as TypeSymbol;
            return ReferenceEquals(containing, container) ? container : containing?.ContainingType;
        }
    }

    MemberContainerSymbol? ContainingMemberContainer
    {
        get
        {
            var containing = ContainingSymbol;
            var container = containing as MemberContainerSymbol;
            return ReferenceEquals(containing, container)
                ? container
                : containing?.ContainingMemberContainer;
        }
    }
}
