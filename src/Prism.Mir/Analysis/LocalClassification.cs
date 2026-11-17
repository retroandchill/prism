// @file LocalClassifier.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Frozen;
using Prism.Abi;
using Prism.Semantic.Layout;
using Prism.Semantic.Symbols;
using Singulink.Enums;

namespace Prism.Mir.Analysis;

public enum LocalStorageKind : byte
{
    SsaRegister,
    StackSlot,
    IndirectParameter,
}

public readonly record struct LocalClassification
{
    private const int MaxRegisterSlotSize = 16;

    public LocalStorageKind StorageKind { get; private init; }

    public bool IsStoredInMemory =>
        StorageKind is LocalStorageKind.StackSlot or LocalStorageKind.IndirectParameter;

    public static FrozenDictionary<Local, LocalClassification> GetClassifications<T>(
        T compilation,
        Function mirFunction,
        CancellationToken cancellationToken
    )
        where T : class, IAbiLookup, ILayoutSource
    {
        var localFlow = FunctionAnalyzer.AnalyzeLocalFlow(
            mirFunction,
            compilation,
            cancellationToken
        );

        return localFlow.ToFrozenDictionary(
            pair => pair.Key,
            pair => ClassifyLocal(compilation, pair.Key, pair.Value, cancellationToken)
        );
    }

    private static LocalClassification ClassifyLocal(
        ILayoutSource compilation,
        Local local,
        LocalUsageInfo info,
        CancellationToken cancellationToken
    )
    {
        if (local.Type is ArrayTypeSymbol)
        {
            return new LocalClassification { StorageKind = LocalStorageKind.StackSlot };
        }

        var layout = compilation.GetTypeLayout(local.Type, cancellationToken);
        if (
            layout.Size > MaxRegisterSlotSize
            || info.IsAddressed
            || info.WriteCount > 1
            || info.Projection.HasAnyFlag(ProjectionUsage.ReadWrite)
        )
        {
            return new LocalClassification { StorageKind = LocalStorageKind.StackSlot };
        }

        return new LocalClassification { StorageKind = LocalStorageKind.SsaRegister };
    }
}
