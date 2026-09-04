// @file MirModule.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Prism.Core.Mir;

namespace Prism.Core.Mir;

internal sealed class MirModule
{
    public MirModule(
        string name,
        ImmutableArray<MirGlobal> globals,
        ImmutableArray<MirFunction> functions,
        MirFunctionId? moduleInitializer = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Globals = globals.IsDefault ? [] : globals;
        Functions = functions.IsDefault ? [] : functions;
        Name = name;
        ModuleInitializer = moduleInitializer;

        ValidateUniqueIds(Globals, Functions, ModuleInitializer);
    }

    public string Name { get; }

    public ImmutableArray<MirGlobal> Globals { get; }

    public ImmutableArray<MirFunction> Functions { get; }

    public MirFunctionId? ModuleInitializer { get; }

    public MirGlobal GetGlobal(MirGlobalId id)
    {
        return Globals.FirstOrDefault(g => g.Id == id)
            ?? throw new KeyNotFoundException($"No MIR global exists with id '{id.Value}'.");
    }

    public MirFunction GetFunction(MirFunctionId id)
    {
        return Functions.FirstOrDefault(f => f.Id == id)
            ?? throw new KeyNotFoundException($"No MIR function exists with id '{id.Value}'.");
    }

    public bool TryGetGlobal(MirGlobalId id, [NotNullWhen(true)] out MirGlobal? global)
    {
        global = Globals.FirstOrDefault(g => g.Id == id);
        return global is not null;
    }

    public bool TryGetFunction(MirFunctionId id, [NotNullWhen(true)] out MirFunction? function)
    {
        function = Functions.FirstOrDefault(f => f.Id == id);
        return function is not null;
    }

    private static void ValidateUniqueIds(
        ImmutableArray<MirGlobal> globals,
        ImmutableArray<MirFunction> functions,
        MirFunctionId? moduleInitializer
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

        var functionIds = new HashSet<MirFunctionId>();
        foreach (var function in functions)
        {
            if (!functionIds.Add(function.Id))
            {
                throw new ArgumentException(
                    $"Duplicate MIR function id '{function.Id.Value}'.",
                    nameof(functions)
                );
            }
        }

        if (moduleInitializer is not null && !functionIds.Contains(moduleInitializer.Value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(moduleInitializer),
                "Module initializer must refer to a function contained in the module."
            );
        }
    }
}
