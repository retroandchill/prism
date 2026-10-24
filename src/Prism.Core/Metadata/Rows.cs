// @file Rows.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
