// @file AssemblyIdentity.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using Cysharp.Text;
using MagicArchive;

namespace Prism.Core.Metadata;

[Archivable]
[method: ArchivableConstructor]
public sealed partial record AssemblyIdentity(string Name, Version Version)
{
    private static readonly Version DefaultVersion = new(0, 0, 0, 0);

    public AssemblyIdentity(string name)
        : this(name, DefaultVersion) { }

    [ArchiveIgnore]
    public string FullName
    {
        get
        {
            if (field is not null)
                return field;

            const string separator = ", ";

            using var builder = ZString.CreateStringBuilder();
            builder.Append(Name);
            builder.Append(separator);
            builder.Append("Version=");
            builder.Append(Version.ToString(4));

            Interlocked.CompareExchange(ref field, builder.ToString(), null);

            return field;
        }
    }

    public bool Equals(AssemblyIdentity? other)
    {
        return other is not null && Name == other.Name && Version == other.Version;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Version);
    }
}
