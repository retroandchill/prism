// @file AssemblyDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using MagicArchive;
using MagicArchive.Utilities;
using NetEscapades.EnumGenerators;
using ZLinq;

namespace Prism.Core.Metadata;

public sealed class AssemblyDefinition : AssemblyDescriptor, IDisposable
{
    public TargetInfo Target { get; }

    private readonly MetadataReader _reader;

    internal AssemblyDefinition(AssemblyIdentity identity, TargetInfo info, MetadataReader reader)
        : base(identity, new MetadataToken(TokenType.AssemblyDef, 1))
    {
        Target = info;
        _reader = reader;
    }

    public ImmutableArray<AssemblyReference> ReferencedAssemblies
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                _reader.ReadReferencedAssemblies()
            );
            return field;
        }
        private init;
    }

    public void Dispose()
    {
        _reader.Dispose();
    }
}
