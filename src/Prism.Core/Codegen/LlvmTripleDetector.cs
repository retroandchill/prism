// @file LlvmTripleDetector.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Cysharp.Text;
using Prism.Core.Configuration;
using OperatingSystem = Prism.Core.Configuration.OperatingSystem;

namespace Prism.Core.Codegen;

public static class LlvmTripleDetector
{
    extension(CompilationSettings settings)
    {
        public string GetLlvmTriple()
        {
            using var builder = ZString.CreateStringBuilder();

            builder.Append(settings.ToLlvmArchString());
            builder.Append("-unknown-");
            builder.Append(settings.OperatingSystem.ToLlvmOsString());
            var env = settings.Environment.ToLlvmEnvString();
            if (!string.IsNullOrEmpty(env))
            {
                builder.Append('-');
                builder.Append(env);
            }

            return builder.ToString();
        }

        private string ToLlvmArchString()
        {
            return settings.Architecture switch
            {
                TargetArchitecture.X86 => settings.PointerWidth == PointerWidth.X64
                    ? "x86_64"
                    : "i386",
                TargetArchitecture.Arm => settings.PointerWidth == PointerWidth.X64
                    ? "aarch64"
                    : "arm",
                TargetArchitecture.Riscv => settings.PointerWidth == PointerWidth.X64
                    ? "riscv64"
                    : "riscv32",
                TargetArchitecture.Wasm => settings.PointerWidth == PointerWidth.X64
                    ? "wasm64"
                    : "wasm32",
                TargetArchitecture.Unknown => "unknown",
                _ => throw new InvalidOperationException("Unknown target architecutre"),
            };
        }
    }

    private static string ToLlvmOsString(this OperatingSystem os)
    {
        return os switch
        {
            OperatingSystem.Linux => "linux",
            OperatingSystem.Windows => "windows",
            OperatingSystem.MacOS => "darwin",
            OperatingSystem.Freestanding => "none",
            _ => throw new ArgumentOutOfRangeException(nameof(os), os, null),
        };
    }

    private static string ToLlvmEnvString(this RuntimeEnvironment env)
    {
        return env switch
        {
            RuntimeEnvironment.Gnu => "gnu",
            RuntimeEnvironment.Musl => "musl",
            RuntimeEnvironment.Msvc => "msvc",
            RuntimeEnvironment.None => "",
            _ => throw new ArgumentOutOfRangeException(nameof(env), env, null),
        };
    }
}
