// @file AssemblyIdentity.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Cysharp.Text;
using MagicArchive;

namespace Prism;

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
        if (ReferenceEquals(this, other))
            return true;

        return other is not null && Name == other.Name && Version == other.Version;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Version);
    }

    public override string ToString()
    {
        return FullName;
    }

    public static AssemblyIdentity Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(string s, [NotNullWhen(true)] out AssemblyIdentity? result)
    {
        var commaIndex = s.IndexOf(',');
        if (commaIndex == -1)
        {
            result = new AssemblyIdentity(s);
            return true;
        }

        var nameSlice = s.AsSpan(0, commaIndex);
        var remaining = s.AsSpan(commaIndex + 1).TrimStart();
        var version = DefaultVersion;

        while (!remaining.IsEmpty)
        {
            var equalIndex = remaining.IndexOf('=');
            if (equalIndex == -1)
            {
                result = null;
                return false;
            }

            var name = remaining[..equalIndex];
            remaining = remaining[(equalIndex + 1)..].TrimStart();
            if (name.Equals("Version", StringComparison.OrdinalIgnoreCase))
            {
                commaIndex = remaining.IndexOf(',');
                var endIndex = commaIndex != -1 ? commaIndex : remaining.Length;
                var textSlice = remaining[..commaIndex];
                if (!Version.TryParse(textSlice, out version))
                {
                    result = null;
                    return false;
                }

                remaining = remaining[(endIndex + 1)..].TrimStart();
            }
            else
            {
                result = null;
                return false;
            }
        }

        result = new AssemblyIdentity(nameSlice.ToString(), version);
        return true;
    }
}
