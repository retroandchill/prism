// @file Rows.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Reflection;
using MagicArchive;

namespace Prism.Core.Metadata;

[Archivable]
internal readonly partial record struct VersionData(int Major, int Minor, int Build, int Revision)
{
    public VersionData(Version version)
        : this(version.Major, version.Minor, version.Build, version.Revision) { }

    public static implicit operator VersionData(Version version) => new(version);
}

[Archivable]
internal readonly partial record struct AssemblyIdentityData(
    StringHandle Name,
    VersionData Version
);

[Archivable]
internal readonly partial record struct NamespaceDefRow(
    StringHandle Name,
    NamespaceDefHandle Parent
);

[Archivable]
internal readonly partial record struct NamespaceRefRow(
    StringHandle Name,
    NamespaceRefHandle Parent
);

[Archivable]
internal readonly partial record struct TypeDefinitionRow(
    NamedTypeAttributes Attributes,
    StringHandle Name,
    MemberContainerDefHandle Container,
    NamedTypeHandle Base = default
);

[Archivable]
internal readonly partial record struct TypeReferenceRow(
    StringHandle Name,
    MemberContainerRefHandle Container
);

[Archivable]
[ArchivableUnion(0, typeof(ArrayTypeRow))]
[ArchivableUnion(1, typeof(ReferenceTypeRow))]
[ArchivableUnion(2, typeof(NullableTypeRow))]
internal closed partial record TypeSpecRow;

[Archivable]
internal sealed partial record ArrayTypeRow(TypeHandle ElementType, ulong? Rank) : TypeSpecRow;

[Archivable]
internal sealed partial record ReferenceTypeRow(TypeHandle ElementType, bool IsMutable)
    : TypeSpecRow;

[Archivable]
internal sealed partial record NullableTypeRow(TypeHandle ElementType) : TypeSpecRow;

[Archivable]
internal readonly partial record struct VariableRow(
    VariableAttributes Attributes,
    StringHandle Name,
    TypeHandle Type,
    MemberContainerDefHandle Container,
    ConstantHandle ConstantValue
);
