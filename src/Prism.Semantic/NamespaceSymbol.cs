// @file NamespaceSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using ZLinq;

namespace Prism.Semantic;

public sealed class NamespaceSymbol : MemberContainerSymbol
{
    internal NamespaceSymbol(INamespaceSymbolData data)
        : base(data) { }

    internal new INamespaceSymbolData Data => Unsafe.As<INamespaceSymbolData>(base.Data);

    public bool IsGlobal => Name == CommonNames.Global;

    public NamespaceKind NamespaceKind => Data.NamespaceKind;

    public NamespaceSymbol? GetNestedNamespace(string name)
    {
        return GetMembers(name).AsValueEnumerable().OfType<NamespaceSymbol>().FirstOrDefault();
    }

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        if (ContainingNamespace is { IsGlobal: false } owner)
        {
            owner.WriteDisplayString(ref builder);
            builder.Append("::");
        }

        builder.Append(Name);
    }
}
