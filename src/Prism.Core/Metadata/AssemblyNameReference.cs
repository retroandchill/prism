// @file AssemblyNameReference.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Cysharp.Text;
using Prism.Core.Configuration;
using Semver;

namespace Prism.Core.Metadata;

public class AssemblyNameReference : IMetadataTokenProvider
{
    private static readonly Version DefaultVersion = new(0, 0, 0, 0);

    public string Name
    {
        get;
        set
        {
            field = value;
            _fullName = null;
        }
    }

    public Version Version { get; set; }

    public TargetArchitecture TargetArchitecture { get; set; }
    public TargetOperatingSystem OperatingSystem { get; set; }
    public ToolchainKind Toolchain { get; set; }

    private string? _fullName;
    public string FullName
    {
        get
        {
            if (_fullName is not null)
                return _fullName;

            const string separator = ", ";

            using var builder = ZString.CreateStringBuilder();
            builder.Append(Name);
            builder.Append(separator);
            builder.Append("Version=");
            builder.Append(Version.ToString(4));
            builder.Append(separator);
            builder.Append("Arch=");
            builder.Append(TargetArchitecture.ToStringFast());
            builder.Append(separator);
            builder.Append("OS=");
            builder.Append(OperatingSystem.ToStringFast());
            builder.Append(separator);
            builder.Append("Toolchain=");
            builder.Append(Toolchain.ToStringFast());

            Interlocked.CompareExchange(ref _fullName, builder.ToString(), null);

            return _fullName;
        }
    }

    public MetadataToken MetadataToken { get; set; }

    public AssemblyNameReference(string name, Version version)
    {
        Name = name;
        Version = version;
        TargetArchitecture = TargetArchitecture.CurrentPlatform;
        OperatingSystem = TargetOperatingSystem.CurrentPlatform;
        Toolchain = ToolchainKind.Detect(OperatingSystem);
        MetadataToken = new MetadataToken(TokenType.AssemblyRef);
    }

    public override string ToString() => FullName;
}
