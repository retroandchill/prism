// @file CppEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using CaseConverter;
using Humanizer;
using Prism.SyntaxGenerator.Models.Resolved;
using Prism.SyntaxGenerator.Output;
using ZLinq;

namespace Prism.SyntaxGenerator.Emitters;

public sealed class CppEmitter
{
    private const string BaseModuleName = "prism.core";
    private const string SyntaxKindClass = "SyntaxKind";

    private readonly SyntaxModel _model;
    private readonly Dictionary<SyntaxKind, string> _syntaxKindCppNames = new();
    private readonly Dictionary<SyntaxGroup, string> _syntaxKindGroupCppNames = new();
    private readonly Dictionary<SyntaxGroup, string> _syntaxKindGroupDisplayNames = new();

    public CppEmitter(SyntaxModel model)
    {
        _model = model;
        _syntaxKindCppNames.EnsureCapacity(model.Kinds.Length);
    }

    public void EmitSyntaxKinds(CodeWriter writer)
    {
        writer.WriteLine($"export module {BaseModuleName}:syntax.kind;");
        writer.WriteLine();
        writer.WriteLine("import std;");
        writer.WriteLine();

        using var namespaceScope = writer.EnterNamespaceScope("prism");

        writer.WriteLine($"export enum class {SyntaxKindClass} : std::uint16_t");
        using (writer.EnterBlockScope(true))
        {
            writer.WriteLine("none = 0,");
            writer.WriteLine("line = 1,");

            foreach (var group in _model.KindGroups)
            {
                writer.WriteLine();
                writer.WriteLineUnindented($"#pragma region {GetDisplayName(group)}");
                writer.WriteLine($"{GetCppName(group)}_start = {group.StartValue},");
                foreach (var kind in group.SyntaxKinds)
                {
                    writer.WriteLine($"{GetCppName(kind)} = {kind.Value},");
                }
                writer.WriteLine($"{GetCppName(group)}_end = {group.EndValue},");
                writer.WriteLineUnindented("#pragma endregion");
            }
        }
        writer.WriteLine();

        writer.WriteLine(
            $"export constexpr std::string_view get_name(const {SyntaxKindClass} kind)"
        );
        using (writer.EnterBlockScope())
        {
            writer.WriteLine("switch (kind)");
            using var switchScope = writer.EnterBlockScope();
            foreach (var kind in _model.Kinds)
            {
                writer.WriteLine(
                    $"case {SyntaxKindClass}::{GetCppName(kind)}: return \"{GetDisplayName(kind)}\";"
                );
            }
            writer.WriteLine(
                $"default: throw std::invalid_argument{{\"Unknown {SyntaxKindClass}\"}};"
            );
        }

        foreach (var group in _model.KindGroups)
        {
            writer.WriteLine();
            var groupName = GetCppName(group);
            writer.WriteLine($"export constexpr bool is_{groupName}(const {SyntaxKindClass} kind)");
            using var block = writer.EnterBlockScope();
            writer.WriteLine(
                $"return std::to_underlying(kind) >= std::to_underlying({groupName}_start) "
                    + $"&& std::to_underlying(kind) <= std::to_underlying({groupName}_end);"
            );
        }

        writer.WriteLine();
        EmitIsSyntaxCategory(writer, "token", k => k.Kind == SyntaxGroupKind.Token);
        writer.WriteLine();
        EmitIsSyntaxCategory(writer, "node", k => k.Kind == SyntaxGroupKind.Node);
    }

    private void EmitIsSyntaxCategory(
        CodeWriter writer,
        string groupName,
        Func<SyntaxGroup, bool> predicate
    )
    {
        writer.WriteLine();
        writer.WriteLine($"export constexpr bool is_{groupName}(const {SyntaxKindClass} kind)");
        using (writer.EnterBlockScope())
        {
            writer.Write("return ");
            var itemWritten = false;
            foreach (var kind in _model.KindGroups.AsValueEnumerable().Where(predicate))
            {
                if (itemWritten)
                {
                    writer.Write(" || ");
                }
                writer.Write($"is_{GetCppName(kind)}(kind)");
                itemWritten = true;
            }

            if (!itemWritten)
            {
                writer.Write("false");
            }
            writer.WriteLine(';');
        }
    }

    private string GetCppName(SyntaxKind kind)
    {
        return _syntaxKindCppNames.GetOrAdd(kind, k => k.Name.ToSnakeCase());
    }

    private string GetCppName(SyntaxGroup group)
    {
        return _syntaxKindGroupCppNames.GetOrAdd(
            group,
            g => g.Name.Singularize(false).ToSnakeCase()
        );
    }

    private static string GetDisplayName(SyntaxKind kind)
    {
        return kind.Data switch
        {
            SyntaxTrivia trivia => trivia.DisplayName ?? trivia.Name,
            SyntaxToken token => token.DisplayName ?? token.Text ?? token.Name,
            _ => kind.Name,
        };
    }

    private string GetDisplayName(SyntaxGroup group)
    {
        return _syntaxKindGroupDisplayNames.GetOrAdd(group, g => g.Name.Humanize());
    }
}
