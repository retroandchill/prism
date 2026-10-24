// @file MetadataHandleGenerator.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Prism.SourceGenerator;

[Generator]
public sealed class MetadataHandleGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Prism.Core.Metadata.HandleAttribute",
            (n, _) => n is RecordDeclarationSyntax,
            (ctx, _) =>
            {
                var symbol = ctx.TargetSymbol as INamedTypeSymbol;
                Debug.Assert(symbol is not null);
                return symbol!;
            }
        );

        context.RegisterSourceOutput(provider, Execute);
    }

    private static void Execute(SourceProductionContext context, INamedTypeSymbol symbol)
    {
        using var writer = new CodeWriter();

        writer.WriteLine("// Generated file, do not edit");
        writer.WriteLine("#nullable enable");
        writer.WriteLine("using System.Numerics;");
        writer.WriteLine("using System.Diagnostics;");
        writer.WriteLine("using System.Runtime.CompilerServices;");
        writer.WriteLine("using MagicArchive;");
        writer.WriteLine("using MagicArchive.Formatters;");
        writer.WriteLine("using MagicArchive.Utilities;");
        writer.WriteLine();
        writer.WriteLine("namespace Prism.Core.Metadata;");

        writer.WriteLine();
        var name = symbol.Name.EndsWith("Handle") ? symbol.Name.Replace("Handle", "") : symbol.Name;
        GenerateHandleRecord(writer, name);

        context.AddSource($"{name}Handle.g.cs", writer.ToString());
    }

    private static void GenerateHandleRecord(CodeWriter writer, string name)
    {
        writer.WriteLine(
            $"readonly partial record struct {name}Handle "
                + $": IEqualityOperators<{name}Handle, {name}Handle, bool>"
        );
        using var scope = writer.EnterBlockScope();
        writer.WriteLine($"private const TokenType Type = TokenType.{name};");
        writer.WriteLine();

        writer.WriteLine($"private partial {name}Handle(int id)");
        using (writer.EnterBlockScope())
        {
            writer.WriteLine("Debug.Assert(MetadataToken.IsValidId(id));");
            writer.WriteLine("_id = id;");
        }

        writer.WriteLine();
        writer.WriteLine($"internal static {name}Handle FromRowId(int id) => new(id);");

        writer.WriteLine();
        writer.WriteLine(
            $"public static implicit operator MetadataToken({name}Handle handle) => new(Type, (uint)handle._id);"
        );

        writer.WriteLine();
        writer.WriteLine($"public static explicit operator {name}Handle(MetadataToken token)");
        using (writer.EnterBlockScope())
        {
            writer.WriteLine("if (token.Type != Type)");
            using (writer.EnterBlockScope())
            {
                writer.WriteLine("throw new InvalidCastException();");
            }
            writer.WriteLine();

            writer.WriteLine($"return new {name}Handle((int)token.Id);");
        }
    }
}
