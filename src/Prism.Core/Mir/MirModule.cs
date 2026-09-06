// @file MirModule.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Prism.Core.Symbols;

namespace Prism.Core.Mir;

internal sealed class MirModule
{
    public MirModule(
        string name,
        ImmutableArray<MirGlobal> globals,
        ImmutableArray<MirFunction> functions,
        FunctionSymbol? moduleInitializer = null,
        FunctionSymbol? entryPoint = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Globals = globals.IsDefault ? [] : globals;
        Functions = functions.IsDefault ? [] : functions;
        Name = name;
        ModuleInitializer = moduleInitializer;
        EntryPoint = entryPoint;

        ValidateUniqueIds(Globals, Functions, ModuleInitializer, EntryPoint);
    }

    public string Name { get; }

    public ImmutableArray<MirGlobal> Globals { get; }

    public ImmutableArray<MirFunction> Functions { get; }

    public FunctionSymbol? ModuleInitializer { get; }

    public FunctionSymbol? EntryPoint { get; }

    public MirGlobal GetGlobal(MirGlobalId id)
    {
        return Globals.FirstOrDefault(g => g.Id == id)
            ?? throw new KeyNotFoundException($"No MIR global exists with id '{id.Value}'.");
    }

    public MirFunction GetFunction(FunctionSymbol symbol)
    {
        return Functions.FirstOrDefault(f => f.Symbol == symbol)
            ?? throw new KeyNotFoundException($"No MIR function exists for {symbol}.");
    }

    public bool TryGetGlobal(MirGlobalId id, [NotNullWhen(true)] out MirGlobal? global)
    {
        global = Globals.FirstOrDefault(g => g.Id == id);
        return global is not null;
    }

    public bool TryGetFunction(FunctionSymbol symbol, [NotNullWhen(true)] out MirFunction? function)
    {
        function = Functions.FirstOrDefault(f => f.Symbol == symbol);
        return function is not null;
    }

    private static void ValidateUniqueIds(
        ImmutableArray<MirGlobal> globals,
        ImmutableArray<MirFunction> functions,
        FunctionSymbol? moduleInitializer,
        FunctionSymbol? entryPoint
    )
    {
        var globalIds = new HashSet<MirGlobalId>();
        foreach (var global in globals)
        {
            if (!globalIds.Add(global.Id))
            {
                throw new ArgumentException(
                    $"Duplicate MIR global id '{global.Id.Value}'.",
                    nameof(globals)
                );
            }
        }

        var functionIds = new HashSet<FunctionSymbol>(ReferenceEqualityComparer.Instance);
        foreach (var function in functions)
        {
            if (!functionIds.Add(function.Symbol))
            {
                throw new ArgumentException(
                    $"Duplicate MIR function id '{function.Symbol}'.",
                    nameof(functions)
                );
            }
        }

        if (moduleInitializer is not null && !functionIds.Contains(moduleInitializer))
        {
            throw new ArgumentOutOfRangeException(
                nameof(moduleInitializer),
                "Module initializer must refer to a function contained in the module."
            );
        }

        if (entryPoint is not null && !functionIds.Contains(entryPoint))
        {
            throw new ArgumentOutOfRangeException(
                nameof(entryPoint),
                "Entry point must refer to a function contained in the module."
            );
        }
    }
}
