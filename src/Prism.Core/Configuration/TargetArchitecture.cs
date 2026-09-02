// @file TargetArchitecture.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Prism.Core.Configuration;

public enum TargetArchitecture : byte
{
    X86,
    Arm,
    Riscv,
    Wasm,
    Unknown,
}

public static class TargetArchitectureExtensions
{
    extension(TargetArchitecture)
    {
        public static TargetArchitecture CurrentPlatform
        {
            get
            {
                return RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X86 or Architecture.X64 => TargetArchitecture.X86,
                    Architecture.Arm or Architecture.Arm64 => TargetArchitecture.Arm,
                    Architecture.RiscV64 => TargetArchitecture.Riscv,
                    Architecture.Wasm => TargetArchitecture.Wasm,
                    _ => TargetArchitecture.Unknown,
                };
            }
        }
    }
}
