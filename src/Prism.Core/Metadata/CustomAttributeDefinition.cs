// @file CustomAttributeDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Semantic;
using Prism.Core.Utils;

namespace Prism.Core.Metadata;

internal sealed class CustomAttributeDefinition(
    TypeHandle type,
    BlobHandle arguments,
    MetadataReader reader
)
{
    public TypeDescriptor Type
    {
        get
        {
            if (field.Value is not null)
                return field;

            UnionInterlocked.Initialize(ref field, reader.GetTypeDescriptor(type));
            return field;
        }
    }

    public ImmutableArray<ConstantValue> Arguments
    {
        get
        {
            if (!field.IsDefault)
                return field;

            if (arguments.IsNone)
            {
                ImmutableInterlocked.InterlockedInitialize(ref field, []);
                return field;
            }

            var argHandles = reader.GetBlob<ImmutableArray<ConstantHandle>>(arguments);
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. argHandles.Select(reader.GetConstant)]
            );
            return field;
        }
    }
}
