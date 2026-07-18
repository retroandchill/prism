// @file CppEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Humanizer;
using Prism.SyntaxGenerator.Models.Cpp;
using Prism.SyntaxGenerator.Models.Resolved;
using Prism.SyntaxGenerator.Models.Spec;
using Prism.SyntaxGenerator.Output;
using ZLinq;

namespace Prism.SyntaxGenerator.Emitters;

public sealed class CppEmitter
{
    private const string BaseModuleName = "prism.core";
    private const string SyntaxKindClass = "SyntaxKind";
    private const string GreenNodeClass = "GreenNode";
    private const string GreenTokenClass = "GreenToken";
    private const string PrismNamespace = "prism";
    private const string GreenFragmentName = "syntax.green";
    private const string RedFragmentName = "syntax";

    private readonly SyntaxModel _model;
    private readonly Dictionary<SyntaxKind, string> _syntaxKindCppNames = new();
    private readonly Dictionary<SyntaxGroup, string> _syntaxKindGroupCppNames = new();
    private readonly Dictionary<SyntaxGroup, string> _syntaxKindGroupDisplayNames = new();
    private readonly Dictionary<SyntaxModule, string> _syntaxModuleCppNames = new();
    private readonly Dictionary<SyntaxNode, string> _syntaxGreenClassNames = new();
    private readonly Dictionary<SyntaxProperty, CppPropertyInfo> _propertyInfos = new();

    public CppEmitter(SyntaxModel model)
    {
        _model = model;
        _syntaxKindCppNames.EnsureCapacity(model.Kinds.Length);
    }

    #region Syntax Kinds
    public void EmitSyntaxKinds(CodeWriter writer)
    {
        writer.WriteLine($"export module {BaseModuleName}:syntax.kind;");
        writer.WriteLine();
        writer.WriteLine("import std;");
        writer.WriteLine();

        using var namespaceScope = writer.EnterNamespaceScope(PrismNamespace);

        writer.WriteLine($"export enum class {SyntaxKindClass} : std::uint16_t");
        using (writer.EnterBlockScope(true))
        {
            writer.WriteLine("none = 0,");
            writer.WriteLine("list = 1,");

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
                $"return std::to_underlying(kind) >= std::to_underlying({SyntaxKindClass}::{groupName}_start) "
                    + $"&& std::to_underlying(kind) <= std::to_underlying({SyntaxKindClass}::{groupName}_end);"
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
    #endregion

    #region Lexing Utils
    public void EmitLexingUtils(CodeWriter writer)
    {
        writer.WriteLine($"export module {BaseModuleName}:syntax.lexing_utils;");
        writer.WriteLine();
        writer.WriteLine($"import :{GreenFragmentName}.token;");
        writer.WriteLine("import :text.cursor;");
        writer.WriteLine();
        using var scope = writer.EnterNamespaceScope(PrismNamespace);
        EmitStaticTokenLookup(writer);

        writer.WriteLine();
        EmitKeywordLookup(writer);

        writer.WriteLine();
        EmitPunctuationTrie(writer);
    }

    private void EmitStaticTokenLookup(CodeWriter writer)
    {
        writer.WriteLine(
            $"GreenPtr<{GreenTokenClass}> get_static_green_token(const {SyntaxKindClass} kind)"
        );
        using var scope = writer.EnterBlockScope();
        EmitStaticGreenTokenBranch(writer, "keyword", t => t.Category == TokenCategory.Keyword);

        writer.WriteLine();
        EmitStaticGreenTokenBranch(
            writer,
            "punctuation",
            t => t.Category == TokenCategory.Punctuation
        );

        writer.WriteLine();
        writer.WriteLine("return nullptr;");
    }

    private void EmitStaticGreenTokenBranch(
        CodeWriter writer,
        string category,
        Func<SyntaxToken, bool> predicate
    )
    {
        writer.WriteLine("if (is_keyword(kind))");
        using (writer.EnterBlockScope())
        {
            writer.Write($"static std::array {category}_list = ");
            using (writer.EnterBlockScope(true))
            {
                foreach (var keyword in _model.Tokens.AsValueEnumerable().Where(predicate))
                {
                    writer.WriteLine(
                        $"make_ref_counted<{GreenTokenClass}>({SyntaxKindClass}::{GetCppName(keyword.Kind)}),"
                    );
                }
            }

            writer.WriteLine();
            writer.WriteLine(
                $"return {category}_list[std::to_underlying(kind) - std::to_underlying({SyntaxKindClass}::{category}_start)];"
            );
        }
    }

    private void EmitKeywordLookup(CodeWriter writer)
    {
        writer.WriteLine(
            $"constexpr Optional<{SyntaxKindClass}> match_keyword(const std::string_view text)"
        );
        using var scope = writer.EnterBlockScope();
        writer.WriteLine("switch (text.size())");
        using (writer.EnterBlockScope())
        {
            foreach (
                var sizeClass in _model
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
                        $"if (text == \"{keyword.Text!}\") return {SyntaxKindClass}::{GetCppName(keyword.Kind)};"
                    );
                }
                writer.WriteLine("break;");
            }
        }

        writer.WriteLine();
        writer.WriteLine("return std::nullopt;");
    }

    private void EmitPunctuationTrie(CodeWriter writer)
    {
        var trie = ConstructPunctuationTrie();
        writer.WriteLine(
            $"constexpr Optional<{SyntaxKindClass}> match_punctuation(TextCursor &cursor)"
        );
        using var block = writer.EnterBlockScope();
        WriteTrie(writer, trie);
        writer.WriteLine();
        writer.WriteLine("return std::nullopt;");
    }

    private sealed class TrieNode
    {
        public Dictionary<char, TrieNode> Children { get; } = new();
        public SyntaxToken? Value { get; set; }
    }

    private TrieNode ConstructPunctuationTrie()
    {
        var node = new TrieNode();
        foreach (
            var production in _model
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

    private void WriteTrie(CodeWriter writer, TrieNode node)
    {
        switch (node.Children.Count)
        {
            case 1:
            {
                var (character, child) = node.Children.Single();
                writer.WriteLine($"if (cursor.current() == '{character}')");
                using var block = writer.EnterBlockScope();
                writer.WriteLine("cursor.advance();");
                WriteTrie(writer, child);
                break;
            }
            case > 1:
            {
                writer.WriteLine("switch (cursor.current())");
                using var block = writer.EnterBlockScope();
                // Sort by the key to ensure consistent output
                foreach (var (character, child) in node.Children.OrderBy(x => x.Key))
                {
                    writer.WriteLine($"case '{character}':");
                    using var unmarkedBlock = writer.EnterIndentationScope();
                    writer.WriteLine("cursor.advance();");
                    WriteTrie(writer, child);
                    if (child.Value is null)
                        writer.WriteLine("break;");
                }

                break;
            }
        }

        if (node.Value is { } terminal)
        {
            writer.WriteLine($"return SyntaxKind::{GetCppName(terminal.Kind)};");
        }
    }
    #endregion

    #region Green Node Interface
    public void EmitGreenNodeInterface(CodeWriter writer, SyntaxModule module)
    {
        writer.WriteLine(
            $"export module {BaseModuleName}:{GreenFragmentName}.{GetCppName(module)};"
        );
        writer.WriteLine();
        writer.WriteLine($"import :{GreenFragmentName}.node;");
        writer.WriteLine($"import :{GreenFragmentName}.token;");
        writer.WriteLine($"import :{GreenFragmentName}.separated_list;");
        writer.WriteLine();
        using var scope = writer.EnterNamespaceScope(PrismNamespace);
        if (module.ForwardDeclarations.Count > 0)
        {
            foreach (
                var declaration in module
                    .ForwardDeclarations.AsValueEnumerable()
                    .OrderBy(x => x.Name)
            )
            {
                writer.WriteLine($"class {GetGreenCppName(declaration)};");
            }
            writer.WriteLine();
        }

        foreach (var (i, node) in module.Nodes.AsValueEnumerable().Index())
        {
            if (i > 0)
                writer.WriteLine();

            EmitGreenNodeClassDeclaration(writer, node);
        }
    }

    private void EmitGreenNodeClassDeclaration(CodeWriter writer, SyntaxNode node)
    {
        var final = !node.IsAbstract ? " final" : "";
        var name = GetGreenCppName(node);
        var baseName = GetGreenCppName(node.Base);
        writer.WriteLine($"class {name}{final} : public {baseName}");
        using var scope = writer.EnterBlockScope(true);
        if (node.IsAbstract)
        {
            EmitGreenNodeAbstractClassBody(writer, node, name, baseName);
        }
        else
        {
            EmitGreenNodeConcreteClassBody(writer, node, name);
        }
    }

    private void EmitGreenNodeAbstractClassBody(
        CodeWriter writer,
        SyntaxNode node,
        string name,
        string baseName
    )
    {
        using (writer.EnterIndentationScope(-1))
        {
            writer.WriteLine("protected:");
        }

        writer.WriteLine(
            $"explicit constexpr {name}(const {SyntaxKindClass} kind, "
                + $"DiagnosticInfoList diagnostics = {{}}) : "
                + $"{baseName}{{kind, std::move(diagnostics)}} {{ }}"
        );
        writer.WriteLine();

        using (writer.EnterIndentationScope(-1))
        {
            writer.WriteLine("public:");
        }
        foreach (var property in node.Properties)
        {
            var info = GetInfo(property);
            writer.Write("[[nodiscard]] virtual ");
            EmitGreenGetterType(writer, property);
            writer.WriteLine($" {info.GetterName}() const noexcept = 0;");
            writer.WriteLine();
        }
        writer.WriteLine(
            $"[[nodiscard]] static constexpr bool instanceof(const {GreenNodeClass}& node) noexcept"
        );
        using (writer.EnterBlockScope())
        {
            writer.Write("return ");
            foreach (
                var (i, derived) in GetAllDerivedTypes(node)
                    .AsValueEnumerable()
                    .Where(n => !n.IsAbstract)
                    .Index()
            )
            {
                if (i > 0)
                    writer.Write(" || ");

                writer.Write($"node.kind() == {SyntaxKindClass}::{GetCppName(derived.Kind!)}");
            }
            writer.WriteLine(";");
        }
    }

    private static IEnumerable<SyntaxNode> GetAllDerivedTypes(SyntaxNode node)
    {
        var stack = new Stack<SyntaxNode>(node.DerivedTypes.AsEnumerable().Reverse());
        while (stack.Count > 0)
        {
            var item = stack.Pop();
            yield return item;

            foreach (var child in item.DerivedTypes.AsEnumerable().Reverse())
            {
                stack.Push(child);
            }
        }
    }

    private void EmitGreenNodeConcreteClassBody(CodeWriter writer, SyntaxNode node, string name)
    {
        using (writer.EnterIndentationScope(-1))
        {
            writer.WriteLine("public:");
        }

        var explicitKeyword = node.Properties.Count <= 1 ? "explicit " : "";
        writer.Write($"{explicitKeyword}{name}(");
        foreach (var property in node.Properties)
        {
            var info = GetInfo(property);
            EmitPropertyFieldType(writer, property);
            writer.Write($" {info.ParameterName}, ");
        }
        writer.WriteLine("DiagnosticInfoList diagnostics = {});");
        writer.WriteLine();

        writer.WriteLine($"~{name}() override;");

        writer.WriteLine();
        foreach (var property in node.Properties)
        {
            var info = GetInfo(property);
            writer.Write("[[nodiscard]] constexpr ");
            EmitGreenGetterType(writer, property);
            var @override = property.IsOverride ? " override" : "";
            writer.WriteLine($" {info.GetterName}() const noexcept{@override}");
            using (writer.EnterBlockScope())
            {
                switch (property.Shape)
                {
                    case PropertyShape.Single:
                        writer.WriteLine($"return *{info.FieldName};");
                        break;
                    case PropertyShape.Optional:
                        writer.WriteLine($"return {info.FieldName}.get();");
                        break;
                    case PropertyShape.List or PropertyShape.SeparatedList:
                        writer.WriteLine($"return {info.FieldName};");
                        break;
                    default:
                        throw new InvalidOperationException("Unknown property shape");
                }
            }
            writer.WriteLine();
        }

        writer.WriteLine(
            $"[[nodiscard]] static constexpr bool instanceof(const {GreenNodeClass}& node) noexcept"
        );
        using (writer.EnterBlockScope())
        {
            writer.WriteLine($"return node.kind() == {SyntaxKindClass}::{GetCppName(node.Kind!)};");
        }
        writer.WriteLine();
        writer.WriteLine(
            $"[[nodiscard]] Optional<const {GreenNodeClass}&> get_child(std::size_t index) const override;"
        );

        writer.WriteLine();
        using (writer.EnterIndentationScope(-1))
        {
            writer.WriteLine("private:");
        }

        foreach (var property in node.Properties)
        {
            var info = GetInfo(property);
            EmitPropertyFieldType(writer, property);
            writer.WriteLine($" {info.FieldName};");
        }
    }

    #endregion

    #region Green Node Implementation
    public void EmitGreenNodeImplementation(CodeWriter writer, SyntaxModule module)
    {
        var moduleName = GetCppName(module);
        writer.WriteLine($"module {BaseModuleName}:{GreenFragmentName}.{moduleName}.impl;");
        writer.WriteLine();
        writer.WriteLine($"import :{GreenFragmentName}.{moduleName};");
        foreach (var import in module.Dependencies.AsValueEnumerable().OrderBy(x => x.Name))
        {
            writer.WriteLine($"import :{GreenFragmentName}.{GetCppName(import)};");
        }
        writer.WriteLine();
        using var namespaceScope = writer.EnterNamespaceScope(PrismNamespace);
        foreach (var node in module.Nodes.AsValueEnumerable().Where(n => !n.IsAbstract))
        {
            var nodeName = GetGreenCppName(node);
            EmitConcreteGreenNodeConstructor(writer, nodeName, node);

            writer.WriteLine();
            writer.WriteLine($"{nodeName}::~{nodeName}() = default;");
            writer.WriteLine();
            EmitGreenGetChildMethod(writer, node, nodeName);
        }
    }

    private void EmitConcreteGreenNodeConstructor(
        CodeWriter writer,
        string nodeName,
        SyntaxNode node
    )
    {
        writer.Write($"{nodeName}::{nodeName}(");
        foreach (var property in node.Properties)
        {
            var info = GetInfo(property);
            EmitPropertyFieldType(writer, property);
            writer.Write($" {info.ParameterName}, ");
        }
        writer.Write(
            $"DiagnosticInfoList diagnostics) : "
                + $"{GetGreenCppName(node.Base)}{{{SyntaxKindClass}::{GetCppName(node.Kind!)}, std::move(diagnostics)}}"
        );
        foreach (var property in node.Properties)
        {
            var info = GetInfo(property);
            writer.Write($", {info.FieldName}{{std::move({info.ParameterName})}}");
        }
        writer.WriteLine();
        using var scope = writer.EnterBlockScope();
        writer.WriteLine($"set_child_count({node.Properties.Count});");
        foreach (var property in node.Properties)
        {
            var info = GetInfo(property);
            if (property.Shape == PropertyShape.Optional)
                writer.Write($"if ({info.FieldName} != nullptr) ");
            writer.Write("adjust_flags_and_width(");
            switch (property.Shape)
            {
                case PropertyShape.Single or PropertyShape.Optional:
                    writer.Write($"*{info.FieldName}");
                    break;
                case PropertyShape.List or PropertyShape.SeparatedList:
                    writer.Write(info.FieldName);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            writer.WriteLine(");");
        }
    }

    private void EmitGreenGetChildMethod(CodeWriter writer, SyntaxNode node, string nodeName)
    {
        writer.WriteLine(
            $"Optional<const {GreenNodeClass}&> {nodeName}::get_child(std::size_t index) const"
        );
        using var blockScope = writer.EnterBlockScope();
        writer.WriteLine("switch (index)");
        using var switchScope = writer.EnterBlockScope();
        foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
        {
            writer.WriteLine($"case {i}:");
            using var caseScope = writer.EnterIndentationScope();
            var info = GetInfo(property);
            switch (property.Shape)
            {
                case PropertyShape.Single:
                    writer.WriteLine($"return *{info.FieldName};");
                    break;
                case PropertyShape.Optional:
                    writer.Write($"return {info.FieldName}.get();");
                    break;
                case PropertyShape.List or PropertyShape.SeparatedList:
                    writer.Write($"return {info.FieldName}.node();");
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
        writer.WriteLine("default:");
        writer.WriteLine("return std::nullopt;");
    }
    #endregion

    private void EmitGreenGetterType(CodeWriter writer, SyntaxProperty property)
    {
        switch (property.Shape)
        {
            case PropertyShape.Single:
                writer.Write($"const {GetGreenCppName(property.Type)}&");
                break;
            case PropertyShape.Optional:
                writer.Write($"Optional<const {GetGreenCppName(property.Type)}&>");
                break;
            case PropertyShape.List:
                writer.Write($"GreenSyntaxList<{GetGreenCppName(property.Type)}>");
                break;
            case PropertyShape.SeparatedList:
                writer.Write($"GreenSeparatedList<{GetGreenCppName(property.Type)}>");
                break;
            default:
                throw new InvalidOperationException("Unknown shape");
        }
    }

    private void EmitPropertyFieldType(CodeWriter writer, SyntaxProperty property)
    {
        switch (property.Shape)
        {
            case PropertyShape.Single or PropertyShape.Optional:
                writer.Write($"GreenPtr<{GetGreenCppName(property.Type)}>");
                break;
            case PropertyShape.List:
                writer.Write($"GreenSyntaxList<{GetGreenCppName(property.Type)}>");
                break;
            case PropertyShape.SeparatedList:
                writer.Write($"GreenSeparatedList<{GetGreenCppName(property.Type)}>");
                break;
        }
    }

    private string GetCppName(SyntaxKind kind)
    {
        return _syntaxKindCppNames.GetOrAdd(kind, k => k.Name.Underscore());
    }

    private string GetCppName(SyntaxGroup group)
    {
        return _syntaxKindGroupCppNames.GetOrAdd(group, g => GetSingularName(g.Name));
    }

    private static string GetSingularName(string name)
    {
        if (name.Equals("Trivia", StringComparison.OrdinalIgnoreCase))
            return "trivia";

        return name.Singularize(false).Underscore();
    }

    private string GetCppName(SyntaxModule module)
    {
        return _syntaxModuleCppNames.GetOrAdd(module, m => m.Name.Underscore());
    }

    private CppPropertyInfo GetInfo(SyntaxProperty property)
    {
        return _propertyInfos.GetOrAdd(property, p => new CppPropertyInfo(p));
    }

    private string GetGreenCppName(SyntaxNode? node)
    {
        return node is not null
            ? _syntaxGreenClassNames.GetOrAdd(node, n => $"Green{n.Name.Pascalize()}")
            : GreenNodeClass;
    }

    private string GetGreenCppName(SyntaxTypeReference reference)
    {
        if (reference.Definition is not null)
            return GetGreenCppName(reference.Definition);

        return reference.Name switch
        {
            "Node" => GreenNodeClass,
            "Token" => GreenTokenClass,
            _ => throw new InvalidOperationException($"Unknown type reference: {reference.Name}"),
        };
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
