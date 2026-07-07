// @file ResolutionContext.cs
// 
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Cysharp.Text;
using Prism.Core.Semantic.Symbols;

namespace Prism.Core.Semantic;

internal enum ResolutionKind
{
    ValueType,
    Initializer,
    FunctionSignature,
    FunctionBody,
}

internal readonly record struct ResolutionKey(Symbol Symbol, ResolutionKind Kind);

internal sealed class ResolutionContext
{
    public static ResolutionContext Empty { get; } = new([], []);
    
    private readonly ImmutableHashSet<ResolutionKey> _active;
    public ImmutableArray<ResolutionKey> Stack { get; }

    private ResolutionContext(
        ImmutableHashSet<ResolutionKey> active,
        ImmutableArray<ResolutionKey> stack
    )
    {
        _active = active;
        Stack = stack;
    }
    
    public bool Contains(ResolutionKey key) => _active.Contains(key);
    
    public ResolutionContext Push(ResolutionKey key)
    {
        return new ResolutionContext(_active.Add(key), Stack.Add(key));
    }

    public string CollectCycle(ResolutionKey key)
    {
        if (Stack.IsEmpty)
            throw new InvalidOperationException("No cycle detected");


        using var builder = ZString.CreateStringBuilder();
        var foundStart = false;
        foreach (var item in Stack)
        {
            if (foundStart || ReferenceEquals(item.Symbol, key.Symbol) && item.Kind == key.Kind)
            {
                builder.Append(item.Symbol.Name);
                builder.Append(" -> ");
                foundStart = true;
            }
        }
        
        if (!foundStart)
            throw new InvalidOperationException("No cycle detected");
        
        builder.Append(key.Symbol.Name);
        return builder.ToString();
    }
}