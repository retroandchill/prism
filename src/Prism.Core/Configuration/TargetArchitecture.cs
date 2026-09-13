// @file TargetArchitecture.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Prism.Core.Configuration;

public enum TargetArchitecture : byte
{
    X86,
    X64,
    Arm,
    Arm64,
}

public static class TargetArchitectureExtensions
{
    extension(TargetArchitecture architecture)
    {
        public static TargetArchitecture CurrentPlatform
        {
            get
            {
                return RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X86 => TargetArchitecture.X86,
                    Architecture.X64 => TargetArchitecture.X64,
                    Architecture.Arm => TargetArchitecture.Arm,
                    Architecture.Arm64 => TargetArchitecture.Arm64,
                    _ => throw new NotSupportedException("Unsupported architecture"),
                };
            }
        }

        public PointerWidth PointerWidth
        {
            get
            {
                return architecture switch
                {
                    TargetArchitecture.X86 or TargetArchitecture.Arm => PointerWidth.X32,
                    TargetArchitecture.X64 or TargetArchitecture.Arm64 => PointerWidth.X64,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(architecture),
                        architecture,
                        null
                    ),
                };
            }
        }

        internal string ArchitectureFolder
        {
            get
            {
                return architecture switch
                {
                    TargetArchitecture.X86 => "x86",
                    TargetArchitecture.X64 => "x64",
                    TargetArchitecture.Arm64 => "arm64",
                    _ => throw new NotSupportedException("Unsupported architecture"),
                };
            }
        }
    }
}
