// @file BasicBlock.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using JetBrains.Annotations;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Mir;

public readonly record struct BasicBlockId(uint Value);

public sealed partial class BasicBlock
{
    public BasicBlockId Id { get; }

    public string Name { get; }

    public ImmutableArray<Statement> Statements { get; }

    public Terminator Terminator { get; }

    private BasicBlock(
        BasicBlockId id,
        string name,
        ImmutableArray<Statement> statements,
        Terminator terminator
    )
    {
        Id = id;
        Name = name;
        Statements = statements;
        Terminator = terminator;
    }

    [Mapper(UseReferenceHandling = true, RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    internal static partial class Mapper
    {
        public static partial BasicBlock Map(
            BasicBlockBuilder builder,
            [ReferenceHandler] IReferenceHandler referenceHandler
        );

        [MapDerivedType<ReturnTerminatorBuilder, ReturnTerminator>]
        [MapDerivedType<CallTerminatorBuilder, CallTerminator>]
        [MapDerivedType<BranchTerminatorBuilder, BranchTerminator>]
        [MapDerivedType<UnreachableTerminatorBuilder, UnreachableTerminator>]
        private static partial Terminator Map(
            TerminatorBuilder builder,
            [ReferenceHandler] IReferenceHandler referenceHandler
        );

#pragma warning disable IDE0060
        private static ReturnTerminator Map([UsedImplicitly] ReturnTerminatorBuilder builder)
        {
            return ReturnTerminator.Instance;
        }

        private static partial CallTerminator Map(
            CallTerminatorBuilder builder,
            [ReferenceHandler] IReferenceHandler referenceHandler
        );

        private static partial BranchTerminator Map(
            BranchTerminatorBuilder builder,
            [ReferenceHandler] IReferenceHandler referenceHandler
        );

        private static UnreachableTerminator Map(
            [UsedImplicitly] UnreachableTerminatorBuilder builder
        )
        {
            return UnreachableTerminator.Instance;
        }
#pragma warning restore IDE0060
    }
}
