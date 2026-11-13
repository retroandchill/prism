// @file Symbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using Prism.Text;

namespace Prism.Semantic;

public closed class Symbol : IEquatable<Symbol>
{
    private protected Symbol(ISymbolData data)
    {
        Data = data;
    }

    internal ISymbolData Data { get; }

    public string Name => Data.Name;

    public string MetadataName => Data.MetadataName;

    public DeclaredVisibility DeclaredVisibility =>
        Data is IVisibilityGuardedSymbolData declaredData
            ? declaredData.DeclaredVisibility
            : DeclaredVisibility.NotApplicable;

    public ImmutableArray<Location> Locations =>
        Data is ILocatableSymbolData locatableData ? locatableData.Locations : [];

    public Location FirstLocation()
    {
        return TryGetFirstLocation()
            ?? throw new InvalidOperationException("Symbol has no locations");
    }

    public Location? TryGetFirstLocation()
    {
        return Locations.Length > 0 ? Locations[0] : null;
    }

    public Location FirstLocationOrNone()
    {
        return TryGetFirstLocation() ?? Location.None;
    }

    public bool IsImplicitlyDeclared => Data is IDeclaredSymbolData { IsImplicitlyDeclared: true };

    public Symbol? ContainingSymbol => Data.ContainingSymbol;

    public AssemblySymbol? ContainingAssembly => Data.ContainingAssembly;

    public NamespaceSymbol? ContainingNamespace => Data.ContainingNamespace;

    public TypeSymbol? ContainingType => Data.ContainingType;

    public MemberContainerSymbol? ContainingMemberContainer => Data.ContainingMemberContainer;

    public ImmutableArray<AttributeData> GetAttributes()
    {
        return Data is IAttributeHolderSymbolData attributeHolderData
            ? attributeHolderData.GetAttributes()
            : [];
    }

    public bool IsDefinedInSourceTree(ISourceDocument tree, TextSpan? definedWithin)
    {
        return Data is IDeclaredSymbolData declaredSymbolData
            && declaredSymbolData.IsDefinedInSourceTree(tree, definedWithin);
    }

    internal bool NeedsCompletion => Data is ICompletableSymbolData;

    internal void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    )
    {
        if (Data is ICompletableSymbolData completableSymbolData)
        {
            completableSymbolData.ForceComplete(location, filter, cancellationToken);
        }
    }

    internal bool IsComplete(CompletionPart part)
    {
        return Data is not ICompletableSymbolData completableSymbolData
            || completableSymbolData.IsComplete(part);
    }

    public abstract void WriteDisplayString(ref Utf16ValueStringBuilder builder);

    public string ToDisplayString()
    {
        var builder = ZString.CreateStringBuilder();
        try
        {
            WriteDisplayString(ref builder);
            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    public override string ToString() => ToDisplayString();

    public override bool Equals(object? obj)
    {
        return Equals(obj as Symbol);
    }

    public bool Equals(Symbol? other)
    {
        return other is not null && Equals(other, SymbolCompareKind.ConsiderEverything);
    }

    public virtual bool Equals(Symbol other, SymbolCompareKind kind)
    {
        return ReferenceEquals(this, other);
    }

    public override int GetHashCode()
    {
        return GetHashCode(SymbolCompareKind.ConsiderEverything);
    }

    public virtual int GetHashCode(SymbolCompareKind kind)
    {
        return RuntimeHelpers.GetHashCode(this);
    }

    public static bool operator ==(Symbol? left, Symbol? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        return left is not null && left.Equals(right);
    }

    public static bool operator !=(Symbol? left, Symbol? right)
    {
        return !(left == right);
    }
}
