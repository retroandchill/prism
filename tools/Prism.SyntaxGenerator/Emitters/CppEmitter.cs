// @file CppEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Humanizer;
using Prism.SyntaxGenerator.Metadata;
using Prism.SyntaxGenerator.Models.Cpp;
using Prism.SyntaxGenerator.Models.Resolved;
using Prism.SyntaxGenerator.Models.Spec;
using Prism.SyntaxGenerator.Output;
using ZLinq;

namespace Prism.SyntaxGenerator.Emitters;

public sealed class CppEmitter
{
    private readonly CppSyntaxModel _model;

    public CppEmitter(CppSyntaxModel model)
    {
        _model = model;
    }

    #region Syntax Kinds
    public void EmitSyntaxKinds(CodeWriter writer)
    {
        writer.WriteLine($"export module {CommonNames.BaseModuleName}:syntax.kind;");
        writer.WriteLine();
        writer.WriteLine("import std;");
        writer.WriteLine();

        using var namespaceScope = writer.EnterNamespaceScope(CommonNames.PrismNamespace);

        writer.WriteLine($"export enum class {CommonNames.SyntaxKindClass} : std::uint16_t");
        using (writer.EnterBlockScope(true))
        {
            writer.WriteLine("none = 0,");
            writer.WriteLine("list = 1,");

            foreach (var group in _model.KindGroups)
            {
                writer.WriteLine();
                writer.WriteLineUnindented($"#pragma region {group.CppName}");
                writer.WriteLine($"{group.CppName}_start = {group.StartValue},");
                foreach (var kind in group.SyntaxKinds)
                {
                    writer.WriteLine($"{group.Kind} = {kind.Value},");
                }
                writer.WriteLine($"{group.CppName}_end = {group.EndValue},");
                writer.WriteLineUnindented("#pragma endregion");
            }
        }
        writer.WriteLine();

        writer.WriteLine(
            $"export constexpr std::string_view get_name(const {CommonNames.SyntaxKindClass} kind)"
        );
        using (writer.EnterBlockScope())
        {
            writer.WriteLine("switch (kind)");
            using var switchScope = writer.EnterBlockScope();
            foreach (var kind in _model.Kinds)
            {
                writer.WriteLine(
                    $"case {CommonNames.SyntaxKindClass}::{kind.CppName}: return \"{kind.DisplayName}\";"
                );
            }
            writer.WriteLine(
                $"default: throw std::invalid_argument{{\"Unknown {CommonNames.SyntaxKindClass}\"}};"
            );
        }

        foreach (var group in _model.KindGroups)
        {
            writer.WriteLine();
            var groupName = group.CppName;
            writer.WriteLine(
                $"export constexpr bool is_{groupName}(const {CommonNames.SyntaxKindClass} kind)"
            );
            using var block = writer.EnterBlockScope();
            writer.WriteLine(
                $"return std::to_underlying(kind) >= std::to_underlying({CommonNames.SyntaxKindClass}::{groupName}_start) "
                    + $"&& std::to_underlying(kind) <= std::to_underlying({CommonNames.SyntaxKindClass}::{groupName}_end);"
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
        Func<CppGroup, bool> predicate
    )
    {
        writer.WriteLine();
        writer.WriteLine(
            $"export constexpr bool is_{groupName}(const {CommonNames.SyntaxKindClass} kind)"
        );
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
                writer.Write($"is_{kind.CppName}(kind)");
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
        writer.WriteLine($"export module {CommonNames.BaseModuleName}:syntax.lexing_utils;");
        writer.WriteLine();
        writer.WriteLine($"import :{CommonNames.GreenFragmentName}.token;");
        writer.WriteLine("import :text.cursor;");
        writer.WriteLine();
        using var scope = writer.EnterNamespaceScope(CommonNames.PrismNamespace);
        EmitStaticTokenLookup(writer);

        writer.WriteLine();
        EmitKeywordLookup(writer);

        writer.WriteLine();
        EmitPunctuationTrie(writer);
    }

    private void EmitStaticTokenLookup(CodeWriter writer)
    {
        writer.WriteLine(
            $"GreenPtr<{CommonNames.GreenTokenClass}> get_static_green_token(const {CommonNames.SyntaxKindClass} kind)"
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
        Func<CppToken, bool> predicate
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
                        $"make_ref_counted<{CommonNames.GreenTokenClass}>({CommonNames.SyntaxKindClass}::{keyword.Kind.CppName}),"
                    );
                }
            }

            writer.WriteLine();
            writer.WriteLine(
                $"return {category}_list[std::to_underlying(kind) - std::to_underlying({CommonNames.SyntaxKindClass}::{category}_start)];"
            );
        }
    }

    private void EmitKeywordLookup(CodeWriter writer)
    {
        writer.WriteLine(
            $"constexpr Optional<{CommonNames.SyntaxKindClass}> match_keyword(const std::string_view text)"
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
                        $"if (text == \"{keyword.Text!}\") return {CommonNames.SyntaxKindClass}::{keyword.Kind.CppName};"
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
            $"constexpr Optional<{CommonNames.SyntaxKindClass}> match_punctuation(TextCursor &cursor)"
        );
        using var block = writer.EnterBlockScope();
        WriteTrie(writer, trie);
        writer.WriteLine();
        writer.WriteLine("return std::nullopt;");
    }

    private sealed class TrieNode
    {
        public Dictionary<char, TrieNode> Children { get; } = new();
        public CppToken? Value { get; set; }
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
            writer.WriteLine($"return SyntaxKind::{terminal.Kind.CppName};");
        }
    }
    #endregion

    #region Green Node Interface
    public void EmitGreenNodeInterface(CodeWriter writer, CppModule module)
    {
        writer.WriteLine(
            $"export module {CommonNames.BaseModuleName}:{CommonNames.GreenFragmentName}.{module.CppName};"
        );
        writer.WriteLine();
        writer.WriteLine($"import :{CommonNames.GreenFragmentName}.node;");
        writer.WriteLine($"import :{CommonNames.GreenFragmentName}.token;");
        writer.WriteLine($"import :{CommonNames.GreenFragmentName}.separated_list;");
        writer.WriteLine();
        using var scope = writer.EnterNamespaceScope(CommonNames.PrismNamespace);
        if (module.ForwardDeclarations.Count > 0)
        {
            foreach (
                var declaration in module
                    .ForwardDeclarations.AsValueEnumerable()
                    .OrderBy(x => x.Name)
            )
            {
                writer.WriteLine($"class {declaration.GreenClassName};");
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

    private void EmitGreenNodeClassDeclaration(CodeWriter writer, CppNode node)
    {
        var final = !node.IsAbstract ? " final" : "";
        var name = node.GreenClassName;
        var baseName = node.Base.GreenClassName;
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
        CppNode node,
        string name,
        string baseName
    )
    {
        using (writer.EnterIndentationScope(-1))
        {
            writer.WriteLine("protected:");
        }

        writer.WriteLine(
            $"explicit constexpr {name}(const {CommonNames.SyntaxKindClass} kind, "
                + $"DiagnosticInfoList diagnostics = {{}}) : "
                + $"{baseName}{{kind, std::move(diagnostics)}} {{ }}"
        );
        writer.WriteLine();

        writer.WriteAccessSpecifier(CppAccessSpecifier.Public);
        foreach (var property in node.Properties)
        {
            writer.Write("[[nodiscard]] virtual ");
            EmitGreenGetterType(writer, property);
            writer.WriteLine($" {property.GetterName}() const noexcept = 0;");
            writer.WriteLine();
        }
        writer.WriteLine(
            $"[[nodiscard]] static constexpr bool instanceof(const {CommonNames.GreenNodeClass}& node) noexcept"
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

                writer.Write(
                    $"node.kind() == {CommonNames.SyntaxKindClass}::{derived.Kind!.CppName}"
                );
            }
            writer.WriteLine(";");
        }

        if (node.Properties.Length == 0)
            return;

        writer.WriteLine();
        foreach (var property in node.Properties)
        {
            writer.WriteLine("template <typename Self>");
            writer.Write(
                $"[[nodiscard]] constexpr GreenPtr<std::decay_t<Self>> with_{property.GetterName}(const Self& self, "
            );
            EmitGreenFieldType(writer, property);
            writer.WriteLine($"{property.ParameterName})");
            using (writer.EnterBlockScope())
            {
                writer.WriteLine(
                    $"return static_pointer_cast<std::decay_t<Self>>(self.with_{property.GetterName}_core(std::move({property.ParameterName})));"
                );
            }

            writer.WriteLine();
        }

        writer.WriteAccessSpecifier(CppAccessSpecifier.Protected);
        foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
        {
            if (i > 0)
                writer.WriteLine();

            writer.Write(
                $"[[nodiscard]] virtual GreenPtr<{name}> with_{property.GetterName}_core("
            );
            EmitGreenFieldType(writer, property);
            writer.WriteLine($"{property.ParameterName}) const = 0;");
        }
    }

    private static IEnumerable<CppNode> GetAllDerivedTypes(CppNode node)
    {
        var stack = new Stack<CppNode>(node.DerivedTypes.AsEnumerable().Reverse());
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

    private void EmitGreenNodeConcreteClassBody(CodeWriter writer, CppNode node, string name)
    {
        writer.WriteAccessSpecifier(CppAccessSpecifier.Public);

        var explicitKeyword = node.Properties.Length <= 1 ? "explicit " : "";
        writer.Write($"{explicitKeyword}{name}(");
        foreach (var property in node.Properties)
        {
            EmitGreenFieldType(writer, property);
            writer.Write($" {property.ParameterName}, ");
        }
        writer.WriteLine("DiagnosticInfoList diagnostics = {});");
        writer.WriteLine();

        writer.WriteLine($"~{name}() override;");

        writer.WriteLine();
        foreach (var property in node.Properties)
        {
            writer.Write("[[nodiscard]] constexpr ");
            EmitGreenGetterType(writer, property);
            var @override = property.IsOverride ? " override" : "";
            writer.WriteLine($" {property.GetterName}() const noexcept{@override}");
            using (writer.EnterBlockScope())
            {
                switch (property.Shape)
                {
                    case PropertyShape.Single:
                        writer.WriteLine($"return *{property.FieldName};");
                        break;
                    case PropertyShape.Optional:
                        writer.WriteLine($"return {property.FieldName}.get();");
                        break;
                    case PropertyShape.List or PropertyShape.SeparatedList:
                        writer.WriteLine($"return {property.FieldName};");
                        break;
                    default:
                        throw new InvalidOperationException("Unknown property shape");
                }
            }
            writer.WriteLine();
        }

        writer.WriteLine(
            $"[[nodiscard]] static constexpr bool instanceof(const {CommonNames.GreenNodeClass}& node) noexcept"
        );
        using (writer.EnterBlockScope())
        {
            writer.WriteLine(
                $"return node.kind() == {CommonNames.SyntaxKindClass}::{node.Kind!.CppName};"
            );
        }
        writer.WriteLine();
        writer.WriteLine(
            $"[[nodiscard]] Optional<const {CommonNames.GreenNodeClass}&> get_child(std::size_t index) const override;"
        );

        EmitGreenMutationDeclarations(writer, node, name);

        writer.WriteLine();
        writer.WriteAccessSpecifier(CppAccessSpecifier.Private);

        foreach (var property in node.Properties)
        {
            EmitGreenFieldType(writer, property);
            writer.WriteLine($" {property.FieldName};");
        }
    }

    private void EmitGreenMutationDeclarations(CodeWriter writer, CppNode node, string name)
    {
        var lastWasPublic = true;
        foreach (var property in node.Properties.AsValueEnumerable())
        {
            writer.WriteLine();
            if (property.IsOverride)
            {
                if (lastWasPublic)
                {
                    writer.WriteAccessSpecifier(CppAccessSpecifier.Protected);
                    lastWasPublic = false;
                }
            }
            else
            {
                if (!lastWasPublic)
                {
                    writer.WriteAccessSpecifier(CppAccessSpecifier.Public);
                    lastWasPublic = true;
                }
            }

            var paramName = property.IsOverride ? property.OverrideOf.Owner.GreenClassName : name;
            var core = property.IsOverride ? "_core" : "";
            writer.Write($"[[nodiscard]] GreenPtr<{paramName}> with_{property.GetterName}{core}(");
            EmitGreenFieldType(writer, property);
            var @override = property.IsOverride ? " override" : "";
            writer.WriteLine($" {property.ParameterName}) const{@override};");
        }

        if (!lastWasPublic)
            writer.WriteAccessSpecifier(CppAccessSpecifier.Public);

        writer.WriteLine();
        writer.Write($"[[nodiscard]] GreenPtr<{name}> update(");
        foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
        {
            if (i > 0)
                writer.Write(", ");

            EmitGreenFieldType(writer, property);
            writer.Write($" {property.ParameterName}");
        }
        writer.WriteLine(") const;");
    }

    #endregion

    #region Green Node Implementation
    public void EmitGreenNodeImplementation(CodeWriter writer, CppModule module)
    {
        var moduleName = module.CppName;
        writer.WriteLine(
            $"module {CommonNames.BaseModuleName}:{CommonNames.GreenFragmentName}.{moduleName}.impl;"
        );
        writer.WriteLine();
        writer.WriteLine($"import :{CommonNames.GreenFragmentName}.{moduleName};");
        foreach (var import in module.Dependencies.AsValueEnumerable().OrderBy(x => x.Name))
        {
            writer.WriteLine($"import :{CommonNames.GreenFragmentName}.{import.CppName};");
        }
        writer.WriteLine();
        using var namespaceScope = writer.EnterNamespaceScope(CommonNames.PrismNamespace);
        foreach (
            var (i, node) in module.Nodes.AsValueEnumerable().Where(n => !n.IsAbstract).Index()
        )
        {
            if (i > 0)
                writer.WriteLine();

            var nodeName = node.GreenClassName;
            EmitConcreteGreenNodeConstructor(writer, nodeName, node);

            writer.WriteLine();
            writer.WriteLine($"{nodeName}::~{nodeName}() = default;");

            writer.WriteLine();
            EmitGreenGetChildMethod(writer, node, nodeName);

            writer.WriteLine();
            EmitGreenUpdateMethod(writer, node, nodeName);
        }
    }

    private void EmitConcreteGreenNodeConstructor(CodeWriter writer, string nodeName, CppNode node)
    {
        writer.Write($"{nodeName}::{nodeName}(");
        foreach (var property in node.Properties)
        {
            EmitGreenFieldType(writer, property);
            writer.Write($" {property.ParameterName}, ");
        }
        writer.Write(
            $"DiagnosticInfoList diagnostics) : "
                + $"{node.Base.GreenClassName}{{{CommonNames.SyntaxKindClass}::{node.Kind!.CppName}, std::move(diagnostics)}}"
        );
        foreach (var property in node.Properties)
        {
            writer.Write($", {property.FieldName}{{std::move({property.ParameterName})}}");
        }
        writer.WriteLine();
        using var scope = writer.EnterBlockScope();
        writer.WriteLine($"set_child_count({node.Properties.Length});");
        foreach (var property in node.Properties)
        {
            if (property.Shape == PropertyShape.Optional)
                writer.Write($"if ({property.FieldName} != nullptr) ");
            writer.Write("adjust_flags_and_width(");
            switch (property.Shape)
            {
                case PropertyShape.Single or PropertyShape.Optional:
                    writer.Write($"*{property.FieldName}");
                    break;
                case PropertyShape.List or PropertyShape.SeparatedList:
                    writer.Write(property.FieldName);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            writer.WriteLine(");");
        }
    }

    private void EmitGreenGetChildMethod(CodeWriter writer, CppNode node, string nodeName)
    {
        writer.WriteLine(
            $"Optional<const {CommonNames.GreenNodeClass}&> {nodeName}::get_child(std::size_t index) const"
        );
        using var blockScope = writer.EnterBlockScope();
        writer.WriteLine("switch (index)");
        using var switchScope = writer.EnterBlockScope();
        foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
        {
            writer.WriteLine($"case {i}:");
            using var caseScope = writer.EnterIndentationScope();
            switch (property.Shape)
            {
                case PropertyShape.Single:
                    writer.WriteLine($"return *{property.FieldName};");
                    break;
                case PropertyShape.Optional:
                    writer.Write($"return {property.FieldName}.get();");
                    break;
                case PropertyShape.List or PropertyShape.SeparatedList:
                    writer.Write($"return {property.FieldName}.node();");
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
        writer.WriteLine("default:");
        writer.WriteLine("return std::nullopt;");
    }

    private void EmitGreenUpdateMethod(CodeWriter writer, CppNode node, string nodeName)
    {
        foreach (var property in node.Properties.AsValueEnumerable())
        {
            var paramName = property.IsOverride
                ? property.OverrideOf.Owner.GreenClassName
                : nodeName;
            var core = property.IsOverride ? "_core" : "";
            writer.Write(
                $"[[nodiscard]] GreenPtr<{paramName}> {nodeName}::with_{property.GetterName}{core}("
            );
            EmitGreenFieldType(writer, property);
            writer.WriteLine($" {property.ParameterName}) const");
            using (writer.EnterBlockScope())
            {
                writer.Write("return update(");
                foreach (var (i, arg) in node.Properties.AsValueEnumerable().Index())
                {
                    if (i > 0)
                        writer.Write(", ");

                    if (ReferenceEquals(property, arg))
                        writer.Write($"std::move({property.ParameterName})");
                    else
                        writer.Write(arg.FieldName);
                }
                writer.WriteLine(");");
            }

            writer.WriteLine();
        }

        writer.Write($"GreenPtr<{nodeName}> {nodeName}::update(");
        foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
        {
            if (i > 0)
                writer.Write(", ");

            EmitGreenFieldType(writer, property);
            writer.Write($" {property.ParameterName}");
        }
        writer.WriteLine(") const");
        using var scope = writer.EnterBlockScope();
        writer.Write("if (");
        foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
        {
            if (i > 0)
                writer.Write(" && ");

            writer.Write($"{property.ParameterName} == {property.FieldName}");
        }
        writer.WriteLine(')');
        using (writer.EnterIndentationScope())
            writer.WriteLine("return shared_from_this();");

        writer.WriteLine();
        writer.Write($"return make_ref_counted<const {nodeName}>(");
        foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
        {
            if (i > 0)
                writer.Write(", ");

            writer.Write($"std::move({property.ParameterName})");
        }

        writer.WriteLine(");");
    }
    #endregion

    private void EmitGreenGetterType(CodeWriter writer, CppProperty property)
    {
        switch (property.Shape)
        {
            case PropertyShape.Single:
                writer.Write($"const {property.Type.GreenClassName}&");
                break;
            case PropertyShape.Optional:
                writer.Write($"Optional<const {property.Type.GreenClassName}&>");
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

    private void EmitGreenFieldType(CodeWriter writer, CppProperty property)
    {
        switch (property.Shape)
        {
            case PropertyShape.Single or PropertyShape.Optional:
                writer.Write($"GreenPtr<{property.Type.GreenClassName}>");
                break;
            case PropertyShape.List:
                writer.Write($"GreenSyntaxList<{property.Type.GreenClassName}>");
                break;
            case PropertyShape.SeparatedList:
                writer.Write($"GreenSeparatedList<{property.Type.GreenClassName}>");
                break;
        }
    }
}
