// @file CSharpEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.SyntaxGenerator.Models.CSharp;
using Prism.SyntaxGenerator.Models.Resolved;
using Prism.SyntaxGenerator.Output;
using ZLinq;
using static Prism.SyntaxGenerator.Metadata.CommonNames;

namespace Prism.SyntaxGenerator.Emitters;

public static class CSharpEmitter
{
    extension(CodeWriter writer)
    {
        public void EmitSyntaxKinds(CSharpSyntaxModel model)
        {
            writer.WriteLine("// Generated file, do not edit");
            writer.WriteLine("namespace Prism.Core.Syntax;");
            writer.WriteLine();

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

                    writer.WriteLineUnindented("#endregion");
                }
            }

            writer.WriteLine();
            writer.WriteLine($"public static class {SyntaxKindClass}Extensions");

            using (writer.EnterBlockScope())
            {
                writer.WriteLine($"extension ({SyntaxKindClass} kind)");
                using var extensionScope = writer.EnterBlockScope();
                writer.WriteLine("public string Name");
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
                    writer.WriteLine();
                    var groupName = group.CSharpName;
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
    }
}
