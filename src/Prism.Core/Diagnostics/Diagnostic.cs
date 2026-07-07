// @file Diagnostic.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Parse;

namespace Prism.Core.Diagnostics;

public abstract class Diagnostic
{
    public abstract DiagnosticDescriptor Descriptor { get; }
    public abstract SourceRange Range { get; }

    public abstract string GetMessage(IFormatProvider? provider = null);

    public override string ToString()
    {
        return $"{Descriptor.Id}: {GetMessage()}";
    }
}
