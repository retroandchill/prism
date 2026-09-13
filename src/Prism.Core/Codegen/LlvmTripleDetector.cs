// @file LlvmTripleDetector.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Cysharp.Text;
using Prism.Core.Configuration;

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
            var env = settings.Toolchain.ToLlvmEnvString(settings.OperatingSystem);
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
                TargetArchitecture.X86 => "i386",
                TargetArchitecture.X64 => "x86_64",
                TargetArchitecture.Arm => "arm",
                TargetArchitecture.Arm64 => "aarch64",
                _ => throw new InvalidOperationException("Unknown target architecutre"),
            };
        }
    }

    private static string ToLlvmOsString(this TargetOperatingSystem os)
    {
        return os switch
        {
            TargetOperatingSystem.Linux => "linux",
            TargetOperatingSystem.Windows => "windows",
            TargetOperatingSystem.MacOS => "darwin",
            TargetOperatingSystem.Freestanding => "none",
            _ => throw new ArgumentOutOfRangeException(nameof(os), os, null),
        };
    }

    private static string ToLlvmEnvString(
        this ToolchainKind env,
        TargetOperatingSystem operatingSystem
    )
    {
        return env switch
        {
            ToolchainKind.Auto => operatingSystem switch
            {
                TargetOperatingSystem.Linux => "gnu",
                TargetOperatingSystem.Windows => "msvc",
                TargetOperatingSystem.MacOS => "unknown",
                TargetOperatingSystem.Freestanding => "unknown",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(operatingSystem),
                    operatingSystem,
                    null
                ),
            },
            ToolchainKind.Gnu or ToolchainKind.Clang => "gnu",
            ToolchainKind.Msvc => "msvc",
            _ => throw new ArgumentOutOfRangeException(nameof(env), env, null),
        };
    }
}
