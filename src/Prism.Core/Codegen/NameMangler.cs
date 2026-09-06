// @file NameMangler.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Cysharp.Text;
using Prism.Core.Symbols;

namespace Prism.Core.Codegen;

public static class NameMangler
{
    public static string Mangle(this VariableSymbol variable)
    {
        var builder = ZString.CreateStringBuilder();

        try
        {
            builder.Append('$');
            if (variable.ContainingAssembly is { Name: var assemblyName })
            {
                builder.Append(assemblyName);
            }

            builder.Append("_V_");
            MangleContainerPath(variable, ref builder);
            MangleName(variable.Name, ref builder);
            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    public static string Mangle(this FunctionSymbol function)
    {
        var builder = ZString.CreateStringBuilder();

        try
        {
            builder.Append('$');
            if (function.ContainingAssembly is { Name: var assemblyName })
            {
                builder.Append(assemblyName);
            }
            builder.Append("_F_");
            MangleContainerPath(function, ref builder);
            MangleName(function.Name, ref builder);
            builder.Append("E_");
            foreach (var param in function.Parameters)
            {
                MangleType(param.Type, ref builder);
                builder.Append('_');
            }
            builder.Append("R_");
            MangleType(function.ReturnType, ref builder);
            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    private static void MangleType(TypeSymbol type, ref Utf16ValueStringBuilder builder)
    {
        switch (type.SpecialType)
        {
            case SpecialType.None:
                MangleContainerPath(type, ref builder);
                MangleName(type.Name, ref builder);
                break;
            case SpecialType.Void:
                builder.Append('V');
                break;
            case SpecialType.Bool:
                builder.Append('B');
                break;
            case SpecialType.I8:
                builder.Append("I8");
                break;
            case SpecialType.I16:
                builder.Append("I16");
                break;
            case SpecialType.I32:
                builder.Append("I32");
                break;
            case SpecialType.I64:
                builder.Append("I64");
                break;
            case SpecialType.I128:
                builder.Append("I128");
                break;
            case SpecialType.ISize:
                builder.Append("IZ");
                break;
            case SpecialType.U8:
                builder.Append("U8");
                break;
            case SpecialType.U16:
                builder.Append("U16");
                break;
            case SpecialType.U32:
                builder.Append("U32");
                break;
            case SpecialType.U64:
                builder.Append("U64");
                break;
            case SpecialType.U128:
                builder.Append("U128");
                break;
            case SpecialType.USize:
                builder.Append("UZ");
                break;
            case SpecialType.F32:
                builder.Append("F32");
                break;
            case SpecialType.F64:
                builder.Append("F64");
                break;
            case SpecialType.Char:
                builder.Append('C');
                break;
            case SpecialType.Char16:
                builder.Append("C16");
                break;
            case SpecialType.Rune:
                builder.Append('R');
                break;
            case SpecialType.Str:
                builder.Append('S');
                break;
            default:
                throw new ArgumentException("Invalid special type");
        }
    }

    private static void MangleContainerPath(Symbol symbol, ref Utf16ValueStringBuilder builder)
    {
        builder.Append('N');
        var names = new List<string>();
        var current = symbol.ContainingSymbol;
        while (current is not null and not AssemblySymbol)
        {
            names.Add(current.Name);
            current = current.ContainingSymbol;
        }

        foreach (var n in names)
        {
            MangleName(n, ref builder);
        }
    }

    private static void MangleName(string name, ref Utf16ValueStringBuilder builder)
    {
        builder.Append(name.Length);
        builder.Append(name);
    }
}
