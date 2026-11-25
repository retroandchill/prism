// @file FunctionReferenceBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Metadata.Symbols;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Metadata.Builders;

public sealed class FunctionReferenceBuilder
{
    internal FunctionReferenceBuilder(
        FunctionRefHandle handle,
        string name,
        MemberContainerReference parent
    )
    {
        Handle = handle;
        Name = name;
        Parent = parent;
    }

    public FunctionRefHandle Handle { get; }

    public string Name { get; }

    public MemberContainerReference Parent { get; }

    public ImmutableArray<TypeDescriptorBuilder> Parameters { get; set; } = [];

    internal FunctionReference ToReference(IReferenceHandler handler)
    {
        return FunctionReference.Mapper.ToReference(this, handler);
    }
}
