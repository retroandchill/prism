// @file MemberContainerSymbolData.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Prism.Compiler.Binding;
using Prism.Compiler.Declarations;
using Prism.Diagnostics;
using Prism.Semantic;
using Prism.Syntax;
using Prism.Utils;
using ZLinq;

namespace Prism.Compiler.Symbols.Source;

internal abstract class SourceMemberContainerSymbol(Compilation declaringCompilation)
    : SourceSymbol(declaringCompilation),
        IMemberContainerSymbolData,
        ICompletableSymbolData
{
    private SymbolCompletionState _completionState;
    private ImmutableArray<Symbol> _members;
    private ImmutableDictionary<string, ImmutableArray<Symbol>>? _nameToMembersMap;

    protected ref SymbolCompletionState CompletionState => ref _completionState;

    public new MemberContainerSymbol PublicSymbol
    {
        get => Unsafe.As<MemberContainerSymbol>(base.PublicSymbol);
        protected init => base.PublicSymbol = value;
    }

    public ImmutableArray<Symbol> GetMembers()
    {
        if (!_members.IsDefault)
            return _members;

        ImmutableInterlocked.InterlockedCompareExchange(ref _members, ComputeMembers(), default);
        return _members;
    }

    public ImmutableArray<Symbol> GetMembers(string name)
    {
        return GetNameToMembersMap().GetValueOrDefault(name, []);
    }

    private ImmutableArray<Symbol> ComputeMembers()
    {
        var members = GetNameToMembersMap()
            .Values.AsValueEnumerable()
            .SelectMany(v => v.AsValueEnumerable())
            .ToArray();
        Array.Sort(members, SymbolLocationComparer.Get(DeclaringCompilation));
        return ImmutableCollectionsMarshal.AsImmutableArray(members);
    }

    protected ImmutableDictionary<string, ImmutableArray<Symbol>> GetNameToMembersMap()
    {
        if (_nameToMembersMap is not null)
            return _nameToMembersMap;

        using var context = BindingContext.Create();
        if (
            Interlocked.CompareExchange(ref _nameToMembersMap, MakeNameToMembersMap(context), null)
            is not null
        )
            return _nameToMembersMap;

        AddDeclarationDiagnostics(context);
        _completionState.MarkPartComplete(CompletionPart.Members);
        return _nameToMembersMap;
    }

    protected abstract ImmutableDictionary<string, ImmutableArray<Symbol>> MakeNameToMembersMap(
        BindingContext context
    );

    private protected static void ValidateMembers(
        string name,
        ImmutableArray<Symbol> members,
        BindingContext context
    )
    {
        if (members.Length == 1)
            return;

        Dictionary<OverloadSignature, List<Symbol>>? overloads = null;
        foreach (var member in members)
        {
            if (member is not FunctionSymbol function)
            {
                break;
            }

            overloads ??= new Dictionary<OverloadSignature, List<Symbol>>();
            var signature = OverloadSignature.Create(function);
            var list = overloads.GetOrAdd(signature, () => []);
            list.Add(function);
        }

        if (overloads is null)
        {
            ReportDuplicateDefinitions(name, members.AsSpan(), context);
        }
        else
        {
            foreach (var (_, functions) in overloads)
            {
                if (functions.Count == 1)
                    continue;

                ReportDuplicateDefinitions(name, CollectionsMarshal.AsSpan(functions), context);
            }
        }
    }

    private static void ReportDuplicateDefinitions(
        string name,
        ReadOnlySpan<Symbol> members,
        BindingContext context
    )
    {
        var primaryMemberLocations = members[0].Locations.AsSpan();
        var primaryLocation = primaryMemberLocations[0];
        context.ReportDiagnostic(
            Diagnostic.DuplicateDeclaration(
                primaryLocation,
                [
                    .. primaryMemberLocations[1..],
                    .. members[1..]
                        .AsValueEnumerable()
                        .SelectMany(x => x.Locations.AsValueEnumerable()),
                ],
                name
            )
        );
    }

    public abstract void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    );

    public bool IsComplete(CompletionPart part)
    {
        return _completionState.IsComplete(part);
    }
}
