// @file MetadataHandleGenerator.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Prism.Core.Metadata;
using Prism.SourceGenerator.Models.Attributes;
using ZLinq;

namespace Prism.SourceGenerator;

[Generator]
public sealed class MetadataHandleGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var singleHandleProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            typeof(HandleAttribute).FullName!,
            (n, _) => n is RecordDeclarationSyntax,
            (ctx, _) =>
            {
                var symbol = ctx.TargetSymbol as INamedTypeSymbol;
                Debug.Assert(symbol is not null);
                return symbol!;
            }
        );

        context.RegisterSourceOutput(singleHandleProvider, ProduceSingleHandle);

        var compositeHandleProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            typeof(CompositeHandleAttribute).FullName!,
            (n, _) => n is RecordDeclarationSyntax,
            (ctx, _) =>
            {
                var symbol = ctx.TargetSymbol as INamedTypeSymbol;
                Debug.Assert(symbol is not null);
                return symbol!;
            }
        );

        context.RegisterSourceOutput(compositeHandleProvider, ProduceCompositeHandle);
    }

    private static void ProduceSingleHandle(
        SourceProductionContext context,
        INamedTypeSymbol symbol
    )
    {
        using var writer = new CodeWriter();

        GenerateMetadataHeader(writer);

        writer.WriteLine();
        var name = symbol.Name.EndsWith("Handle") ? symbol.Name.Replace("Handle", "") : symbol.Name;
        GenerateSingleHandleRecord(writer, name);

        context.AddSource($"{name}Handle.g.cs", writer.ToString());
    }

    private static void GenerateMetadataHeader(CodeWriter writer)
    {
        writer.WriteLine("// Generated file, do not edit");
        writer.WriteLine("#nullable enable");
        writer.WriteLine("using System.Numerics;");
        writer.WriteLine("using System.Diagnostics;");
        writer.WriteLine("using System.Runtime.CompilerServices;");
        writer.WriteLine();
        writer.WriteLine("namespace Prism.Core.Metadata;");
    }

    private static void GenerateSingleHandleRecord(CodeWriter writer, string name)
    {
        writer.WriteLine(
            $"readonly partial record struct {name}Handle "
                + $": IEqualityOperators<{name}Handle, {name}Handle, bool>, ITableRowHandle<{name}Handle>"
        );
        using var scope = writer.EnterBlockScope();
        writer.WriteLine($"internal const TokenType Type = TokenType.{name};");
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
            $"static {name}Handle ITableRowHandle<{name}Handle>.Create(int id) => new(id);"
        );

        writer.WriteLine();
        writer.WriteLine($"public static {name}Handle None => new(0);");

        writer.WriteLine();
        writer.WriteLine("public bool IsNone => _id == 0;");

        writer.WriteLine();
        writer.WriteLine(
            $"public static implicit operator MetadataToken({name}Handle handle) => new(Type, (uint)handle._id);"
        );

        writer.WriteLine();
        writer.WriteLine($"public static explicit operator {name}Handle(MetadataToken token)");
        using (writer.EnterBlockScope())
        {
            writer.WriteLine("if (token.IsNone)");
            using (writer.EnterBlockScope())
            {
                writer.WriteLine("return None;");
            }
            writer.WriteLine();

            writer.WriteLine("if (token.Type != Type)");
            using (writer.EnterBlockScope())
            {
                writer.WriteLine("throw new InvalidCastException();");
            }
            writer.WriteLine();

            writer.WriteLine($"return new {name}Handle((int)token.Id);");
        }
    }

    private static void ProduceCompositeHandle(
        SourceProductionContext context,
        INamedTypeSymbol symbol
    )
    {
        var types = symbol.GetCompositeHandleAttributeInfo().Types;

        using var writer = new CodeWriter();

        GenerateMetadataHeader(writer);

        var name = symbol.Name;
        GenerateCompositeHandleRecord(writer, name, types);

        context.AddSource($"{name}.g.cs", writer.ToString());
    }

    private static void GenerateCompositeHandleRecord(
        CodeWriter writer,
        string name,
        ImmutableArray<ITypeSymbol> types
    )
    {
        writer.WriteLine("[Union]");
        writer.WriteLine(
            $"readonly partial record struct {name} "
                + $": IEqualityOperators<{name}, {name}, bool>, IUnion"
        );
        using var scope = writer.EnterBlockScope();

        writer.WriteLine($"private partial {name}(MetadataToken token)");
        using (writer.EnterBlockScope())
        {
            writer.Write("Debug.Assert(token.Type is ");
            foreach (var (i, type) in types.AsValueEnumerable().Index())
            {
                if (i > 0)
                    writer.Write(" or ");
                writer.Write($"{type.Name}.Type");
            }
            writer.WriteLine(");");
            writer.WriteLine("_token = token;");
        }

        foreach (var type in types)
        {
            writer.WriteLine($"public {name}({type.Name} handle)");
            using (writer.EnterBlockScope())
            {
                writer.WriteLine("_token = !handle.IsNone ? handle : MetadataToken.None;");
            }
            writer.WriteLine();
        }

        writer.WriteLine($"public static {name} None => new(MetadataToken.None);");
        writer.WriteLine();

        writer.WriteLine("public bool IsNone => _token.IsNone;");
        writer.WriteLine();

        writer.WriteLine("public bool HasValue => !IsNone;");
        writer.WriteLine();

        writer.WriteLine("public object? Value");
        using (writer.EnterBlockScope())
        {
            writer.WriteLine("get");
            using var getterScope = writer.EnterBlockScope();
            writer.WriteLine("return _token.Type switch");
            using var switchScope = writer.EnterBlockScope(true);
            foreach (var type in types)
            {
                writer.WriteLine(
                    $"{type.Name}.Type => {type.Name}.FromRowId(unchecked((int)_token.Id)),"
                );
            }
            writer.WriteLine("_ => null");
        }

        writer.WriteLine();
        writer.WriteLine(
            $"public static implicit operator MetadataToken({name} handle) => handle._token;"
        );

        writer.WriteLine();
        writer.WriteLine($"public static explicit operator {name}(MetadataToken token)");
        using (writer.EnterBlockScope())
        {
            writer.WriteLine("if (token.IsNone)");
            using (writer.EnterBlockScope())
            {
                writer.WriteLine("return None;");
            }
            writer.WriteLine();

            writer.Write("if (token.Type is ");
            foreach (var (i, type) in types.AsValueEnumerable().Index())
            {
                if (i > 0)
                    writer.Write(" and ");
                writer.Write($"not {type.Name}.Type");
            }
            writer.WriteLine(')');
            using (writer.EnterBlockScope())
            {
                writer.WriteLine("throw new InvalidCastException();");
            }
            writer.WriteLine();
            writer.WriteLine($"return new {name}(token);");
        }
        writer.WriteLine();

        foreach (var type in types)
        {
            writer.WriteLine($"public static explicit operator {type.Name}({name} handle)");
            using (writer.EnterBlockScope())
            {
                writer.WriteLine($"return ({type.Name}) handle._token;");
            }
            writer.WriteLine();
        }
        foreach (var (i, type) in types.AsValueEnumerable().Index())
        {
            if (i > 0)
                writer.WriteLine();

            writer.WriteLine($"public bool TryGetValue(out {type.Name} value)");
            using var blockScope = writer.EnterBlockScope();
            writer.WriteLine($"if (_token.Type != {type.Name}.Type && !_token.IsNone)");
            using (writer.EnterBlockScope())
            {
                writer.WriteLine("value = default;");
                writer.WriteLine("return false;");
            }

            writer.WriteLine();
            writer.WriteLine($"value = {type.Name}.FromRowId(unchecked((int)_token.Id));");
            writer.WriteLine("return true;");
        }
    }
}
