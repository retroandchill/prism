// @file CSharpEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Prism.SyntaxGenerator.Models.CSharp;
using Prism.SyntaxGenerator.Models.Resolved;
using Prism.SyntaxGenerator.Models.Spec;
using Prism.SyntaxGenerator.Output;
using ZLinq;
using static Prism.SyntaxGenerator.Metadata.CommonNames;

namespace Prism.SyntaxGenerator.Emitters;

public static class CSharpEmitter
{
    extension(CodeWriter writer)
    {
        #region Syntax Kinds

        public void EmitSyntaxKinds(CSharpSyntaxModel model)
        {
            writer.WriteLine("// Generated file, do not edit");
            writer.WriteLine("using NetEscapades.EnumGenerators;");
            writer.WriteLine("using Prism.Core.Text;");
            writer.WriteLine();

            writer.WriteLine("namespace Prism.Core.Syntax;");
            writer.WriteLine();

            writer.WriteLine($"[EnumExtensions]");
            writer.WriteLine($"public enum {SyntaxKindClass} : ushort");
            using (writer.EnterBlockScope())
            {
                writer.WriteLine("None = 0,");
                writer.WriteLine("List = 1,");

                foreach (var group in model.KindGroups)
                {
                    writer.WriteLine();
                    writer.WriteLineUnindented($"#region {group.DisplayName}");
                    foreach (var kind in group.SyntaxKinds)
                    {
                        writer.WriteLine($"{kind.CSharpName} = {kind.Value},");
                    }

                    writer.WriteLine("#endregion");
                }
            }

            writer.WriteLine();
            writer.WriteLine($"public static partial class {SyntaxKindClass}Extensions");

            using (writer.EnterBlockScope())
            {
                writer.WriteLine($"extension ({SyntaxKindClass} kind)");
                using var extensionScope = writer.EnterBlockScope();
                writer.WriteLine("public string DisplayText");
                using (writer.EnterBlockScope())
                {
                    writer.WriteLine("get");
                    using var getterScope = writer.EnterBlockScope();
                    writer.WriteLine("return kind switch");
                    using var switchScope = writer.EnterBlockScope(true);
                    foreach (var kind in model.Kinds)
                    {
                        writer.WriteLine(
                            $"{SyntaxKindClass}.{kind.CSharpName} => \"{kind.DisplayName}\","
                        );
                    }

                    writer.WriteLine(
                        $"_ => throw new ArgumentException(\"Unknown {SyntaxKindClass}\", nameof(kind))"
                    );
                }

                foreach (var group in model.KindGroups)
                {
                    var groupName = group.CSharpName;

                    writer.WriteLine();
                    writer.WriteLine(
                        $"public static {SyntaxKindClass} {groupName}Start => ({SyntaxKindClass}){group.StartValue};"
                    );
                    writer.WriteLine(
                        $"public static {SyntaxKindClass} {groupName}End => ({SyntaxKindClass}){group.EndValue};"
                    );

                    writer.WriteLine();
                    writer.WriteLine(
                        $"public bool Is{groupName} => (ushort)kind >= {group.StartValue} && (ushort)kind <= {group.EndValue};"
                    );
                }

                writer.WriteLine();
                writer.EmitIsSyntaxCategory(
                    model,
                    "StructuredTrivia",
                    k => k.Kind == SyntaxGroupKind.StructuredTrivia
                );
                writer.WriteLine();
                writer.EmitIsSyntaxCategory(model, "Token", k => k.Kind == SyntaxGroupKind.Token);
                writer.WriteLine();
                writer.EmitIsSyntaxCategory(model, "Node", k => k.Kind == SyntaxGroupKind.Node);

                writer.WriteLine();
                writer.EmitKeywordLookup(model);
                writer.WriteLine();
                writer.EmitPunctuationTrie(model);
            }
        }

        private void EmitIsSyntaxCategory(
            CSharpSyntaxModel model,
            string groupName,
            Func<CSharpGroup, bool> predicate
        )
        {
            writer.Write($"public bool Is{groupName} => ");
            var itemWritten = false;
            foreach (var kind in model.KindGroups.AsValueEnumerable().Where(predicate))
            {
                if (itemWritten)
                {
                    writer.Write(" || ");
                }

                writer.Write($"kind.Is{kind.CSharpName}");
                itemWritten = true;
            }

            if (!itemWritten)
            {
                writer.Write("false");
            }

            writer.WriteLine(';');
        }

        private void EmitKeywordLookup(CSharpSyntaxModel model)
        {
            writer.WriteLine(
                "internal static SyntaxKind? MatchKeyword(scoped ReadOnlySpan<char> text)"
            );
            using var scope = writer.EnterBlockScope();
            writer.WriteLine("switch (text.Length)");
            using (writer.EnterBlockScope())
            {
                foreach (
                    var sizeClass in model
                        .Tokens.AsValueEnumerable()
                        .Where(t => t.Category == TokenCategory.Keyword)
                        .GroupBy(t => t.Text!.Length)
                        .OrderBy(t => t.Key)
                )
                {
                    writer.WriteLine($"case {sizeClass.Key}:");
                    using var indent = writer.EnterIndentationScope();
                    foreach (var keyword in sizeClass.AsValueEnumerable().OrderBy(k => k.Text))
                    {
                        writer.WriteLine(
                            $"if (text.Equals(\"{keyword.Text!}\", StringComparison.Ordinal)) return {SyntaxKindClass}.{keyword.Kind.CSharpName};"
                        );
                    }

                    writer.WriteLine("break;");
                }
            }

            writer.WriteLine("return null;");
        }

        private void EmitPunctuationTrie(CSharpSyntaxModel model)
        {
            var trie = ConstructPunctuationTrie(model);
            writer.WriteLine(
                $"internal static {SyntaxKindClass}? MatchPunctuation(TextCursor cursor)"
            );
            using var block = writer.EnterBlockScope();
            writer.WriteTrie(trie);
            writer.WriteLine();
            writer.WriteLine("return null;");
        }

        private void WriteTrie(TrieNode node)
        {
            switch (node.Children.Count)
            {
                case 1:
                {
                    var (character, child) = node.Children.Single();
                    writer.WriteLine($"if (cursor.Current == '{character}')");
                    using var block = writer.EnterBlockScope();
                    writer.WriteLine("cursor.Advance();");
                    writer.WriteTrie(child);
                    break;
                }
                case > 1:
                {
                    writer.WriteLine("switch (cursor.Current)");
                    using var block = writer.EnterBlockScope();
                    // Sort by the key to ensure consistent output
                    foreach (var (character, child) in node.Children.OrderBy(x => x.Key))
                    {
                        writer.WriteLine($"case '{character}':");
                        using var unmarkedBlock = writer.EnterIndentationScope();
                        writer.WriteLine("cursor.Advance();");
                        writer.WriteTrie(child);
                        if (child.Value is null)
                            writer.WriteLine("break;");
                    }

                    break;
                }
            }

            if (node.Value is { } terminal)
            {
                writer.WriteLine($"return SyntaxKind.{terminal.Kind.CSharpName};");
            }
        }

        #endregion

        #region Green Nodes

        public void EmitGreenNodeClass(CSharpModule module)
        {
            writer.WriteLine("// Generated file, do not edit");
            writer.WriteLine("using System.Collections.Immutable;");
            writer.WriteLine("using Prism.Core.Diagnostics;");
            writer.WriteLine();

            writer.WriteLine("namespace Prism.Core.Syntax.Green;");
            writer.WriteLine();

            foreach (var (i, node) in module.Nodes.AsValueEnumerable().Index())
            {
                if (i > 0)
                    writer.WriteLine();

                writer.EmitGreenNodeClass(node);
            }
        }

        private void EmitGreenNodeClass(CSharpNode node)
        {
            var qualifier = node.IsAbstract ? "abstract " : "sealed";
            var baseName =
                node.Base?.GreenClassName
                ?? node.Module.Kind switch
                {
                    ModuleKind.Node => GreenNodeClass,
                    ModuleKind.StructuredTrivia => GreenStructuredTriviaClass,
                    _ => throw new InvalidOperationException("Invalid module kind"),
                };

            writer.WriteLine($"internal {qualifier} class {node.GreenClassName} : {baseName}");
            using var blockScope = writer.EnterBlockScope();
            if (node.IsAbstract)
            {
                writer.EmitGreenNodeAbstractClassBody(node);
            }
            else
            {
                writer.EmitGreenNodeConcreteClassBody(node);
            }
        }

        private void EmitGreenNodeAbstractClassBody(CSharpNode node)
        {
            writer.WriteLine(
                $"protected {node.GreenClassName}({SyntaxKindClass} kind) : " + $"base(kind) {{ }}"
            );

            foreach (var property in node.Properties)
            {
                writer.WriteLine();
                writer.Write("public abstract ");
                writer.EmitGreenPropertyType(property);
                writer.WriteLine($" {property.PropertyName} {{ get; }}");
                writer.WriteLine();
                writer.Write($"public abstract {node.GreenClassName} With{property.PropertyName}(");
                writer.EmitGreenPropertyType(property);
                writer.WriteLine(" value);");
            }
        }

        private void EmitGreenNodeConcreteClassBody(CSharpNode node)
        {
            writer.Write($"public {node.GreenClassName}(");
            foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
            {
                if (i > 0)
                    writer.Write(", ");

                writer.EmitGreenPropertyType(property);
                writer.Write($" {property.ParameterName}");
            }

            writer.WriteLine($") : base({SyntaxKindClass}.{node.Kind!.CSharpName})");
            using (writer.EnterBlockScope())
            {
                foreach (var property in node.Properties)
                {
                    writer.WriteLine($"{property.PropertyName} = {property.ParameterName};");
                }
            }

            foreach (var property in node.Properties)
            {
                writer.WriteLine();
                var @override = property.IsOverride ? " override" : "";
                writer.Write($"public{@override} ");
                writer.EmitGreenPropertyType(property);
                writer.Write($" {property.PropertyName} {{ get; }}");
            }

            writer.WriteLine();
            writer.EmitGreenGetSlotMethod(node);

            writer.WriteLine();
            writer.EmitGreenCreateRedMethod(node);

            writer.EmitGreenMutationMethods(node);
        }

        private void EmitGreenPropertyType(CSharpProperty property)
        {
            switch (property.Shape)
            {
                case PropertyShape.Single:
                    writer.Write(property.Type.GreenClassName);
                    break;
                case PropertyShape.Optional:
                    writer.Write($"{property.Type.GreenClassName}?");
                    break;
                case PropertyShape.List:
                    writer.Write($"GreenSyntaxList<{property.Type.GreenClassName}>");
                    break;
                case PropertyShape.SeparatedList:
                    writer.Write($"GreenSeparatedList<{property.Type.GreenClassName}>");
                    break;
                default:
                    throw new InvalidOperationException("Unknown shape");
            }
        }

        private void EmitGreenGetSlotMethod(CSharpNode node)
        {
            writer.WriteLine($"public override {GreenNodeClass}? GetSlot(int index)");
            using var scope = writer.EnterBlockScope();
            if (node.Properties.Length == 1)
            {
                var prop = node.Properties[0];
                var isList = prop.Shape is PropertyShape.List or PropertyShape.SeparatedList;
                var nodeSuffix = isList ? ".Node" : "";

                writer.WriteLine($"return index == 0 ? {prop.PropertyName}{nodeSuffix} : null;");
                return;
            }

            writer.WriteLine("return index switch");
            using var switchScope = writer.EnterBlockScope(true);
            foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
            {
                var isList = property.Shape is PropertyShape.List or PropertyShape.SeparatedList;
                var nodeSuffix = isList ? ".Node" : "";
                writer.WriteLine($"{i} => {property.PropertyName}{nodeSuffix},");
            }

            writer.WriteLine("_ => null");
        }

        private void EmitGreenCreateRedMethod(CSharpNode node)
        {
            writer.WriteLine(
                $"public override {SyntaxNodeClass} CreateRed("
                    + $"{SyntaxNodeClass}? parent = null, int position = 0)"
            );
            using var scope = writer.EnterBlockScope();
            writer.WriteLine($"return new {node.RedClassName}(this, parent, position);");
        }

        private void EmitGreenMutationMethods(CSharpNode node)
        {
            foreach (var property in node.Properties)
            {
                var @override = property.IsOverride ? " override" : "";
                writer.Write(
                    $"public{@override} {node.GreenClassName} " + $"With{property.PropertyName}("
                );
                writer.EmitGreenPropertyType(property);
                writer.Write($" {property.ParameterName})");
                using (writer.EnterBlockScope())
                {
                    writer.WriteLine($"if ({property.PropertyName} == {property.ParameterName})");
                    using (writer.EnterIndentationScope())
                    {
                        writer.WriteLine("return this;");
                    }

                    writer.WriteLine();
                    writer.Write($"return new {node.GreenClassName}(");
                    foreach (var (i, prop) in node.Properties.AsValueEnumerable().Index())
                    {
                        if (i > 0)
                            writer.Write(", ");

                        writer.Write(
                            ReferenceEquals(prop, property) ? prop.ParameterName : prop.PropertyName
                        );
                    }

                    writer.WriteLine(')');
                    using var initializer = writer.EnterBlockScope(true);
                    writer.WriteLine("Diagnostics = Diagnostics");
                }
            }

            writer.WriteLine();
            writer.WriteLine(
                $"public override {node.GreenClassName} WithDiagnostics("
                    + $"ImmutableArray<SyntaxDiagnosticInfo> diagnostics)"
            );
            using (writer.EnterBlockScope())
            {
                writer.WriteLine("if (Diagnostics == diagnostics)");
                using (writer.EnterIndentationScope())
                {
                    writer.WriteLine("return this;");
                }

                writer.WriteLine();
                writer.Write($"return new {node.GreenClassName}(");
                foreach (var (i, prop) in node.Properties.AsValueEnumerable().Index())
                {
                    if (i > 0)
                        writer.Write(", ");

                    writer.Write(prop.PropertyName);
                }

                writer.WriteLine(')');
                using var initializer = writer.EnterBlockScope(true);
                writer.WriteLine("Diagnostics = diagnostics");
            }

            writer.WriteLine();
            writer.Write($"public {node.GreenClassName} Update(");
            foreach (var (i, prop) in node.Properties.AsValueEnumerable().Index())
            {
                if (i > 0)
                    writer.WriteLine(", ");

                writer.EmitGreenPropertyType(prop);
                writer.Write($" {prop.ParameterName}");
            }

            writer.WriteLine(")");
            using (writer.EnterBlockScope())
            {
                writer.Write("if (");
                foreach (var (i, prop) in node.Properties.AsValueEnumerable().Index())
                {
                    if (i > 0)
                        writer.Write(" && ");

                    writer.Write($"{prop.PropertyName} == {prop.ParameterName}");
                }

                writer.WriteLine(')');
                using (writer.EnterBlockScope())
                {
                    writer.WriteLine("return this;");
                }

                writer.WriteLine();
                writer.Write($"return new {node.GreenClassName}(");
                foreach (var (i, prop) in node.Properties.AsValueEnumerable().Index())
                {
                    if (i > 0)
                        writer.Write(", ");

                    writer.Write(prop.ParameterName);
                }

                writer.WriteLine(')');
                using var initializer = writer.EnterBlockScope(true);
                writer.WriteLine("Diagnostics = Diagnostics");
            }
        }

        #endregion

        #region Red Nodes

        public void EmitRedNodeClass(CSharpModule module)
        {
            writer.WriteLine("// Generated file, do not edit");
            writer.WriteLine("using System.Collections.Immutable;");
            writer.WriteLine("using Prism.Core.Diagnostics;");
            writer.WriteLine("using Prism.Core.Syntax.Green;");
            writer.WriteLine();

            writer.WriteLine("namespace Prism.Core.Syntax;");
            writer.WriteLine();
            foreach (var (i, node) in module.Nodes.AsValueEnumerable().Index())
            {
                if (i > 0)
                    writer.WriteLine();

                writer.EmitRedNodeClass(node);
            }
        }

        private void EmitRedNodeClass(CSharpNode node)
        {
            var qualified = node.IsAbstract ? "abstract" : "sealed";
            var baseName =
                node.Base?.RedClassName
                ?? node.Module.Kind switch
                {
                    ModuleKind.Node => SyntaxNodeClass,
                    ModuleKind.StructuredTrivia => StructuredTriviaSyntaxClass,
                    _ => throw new InvalidOperationException("Invalid module kind"),
                };
            writer.WriteLine($"public {qualified} class {node.RedClassName} : {baseName}");
            using var scope = writer.EnterBlockScope(true);
            var constructorSpecifier = node.IsAbstract ? "private protected" : "internal";

            writer.WriteLine(
                $"{constructorSpecifier} {node.RedClassName}({node.GreenClassName} node, {SyntaxNodeClass}? parent, int position) : "
                    + $"base(node, parent, position) {{ }}"
            );
            writer.WriteLine();

            foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
            {
                if (!node.IsAbstract && property.Type.Name != "Token")
                {
                    var className = property.Shape is PropertyShape.Single or PropertyShape.Optional
                        ? property.Type.RedClassName
                        : SyntaxNodeClass;
                    writer.WriteLine($"private {className}? {property.FieldName};");
                }
                writer.EmitRedNodePropertyGetter(property, i);
            }

            if (node.IsAbstract)
                return;

            writer.WriteLine();
            writer.EmitGetNodeSlot(node);

            writer.WriteLine();
            writer.EmitGetCachedSlot(node);
        }

        private void EmitRedPropertyType(CSharpProperty property)
        {
            if (property.Type.Name == "Token")
            {
                switch (property.Shape)
                {
                    case PropertyShape.Single:
                        writer.Write(SyntaxTokenClass);
                        break;
                    case PropertyShape.Optional:
                        writer.Write($"{SyntaxTokenClass}?");
                        break;
                    case PropertyShape.List:
                        writer.Write("SyntaxTokenList");
                        break;
                    case PropertyShape.SeparatedList:
                        throw new InvalidOperationException(
                            "Cannot generate getter for separated list or tokens"
                        );
                    default:
                        throw new InvalidOperationException("Unknown shape");
                }
            }
            else
            {
                switch (property.Shape)
                {
                    case PropertyShape.Single:
                        writer.Write(property.Type.RedClassName);
                        break;
                    case PropertyShape.Optional:
                        writer.Write($"{property.Type.RedClassName}?");
                        break;
                    case PropertyShape.List:
                        writer.Write($"SyntaxList<{property.Type.RedClassName}>");
                        break;
                    case PropertyShape.SeparatedList:
                        writer.Write($"SeparatedSyntaxList<{property.Type.RedClassName}>");
                        break;
                    default:
                        throw new InvalidOperationException("Unknown shape");
                }
            }
        }

        private void EmitRedNodePropertyGetter(CSharpProperty property, int index)
        {
            var node = property.Owner;
            var @abstract = node.IsAbstract ? " abstract " : "";
            var @override = property.IsOverride ? " override" : "";
            writer.Write($"public{@abstract}{@override} ");
            writer.EmitRedPropertyType(property);
            writer.Write($" {property.PropertyName}");
            if (node.IsAbstract)
            {
                writer.WriteLine("{ get; }");
                return;
            }

            writer.WriteLine();
            using var propertyScope = writer.EnterBlockScope();
            writer.WriteLine("get");
            using var getterScope = writer.EnterBlockScope();

            if (property.Type.Name == "Token")
            {
                switch (property.Shape)
                {
                    case PropertyShape.Single:
                        writer.Write($"return new {SyntaxTokenClass}(");
                        writer.EmitInvokeGreenGetter(property);
                        writer.Write(", this, ");
                        if (index == 0)
                            writer.WriteLine("Position);");
                        else
                            writer.WriteLine($"GetSlotPosition({index}));");
                        break;
                    case PropertyShape.Optional:
                        writer.Write("var green = ");
                        writer.EmitInvokeGreenGetter(property);
                        writer.WriteLine(';');
                        writer.Write(
                            $"return green is not null ? new {SyntaxTokenClass}(green, this, "
                        );
                        if (index == 0)
                            writer.WriteLine("Position) : null;");
                        else
                            writer.WriteLine($"GetSlotPosition({index})) : null;");
                        break;
                    case PropertyShape.List:
                        writer.Write($"return new SyntaxTokenList(this, ");
                        writer.EmitInvokeGreenGetter(property);
                        if (index == 0)
                            writer.WriteLine(", Position);");
                        else
                            writer.WriteLine($", GetSlotPosition({index}));");
                        break;
                    case PropertyShape.SeparatedList:
                        throw new InvalidOperationException(
                            "Cannot have a separated list of tokens"
                        );
                    default:
                        throw new InvalidOperationException("Unknown shape");
                }
            }
            else
            {
                switch (property.Shape)
                {
                    case PropertyShape.Single or PropertyShape.Optional:
                        writer.Write($"return GetRed(ref {property.FieldName}");
                        if (index > 0)
                            writer.Write($", {index}");
                        writer.WriteLine(");");
                        break;
                    case PropertyShape.List:
                        writer.Write($"var red = GetRed(ref {property.FieldName}");
                        if (index > 0)
                            writer.Write($", {index}");
                        writer.WriteLine(");");
                        writer.WriteLine(
                            $"return new SyntaxList<{property.Type.RedClassName}>(red);"
                        );
                        break;
                    case PropertyShape.SeparatedList:
                        writer.Write($"var red = GetRed(ref {property.FieldName}");
                        if (index > 0)
                            writer.Write($", {index}");
                        writer.WriteLine(");");
                        writer.WriteLine(
                            $"return new SeparatedSyntaxList<{property.Type.RedClassName}>(red);"
                        );
                        break;
                    default:
                        throw new InvalidOperationException("Unknown shape");
                }
            }
            writer.WriteLine();
        }

        private void EmitGetNodeSlot(CSharpNode node)
        {
            using var targetNodes = node
                .Properties.AsValueEnumerable()
                .Index()
                .Where(p => p.Item.Type.Name != "Token")
                .ToArrayPool();
            var targetSpan = targetNodes.Span;
            writer.WriteLine($"internal override {SyntaxNodeClass}? GetNodeSlot(int index)");
            using var scope = writer.EnterBlockScope();
            switch (targetSpan.Length)
            {
                case 0:
                    writer.WriteLine("return null;");
                    break;
                case 1:
                {
                    var (i, property) = targetSpan[0];
                    writer.WriteLine(
                        $"return index == {i} ? GetRed(ref {property.FieldName}) : null;"
                    );
                    break;
                }
                default:
                {
                    writer.WriteLine("return index switch");
                    using var switchScope = writer.EnterBlockScope(true);
                    foreach (var (i, property) in targetNodes.Span)
                    {
                        writer.Write($"{i} => ");
                        if (i == 0)
                        {
                            writer.WriteLine($"GetRed(ref {property.FieldName}),");
                        }
                        else
                        {
                            writer.WriteLine($"GetRed(ref {property.FieldName}, {i}),");
                        }
                    }

                    writer.WriteLine("_ => null");
                    break;
                }
            }
        }

        private void EmitGetCachedSlot(CSharpNode node)
        {
            using var targetNodes = node
                .Properties.AsValueEnumerable()
                .Index()
                .Where(p => p.Item.Type.Name != "Token")
                .ToArrayPool();
            var targetSpan = targetNodes.Span;
            writer.WriteLine($"internal override {SyntaxNodeClass}? GetCachedSlot(int index)");
            using var scope = writer.EnterBlockScope();
            switch (targetSpan.Length)
            {
                case 0:
                    writer.WriteLine("return null;");
                    break;
                case 1:
                {
                    var (i, property) = targetSpan[0];
                    writer.WriteLine($"return index == {i} ? {property.FieldName} : null;");
                    break;
                }
                default:
                {
                    writer.WriteLine("return index switch");
                    using var switchScope = writer.EnterBlockScope(true);
                    foreach (var (i, property) in targetSpan)
                    {
                        writer.WriteLine($"{i} => {property.FieldName},");
                    }
                    writer.WriteLine("_ => null");
                    break;
                }
            }
        }

        private void EmitInvokeGreenGetter(CSharpProperty property)
        {
            writer.Write($"(({property.Owner.GreenClassName}) Green).{property.PropertyName}");
        }

        #endregion

        #region Token Replacers

        public void EmitGreenTokenReplacer(CSharpSyntaxModel model)
        {
            writer.WriteLine("// Generated file, do not edit");
            writer.WriteLine("using Prism.Core.Syntax.Green;");
            writer.WriteLine("using System.Diagnostics.CodeAnalysis;");
            writer.WriteLine();

            writer.WriteLine("namespace Prism.Core.Syntax;");
            writer.WriteLine();
            writer.WriteLine("public static partial class TokenReplacer");
            using var classBlock = writer.EnterBlockScope();
            writer.GenerateTokenReplacerMethods(model, "ReplaceFirstToken", false);
            writer.WriteLine();
            writer.GenerateTokenReplacerMethods(model, "ReplaceLastToken", true);
        }

        private void GenerateTokenReplacerMethods(
            CSharpSyntaxModel model,
            string methodName,
            bool isReversed
        )
        {
            foreach (var group in model.DispatchGroups)
            {
                writer.EmitTokenReplacerMethod(group, methodName, true, group.GreenClassName);
                writer.WriteLine();
            }

            writer.EmitTokenReplacerPartial(methodName, GreenTokenClass);
            writer.EmitTokenReplacerPartial(methodName, GreenTriviaClass);
            writer.EmitTokenReplacerPartial(methodName, GreenListNodeClass);

            writer.EmitConcreteReplaceMethod(model, methodName, isReversed);
        }

        private void EmitConcreteReplaceMethod(
            CSharpSyntaxModel model,
            string methodName,
            bool isReversed = false
        )
        {
            foreach (
                var node in model
                    .Modules.AsValueEnumerable()
                    .SelectMany(m => m.Nodes)
                    .Where(n => !n.IsAbstract)
            )
            {
                writer.WriteLine();

                writer.WriteLine("[return: NotNullIfNotNull(nameof(node))]");
                var className = node.GreenClassName;
                writer.WriteLine(
                    $"internal static {className}? {methodName}({className}? node, {GreenTokenClass} newToken)"
                );
                using var functionScope = writer.EnterBlockScope();
                writer.WriteLine("if (node is null)");
                using (writer.EnterIndentationScope())
                {
                    writer.WriteLine("return null;");
                }
                writer.WriteLine();

                if (isReversed)
                {
                    foreach (var prop in node.Properties.AsValueEnumerable().Reverse())
                    {
                        writer.EmitTryRepleceNodeProperty(methodName, prop);
                    }
                }
                else
                {
                    foreach (var prop in node.Properties)
                    {
                        writer.EmitTryRepleceNodeProperty(methodName, prop);
                    }
                }

                writer.WriteLine("return node;");
            }
        }

        private void EmitTryRepleceNodeProperty(string methodName, CSharpProperty prop)
        {
            writer.WriteLine($"var old{prop.PropertyName} = node.{prop.PropertyName};");
            writer.WriteLine(
                $"var new{prop.PropertyName} = {methodName}(old{prop.PropertyName}, newToken);"
            );
            writer.WriteLine($"if (new{prop.PropertyName} != old{prop.PropertyName})");
            using (writer.EnterBlockScope())
            {
                writer.WriteLine($"return node.With{prop.PropertyName}(new{prop.PropertyName});");
            }
            writer.WriteLine();
        }

        private void EmitTokenReplacerMethod(
            CSharpDispatchGroup group,
            string methodName,
            bool isGreen,
            string returnType
        )
        {
            writer.WriteLine("[return: NotNullIfNotNull(nameof(node))]");
            var baseName = isGreen ? group.GreenClassName : group.RedClassName;

            var visibility = isGreen ? "internal" : "public";
            writer.WriteLine(
                $"{visibility} static {returnType}? {methodName}({baseName}? node, {GreenTokenClass} newToken)"
            );
            using var functionScope = writer.EnterBlockScope();
            writer.WriteLine("return node switch ");
            using var switchScope = writer.EnterBlockScope(true);
            writer.WriteLine("null => null,");
            if (group.IncludesListNode && isGreen)
            {
                writer.EmitNodeTypeCase(methodName, "GreenListNode", "list");
                writer.EmitNodeTypeCase(methodName, "GreenToken", "token");
                writer.EmitNodeTypeCase(methodName, "GreenTrivia", "trivia");
            }

            foreach (var node in group.Nodes)
            {
                writer.EmitNodeTypeCase(methodName, node.GreenClassName, node.CSharpVariableName);
            }

            const string exceptionLine =
                "throw new InvalidOperationException(\"Invalid node type passed into visit\")";
            writer.WriteLine($"_ => {exceptionLine}");
        }

        private void EmitTokenReplacerPartial(string methodName, string className)
        {
            writer.WriteLine("[return: NotNullIfNotNull(nameof(node))]");
            writer.WriteLine(
                $"internal static partial {className}? {methodName}({className}? node, GreenToken newToken);"
            );
        }

        private void EmitNodeTypeCase(string methodName, string className, string variableName)
        {
            writer.WriteLine(
                $"{className} {variableName} => {methodName}({variableName}, newToken),"
            );
        }

        #endregion

        #region Diagnostic Codes

        public void EmitDiagnosticCodes(CSharpSyntaxModel model)
        {
            writer.WriteLine("// Generated file, do not edit");
            writer.WriteLine("using NetEscapades.EnumGenerators;");
            writer.WriteLine("using System.Runtime.Serialization;");
            writer.WriteLine();
            writer.WriteLine("namespace Prism.Core.Diagnostics;");
            writer.EmitDiagnosticCodeEnum(model);
        }

        private void EmitDiagnosticCodeEnum(CSharpSyntaxModel model)
        {
            writer.WriteLine("[EnumExtensions]");
            writer.WriteLine("public enum DiagnosticCode : uint");
            using var scope = writer.EnterBlockScope();
            writer.WriteLine("Unknown = 0,");
            foreach (var category in model.Diagnostics)
            {
                writer.WriteLine();
                writer.WriteLine(
                    $"// -- {category.DisplayName} ({category.Start}-{category.End}) ---"
                );
                foreach (var diagnostic in category.Diagnostics)
                {
                    writer.WriteLine($"[EnumMember(Value = \"E{diagnostic.Value:D4}\")]");
                    writer.WriteLine($"{diagnostic.CSharpName} = {diagnostic.Value},");
                }
            }
        }

        #endregion

        #region DiagnosticDescriptors

        public void EmitDiagnosticDescriptors(CSharpSyntaxModel model)
        {
            writer.WriteLine("// Generated file, do not edit");
            writer.WriteLine("namespace Prism.Core.Diagnostics;");
            writer.WriteLine();
            writer.WriteLine("public static class DiagnosticDescriptors");
            using var classScope = writer.EnterBlockScope();

            foreach (
                var diagnostic in model
                    .Diagnostics.AsValueEnumerable()
                    .SelectMany(x => x.Diagnostics)
            )
            {
                writer.EmitDiagnosticDescriptorConstant(diagnostic);
                writer.WriteLine();
            }
            writer.EmitDescriptorLookupFunction(model);
        }

        private void EmitDiagnosticDescriptorConstant(CSharpDiagnostic diagnostic)
        {
            writer.Write(
                $"public static readonly DiagnosticDescriptor {diagnostic.CSharpName} = new()"
            );
            using var scope = writer.EnterBlockScope(true);
            writer.WriteLine($"Id = DiagnosticCode.{diagnostic.CSharpName}.ToStringFast(),");
            writer.WriteLine($"Title = \"{diagnostic.Title}\",");
            writer.Write($"MessageFormat = \"{diagnostic.Format}\",");
            writer.WriteLine($"Category = \"{diagnostic.Category.Name}\",");
            writer.WriteLine(
                $"DefaultSeverity = DiagnosticSeverity.{GetCSharpSeverity(diagnostic.Severity)},"
            );
            if (!string.IsNullOrEmpty(diagnostic.Explanation))
            {
                writer.WriteLine($" = Explanation\"{diagnostic.Explanation}\",");
            }

            if (!string.IsNullOrEmpty(diagnostic.HelpLink))
            {
                writer.WriteLine($"HelpLink = \"{diagnostic.HelpLink}\",");
            }

            if (diagnostic.Tags.IsEmpty)
                return;
            writer.WriteLine($"Tags = [");
            using (writer.EnterIndentationScope())
            {
                foreach (var tag in diagnostic.Tags)
                {
                    writer.WriteLine($"\"{tag}\",");
                }
            }
            writer.WriteLine("],");
        }

        private void EmitDescriptorLookupFunction(CSharpSyntaxModel model)
        {
            writer.WriteLine("public static DiagnosticDescriptor? Find(DiagnosticCode code)");
            using var scope = writer.EnterBlockScope();
            writer.WriteLine("return code switch");
            using var switchScope = writer.EnterBlockScope(true);
            foreach (
                var diagnostic in model
                    .Diagnostics.AsValueEnumerable()
                    .SelectMany(x => x.Diagnostics)
            )
            {
                writer.WriteLine(
                    $"DiagnosticCode.{diagnostic.CSharpName} => {diagnostic.CSharpName},"
                );
            }
            writer.WriteLine("_ => null");
        }
        #endregion

        #region Diagnostic Factories

        public void EmitDiagnosticFactories(CSharpSyntaxModel mode)
        {
            writer.WriteLine("// Generated file, do not edit");
            writer.WriteLine("namespace Prism.Core.Diagnostics;");
            writer.WriteLine();

            writer.WriteLine("public static class DiagnosticExtensions");
            using var classScope = writer.EnterBlockScope();
            writer.WriteLine("extension (DiagnosticInfo)");
            using (writer.EnterBlockScope())
            {
                foreach (
                    var (i, diagnostic) in mode
                        .Diagnostics.AsValueEnumerable()
                        .SelectMany(x => x.Diagnostics)
                        .Index()
                )
                {
                    if (i > 0)
                        writer.WriteLine();

                    writer.EmitDiagnosticInfoFactory(diagnostic, false);
                    writer.WriteLine();
                    writer.EmitDiagnosticInfoFactory(diagnostic, true);
                }
            }

            writer.WriteLine();
            writer.WriteLine("extension (Diagnostic)");
            using (writer.EnterBlockScope())
            {
                foreach (
                    var diagnostic in mode
                        .Diagnostics.AsValueEnumerable()
                        .SelectMany(x => x.Diagnostics)
                )
                {
                    writer.WriteLine();
                    writer.EmitDiagnosticFactory(diagnostic, false, false);
                    writer.WriteLine();
                    writer.EmitDiagnosticFactory(diagnostic, true, false);
                    writer.WriteLine();
                    writer.EmitDiagnosticFactory(diagnostic, false, true);
                    writer.WriteLine();
                    writer.EmitDiagnosticFactory(diagnostic, true, true);
                }
            }
        }

        private void EmitDiagnosticInfoFactory(
            CSharpDiagnostic diagnostic,
            bool withSeverityOverride
        )
        {
            writer.Write($"internal static DiagnosticInfo {diagnostic.CSharpName}(");
            if (withSeverityOverride)
            {
                writer.Write("DiagnosticSeverity severity");
            }
            foreach (var (i, parameter) in diagnostic.Arguments.AsValueEnumerable().Index())
            {
                if (i > 0 || withSeverityOverride)
                    writer.Write(", ");

                writer.Write($"{parameter.CSharpType} {parameter.CSharpName}");
            }
            writer.WriteLine(")");
            using var blockScope = writer.EnterBlockScope();
            writer.Write(
                $"return new DiagnosticInfo(DiagnosticDescriptors.{diagnostic.CSharpName}"
            );
            if (withSeverityOverride)
            {
                writer.Write(", severity");
            }
            foreach (var parameter in diagnostic.Arguments)
            {
                writer.Write($", {parameter.CSharpName}");
            }
            writer.WriteLine(");");
        }

        private void EmitDiagnosticFactory(
            CSharpDiagnostic diagnostic,
            bool withSeverityOverride,
            bool withAdditionalLocations
        )
        {
            writer.Write($"public static Diagnostic {diagnostic.CSharpName}(");
            if (withSeverityOverride)
            {
                writer.Write("DiagnosticSeverity severity, ");
            }

            writer.Write("Location location");

            if (withAdditionalLocations)
            {
                writer.Write(", IEnumerable<Location> additionalLocations");
            }

            foreach (var param in diagnostic.Arguments)
            {
                writer.Write($", {param.CSharpType} {param.CSharpName}");
            }
            writer.WriteLine(")");
            using var blockScope = writer.EnterBlockScope();
            writer.Write($"return new Diagnostic(DiagnosticDescriptors.{diagnostic.CSharpName}");
            if (withSeverityOverride)
            {
                writer.Write(", severity");
            }
            writer.Write(", location");
            if (withAdditionalLocations)
            {
                writer.Write(", additionalLocations");
            }
            foreach (var param in diagnostic.Arguments)
            {
                writer.Write($", {param.CSharpName}");
            }
            writer.WriteLine(");");
        }

        #endregion
    }

    private sealed class TrieNode
    {
        public Dictionary<char, TrieNode> Children { get; } = new();
        public CSharpToken? Value { get; set; }
    }

    private static TrieNode ConstructPunctuationTrie(CSharpSyntaxModel model)
    {
        var node = new TrieNode();
        foreach (
            var production in model
                .Tokens.AsValueEnumerable()
                .Where(t => t.Category == TokenCategory.Punctuation)
        )
        {
            var current = node;

            foreach (var c in production.Text!)
            {
                if (current.Children.TryGetValue(c, out var child))
                {
                    current = child;
                }
                else
                {
                    child = new TrieNode();
                    current.Children[c] = child;
                    current = child;
                }
            }

            current.Value = production;
        }

        return node;
    }

    private static string GetCSharpSeverity(DiagnosticSeverity severity)
    {
        return severity switch
        {
            DiagnosticSeverity.Error => "Error",
            DiagnosticSeverity.Warning => "Warning",
            DiagnosticSeverity.Info => "Info",
            DiagnosticSeverity.Hint => "Hint",
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null),
        };
    }
}
