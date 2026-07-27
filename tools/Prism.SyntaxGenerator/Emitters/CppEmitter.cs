// @file CppEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Prism.SyntaxGenerator.Models.Cpp;
using Prism.SyntaxGenerator.Models.Resolved;
using Prism.SyntaxGenerator.Models.Spec;
using Prism.SyntaxGenerator.Output;
using ZLinq;
using static Prism.SyntaxGenerator.Metadata.CommonNames;

namespace Prism.SyntaxGenerator.Emitters;

public static class CppEmitter
{
    extension(CodeWriter writer)
    {
        #region Syntax Kinds

        public void EmitSyntaxKinds(CppSyntaxModel model)
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

                foreach (var group in model.KindGroups)
                {
                    writer.WriteLine();
                    writer.WriteLineUnindented($"#pragma region {group.DisplayName}");
                    writer.WriteLine($"{group.CppName}_start = {group.StartValue},");
                    foreach (var kind in group.SyntaxKinds)
                    {
                        writer.WriteLine($"{kind.CppName} = {kind.Value},");
                    }

                    writer.WriteLine($"{group.CppName}_end = {group.EndValue},");
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
                foreach (var kind in model.Kinds)
                {
                    writer.WriteLine(
                        $"case {SyntaxKindClass}::{kind.CppName}: return \"{kind.DisplayName}\";"
                    );
                }

                writer.WriteLine(
                    $"default: throw std::invalid_argument{{\"Unknown {SyntaxKindClass}\"}};"
                );
            }

            foreach (var group in model.KindGroups)
            {
                writer.WriteLine();
                var groupName = group.CppName;
                writer.WriteLine(
                    $"export constexpr bool is_{groupName}(const {SyntaxKindClass} kind)"
                );
                using var block = writer.EnterBlockScope();
                writer.WriteLine(
                    $"return std::to_underlying(kind) >= std::to_underlying({SyntaxKindClass}::{groupName}_start) "
                        + $"&& std::to_underlying(kind) <= std::to_underlying({SyntaxKindClass}::{groupName}_end);"
                );
            }

            writer.WriteLine();
            writer.EmitIsSyntaxCategory(
                model,
                "structured_trivia",
                k => k.Kind == SyntaxGroupKind.StructuredTrivia
            );
            writer.WriteLine();
            writer.EmitIsSyntaxCategory(model, "token", k => k.Kind == SyntaxGroupKind.Token);
            writer.WriteLine();
            writer.EmitIsSyntaxCategory(model, "node", k => k.Kind == SyntaxGroupKind.Node);
        }

        private void EmitIsSyntaxCategory(
            CppSyntaxModel model,
            string groupName,
            Func<CppGroup, bool> predicate
        )
        {
            writer.WriteLine($"export constexpr bool is_{groupName}(const {SyntaxKindClass} kind)");
            using (writer.EnterBlockScope())
            {
                writer.Write("return ");
                var itemWritten = false;
                foreach (var kind in model.KindGroups.AsValueEnumerable().Where(predicate))
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

        public void EmitLexingUtils(CppSyntaxModel model)
        {
            writer.WriteLine($"export module {BaseModuleName}:syntax.lexing_utils;");
            writer.WriteLine();
            writer.WriteLine($"import :{GreenFragmentName}.token;");
            writer.WriteLine("import :text.cursor;");
            writer.WriteLine();
            using var scope = writer.EnterNamespaceScope(PrismNamespace);
            writer.EmitStaticTokenLookup(model);

            writer.WriteLine();
            writer.EmitKeywordLookup(model);

            writer.WriteLine();
            writer.EmitPunctuationTrie(model);
        }

        private void EmitStaticTokenLookup(CppSyntaxModel model)
        {
            writer.WriteLine(
                $"GreenPtr<{GreenTokenClass}> get_static_green_token(const {SyntaxKindClass} kind)"
            );
            using var scope = writer.EnterBlockScope();
            writer.EmitStaticGreenTokenBranch(
                model,
                "keyword",
                t => t.Category == TokenCategory.Keyword
            );

            writer.WriteLine();
            writer.EmitStaticGreenTokenBranch(
                model,
                "punctuation",
                t => t.Category == TokenCategory.Punctuation
            );

            writer.WriteLine();
            writer.WriteLine("return nullptr;");
        }

        private void EmitStaticGreenTokenBranch(
            CppSyntaxModel model,
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
                    foreach (var keyword in model.Tokens.AsValueEnumerable().Where(predicate))
                    {
                        writer.WriteLine(
                            $"make_ref_counted<{GreenTokenClass}>({SyntaxKindClass}::{keyword.Kind.CppName}),"
                        );
                    }
                }

                writer.WriteLine();
                writer.WriteLine(
                    $"return {category}_list[std::to_underlying(kind) - std::to_underlying({SyntaxKindClass}::{category}_start)];"
                );
            }
        }

        private void EmitKeywordLookup(CppSyntaxModel model)
        {
            writer.WriteLine(
                $"constexpr Optional<{SyntaxKindClass}> match_keyword(const std::string_view text)"
            );
            using var scope = writer.EnterBlockScope();
            writer.WriteLine("switch (text.size())");
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
                            $"if (text == \"{keyword.Text!}\") return {SyntaxKindClass}::{keyword.Kind.CppName};"
                        );
                    }

                    writer.WriteLine("break;");
                }
            }

            writer.WriteLine();
            writer.WriteLine("return std::nullopt;");
        }

        private void EmitPunctuationTrie(CppSyntaxModel model)
        {
            var trie = ConstructPunctuationTrie(model);
            writer.WriteLine(
                $"constexpr Optional<{SyntaxKindClass}> match_punctuation(TextCursor &cursor)"
            );
            using var block = writer.EnterBlockScope();
            writer.WriteTrie(trie);
            writer.WriteLine();
            writer.WriteLine("return std::nullopt;");
        }

        private void WriteTrie(TrieNode node)
        {
            switch (node.Children.Count)
            {
                case 1:
                {
                    var (character, child) = node.Children.Single();
                    writer.WriteLine($"if (cursor.current() == '{character}')");
                    using var block = writer.EnterBlockScope();
                    writer.WriteLine("cursor.advance();");
                    writer.WriteTrie(child);
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
                        writer.WriteTrie(child);
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

        public void EmitGreenNodeInterface(CppModule module)
        {
            writer.WriteLine(
                $"export module {BaseModuleName}:{GreenFragmentName}.{module.CppName};"
            );
            writer.WriteLine();
            writer.WriteLine($"import :{GreenFragmentName}.node;");
            writer.WriteLine($"import :{GreenFragmentName}.token;");
            writer.WriteLine($"import :{GreenFragmentName}.trivia;");
            writer.WriteLine($"import :{GreenFragmentName}.separated_list;");
            writer.WriteLine();
            using var scope = writer.EnterNamespaceScope(PrismNamespace);
            if (module.ForwardDeclarations.Count > 0)
            {
                foreach (
                    var declaration in module
                        .ForwardDeclarations.AsValueEnumerable()
                        .OrderBy(x => x.GreenClassName)
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

                writer.EmitGreenNodeClassDeclaration(node);
                if (node.IsAbstract)
                    continue;
                writer.WriteLine();
                writer.EmitGreenNodeTraits(node);
            }
        }

        private void EmitGreenNodeClassDeclaration(CppNode node)
        {
            var final = !node.IsAbstract ? " final" : "";
            var baseName =
                node.Base?.GreenClassName
                ?? node.Module.Kind switch
                {
                    ModuleKind.Node => GreenNodeClass,
                    ModuleKind.StructuredTrivia => GreenStructuredTriviaClass,
                    _ => throw new InvalidOperationException("Invalid module kind"),
                };
            writer.WriteLine($"class {node.GreenClassName}{final} : public {baseName}");
            using var scope = writer.EnterBlockScope(true);
            if (node.IsAbstract)
            {
                writer.EmitGreenNodeAbstractClassBody(node, baseName);
            }
            else
            {
                writer.EmitGreenNodeConcreteClassBody(node);
            }
        }

        private void EmitGreenNodeAbstractClassBody(CppNode node, string baseName)
        {
            using (writer.EnterIndentationScope(-1))
            {
                writer.WriteLine("protected:");
            }

            writer.WriteLine(
                $"explicit constexpr {node.GreenClassName}(const {SyntaxKindClass} kind, "
                    + $"DiagnosticInfoList diagnostics = {{}}) : "
                    + $"{baseName}{{kind, std::move(diagnostics)}} {{ }}"
            );
            writer.WriteLine();

            writer.WriteAccessSpecifier(CppAccessSpecifier.Public);
            foreach (var property in node.Properties)
            {
                writer.Write("[[nodiscard]] virtual ");
                writer.EmitGreenGetterType(property);
                writer.WriteLine($" {property.GetterName}() const noexcept = 0;");
                writer.Write($"virtual void set_{property.GetterName}(");
                writer.EmitGreenFieldType(property);
                writer.WriteLine(" value) noexcept = 0;");
            }
            writer.WriteLine();

            writer.WriteLine(
                $"[[nodiscard]] static constexpr bool instance_of(const {GreenNodeClass}& node) noexcept"
            );
            writer.EmitAbstractInstanceOfMethod(node);

            if (node.Properties.Length == 0)
                return;

            writer.WriteLine();
            foreach (var property in node.Properties)
            {
                writer.WriteLine("template <typename Self>");
                writer.Write(
                    $"[[nodiscard]] constexpr GreenPtr<std::decay_t<Self>> with_{property.GetterName}(this const Self& self, "
                );
                writer.EmitGreenFieldType(property);
                writer.WriteLine($"{property.ParameterName})");
                using (writer.EnterBlockScope())
                {
                    writer.WriteLine(
                        $"return static_pointer_cast<const std::decay_t<Self>>(self.with_{property.GetterName}_core(std::move({property.ParameterName})));"
                    );
                }

                writer.WriteLine();
            }

            foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
            {
                if (i > 0)
                    writer.WriteLine();

                writer.Write(
                    $"[[nodiscard]] virtual GreenPtr<{node.GreenClassName}> with_{property.GetterName}_core("
                );
                writer.EmitGreenFieldType(property);
                writer.WriteLine($"{property.ParameterName}) const = 0;");
            }
        }

        private void EmitGreenNodeConcreteClassBody(CppNode node)
        {
            writer.WriteAccessSpecifier(CppAccessSpecifier.Public);

            var explicitKeyword = node.Properties.Length <= 1 ? "explicit " : "";
            writer.Write($"{explicitKeyword}{node.GreenClassName}(");
            foreach (var property in node.Properties)
            {
                writer.EmitGreenFieldType(property);
                writer.Write($" {property.ParameterName}, ");
            }

            writer.WriteLine("DiagnosticInfoList diagnostics = {});");
            writer.WriteLine();

            writer.WriteLine($"~{node.GreenClassName}() override;");

            writer.WriteLine();
            foreach (var property in node.Properties)
            {
                var @override = property.IsOverride ? " override" : "";
                writer.EmitInlineGetter(property);

                writer.WriteLine();
                writer.Write($"void set_{property.GetterName}(");
                writer.EmitGreenFieldType(property);
                writer.WriteLine($" value) noexcept{@override};");
                writer.WriteLine();
            }

            writer.WriteLine(
                $"[[nodiscard]] static constexpr bool instance_of(const {GreenNodeClass}& node) noexcept"
            );
            writer.EmitConcreteInstanceOfMethod(node);

            writer.WriteLine();
            writer.WriteLine(
                $"[[nodiscard]] Optional<const {GreenNodeClass}&> get_slot(std::size_t index) const override;"
            );

            writer.WriteLine();
            writer.WriteLine(
                $"[[nodiscard]] {SyntaxNodeClass} &create_red({SyntaxLifetimeClass} &lifetime, "
                    + $"const {SyntaxNodeClass} *parent, std::uint32_t position) const override;"
            );

            writer.EmitGreenMutationDeclarations(node);
            writer.WriteLine(
                $"[[nodiscard]] RefCountPtr<{GreenNodeClass}> clone_internal() const override;"
            );

            writer.WriteLine();
            writer.WriteAccessSpecifier(CppAccessSpecifier.Private);

            foreach (var property in node.Properties)
            {
                writer.EmitGreenFieldType(property);
                writer.WriteLine($" {property.FieldName};");
            }
        }

        private void EmitGreenNodeTraits(CppNode node)
        {
            writer.WriteLine("template <>");
            writer.WriteLine($"struct GreenNodeTraits<{node.GreenClassName}>");
            using var scope = writer.EnterBlockScope(true);
            writer.WriteLine(
                $"static constexpr std::size_t slot_count = {node.Properties.Length};"
            );
            writer.WriteLine();
            writer.Write("using ChildTypes = std::tuple<");
            foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
            {
                if (i > 0)
                    writer.Write(", ");

                writer.EmitGreenRawType(property);
            }
            writer.WriteLine(">;");
            writer.WriteLine();
            writer.WriteLine("template <std::size_t N>");
            using (writer.EnterIndentationScope())
            {
                writer.WriteLine("requires (N < slot_count)");
            }
            writer.WriteLine(
                $"static constexpr decltype(auto) get(const {node.GreenClassName}& node)"
            );
            using (writer.EnterBlockScope())
            {
                foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
                {
                    if (i > 0)
                        writer.Write("else ");

                    if (i < node.Properties.Length - 1)
                        writer.WriteLine($"if constexpr (N == {i})");
                    using var blockScope = writer.EnterBlockScope();
                    if (i == node.Properties.Length - 1)
                        writer.WriteLine($"static_assert(N == {i});");
                    writer.WriteLine($"return node.{property.GetterName}();");
                }
            }

            writer.WriteLine();
            writer.WriteLine(
                $"template <std::size_t N, std::convertible_to<GreenSetterParam<N, {node.GreenClassName}>> Arg>"
            );
            using (writer.EnterIndentationScope())
            {
                writer.WriteLine("requires (N < slot_count)");
            }
            writer.WriteLine(
                $"static constexpr void set({node.GreenClassName}& node, Arg&& value)"
            );
            using (writer.EnterBlockScope())
            {
                foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
                {
                    if (i > 0)
                        writer.Write("else ");

                    if (i < node.Properties.Length - 1)
                        writer.WriteLine($"if constexpr (N == {i})");
                    using var blockScope = writer.EnterBlockScope();
                    if (i == node.Properties.Length - 1)
                        writer.WriteLine($"static_assert(N == {i});");
                    writer.WriteLine($"node.set_{property.GetterName}(std::forward<Arg>(value));");
                }
            }

            writer.WriteLine();
            writer.WriteLine(
                $"template <std::size_t N, std::convertible_to<GreenSetterParam<N, {node.GreenClassName}>> Arg>"
            );
            using (writer.EnterIndentationScope())
            {
                writer.WriteLine("requires (N < slot_count)");
            }
            writer.WriteLine(
                $"static constexpr GreenPtr<{node.GreenClassName}> with(const {node.GreenClassName}& node, Arg&& value)"
            );
            using (writer.EnterBlockScope())
            {
                foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
                {
                    if (i > 0)
                        writer.Write("else ");

                    if (i < node.Properties.Length - 1)
                        writer.WriteLine($"if constexpr (N == {i})");
                    using var blockScope = writer.EnterBlockScope();
                    if (i == node.Properties.Length - 1)
                        writer.WriteLine($"static_assert(N == {i});");
                    writer.WriteLine(
                        $"return node.with_{property.GetterName}(std::forward<Arg>(value));"
                    );
                }
            }
        }

        private void EmitInlineGetter(CppProperty property)
        {
            writer.Write("[[nodiscard]] constexpr ");
            writer.EmitGreenGetterType(property);
            var @override = property.IsOverride ? " override" : "";
            writer.WriteLine($" {property.GetterName}() const noexcept{@override}");
            using var scope = writer.EnterBlockScope();
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

        private void EmitGreenMutationDeclarations(CppNode node)
        {
            foreach (var property in node.Properties.AsValueEnumerable())
            {
                writer.WriteLine();
                var paramName = property.IsOverride
                    ? property.OverrideOf.Owner.GreenClassName
                    : node.GreenClassName;
                var core = property.IsOverride ? "_core" : "";
                writer.Write(
                    $"[[nodiscard]] GreenPtr<{paramName}> with_{property.GetterName}{core}("
                );
                writer.EmitGreenFieldType(property);
                var @override = property.IsOverride ? " override" : "";
                writer.WriteLine($" {property.ParameterName}) const{@override};");
            }

            writer.WriteLine();
            writer.Write($"[[nodiscard]] GreenPtr<{node.GreenClassName}> update(");
            foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
            {
                if (i > 0)
                    writer.Write(", ");

                writer.EmitGreenFieldType(property);
                writer.Write($" {property.ParameterName}");
            }

            writer.WriteLine(") const;");
        }

        #endregion

        #region Green Node Implementation

        public void EmitGreenNodeImplementation(CppModule module)
        {
            var moduleName = module.CppName;
            writer.WriteLine($"module {BaseModuleName}:{GreenFragmentName}.{moduleName}.impl;");
            writer.WriteLine();
            writer.WriteLine("import :syntax.lifetime;");
            writer.WriteLine($"import :{GreenFragmentName}.{moduleName};");
            writer.WriteLine($"import :{RedFragmentName}.{moduleName};");
            foreach (var import in module.Dependencies.AsValueEnumerable().OrderBy(x => x.CppName))
            {
                writer.WriteLine($"import :{GreenFragmentName}.{import.CppName};");
            }

            writer.WriteLine();
            using var namespaceScope = writer.EnterNamespaceScope(PrismNamespace);
            foreach (
                var (i, node) in module.Nodes.AsValueEnumerable().Where(n => !n.IsAbstract).Index()
            )
            {
                if (i > 0)
                    writer.WriteLine();

                writer.EmitConcreteGreenNodeConstructor(node);
                writer.WriteLine();

                writer.WriteLine($"{node.GreenClassName}::~{node.GreenClassName}() = default;");

                writer.WriteLine();
                writer.EmitGreenSetters(node);

                writer.WriteLine();
                writer.EmitGreenGetChildMethod(node);

                writer.WriteLine();
                writer.EmitGreenCreateRedMethod(node);

                writer.WriteLine();
                writer.EmitGreenUpdateMethod(node);

                writer.WriteLine();
                writer.EmitGreenCloneMethod(node);
            }
        }

        private void EmitConcreteGreenNodeConstructor(CppNode node)
        {
            writer.Write($"{node.GreenClassName}::{node.GreenClassName}(");
            foreach (var property in node.Properties)
            {
                writer.EmitGreenFieldType(property);
                writer.Write($" {property.ParameterName}, ");
            }

            var baseName =
                node.Base?.GreenClassName
                ?? node.Module.Kind switch
                {
                    ModuleKind.Node => GreenNodeClass,
                    ModuleKind.StructuredTrivia => GreenStructuredTriviaClass,
                    _ => throw new InvalidOperationException("Invalid module kind"),
                };

            writer.Write(
                $"DiagnosticInfoList diagnostics) : "
                    + $"{baseName}{{{SyntaxKindClass}::{node.Kind!.CppName}, std::move(diagnostics)}}"
            );
            foreach (var property in node.Properties)
            {
                writer.Write($", {property.FieldName}{{std::move({property.ParameterName})}}");
            }

            writer.WriteLine();
            using var scope = writer.EnterBlockScope();
            writer.WriteLine($"set_slot_count({node.Properties.Length});");
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

        private void EmitGreenSetters(CppNode node)
        {
            foreach (var property in node.Properties)
            {
                writer.Write($"void {node.GreenClassName}::set_{property.GetterName}(");
                writer.EmitGreenFieldType(property);
                writer.WriteLine(" value) noexcept");
                using (writer.EnterBlockScope())
                {
                    writer.WriteLine($"{property.FieldName} = std::move(value);");
                }
                writer.WriteLine();
            }
        }

        private void EmitGreenGetChildMethod(CppNode node)
        {
            writer.WriteLine(
                $"Optional<const {GreenNodeClass}&> {node.GreenClassName}::get_slot(std::size_t index) const"
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

        private void EmitGreenCreateRedMethod(CppNode node)
        {
            writer.Write(
                $"[[nodiscard]] {SyntaxNodeClass} &{node.GreenClassName}::create_red({SyntaxLifetimeClass} &lifetime, "
                    + $"const {SyntaxNodeClass} *parent, std::uint32_t position) const"
            );
            using var scope = writer.EnterBlockScope();
            writer.WriteLine($"return lifetime.add<{node.RedClassName}>(*this, parent, position);");
        }

        private void EmitGreenUpdateMethod(CppNode node)
        {
            foreach (var property in node.Properties.AsValueEnumerable())
            {
                var paramName = property.IsOverride
                    ? property.OverrideOf.Owner.GreenClassName
                    : node.GreenClassName;
                var core = property.IsOverride ? "_core" : "";
                writer.Write(
                    $"[[nodiscard]] GreenPtr<{paramName}> {node.GreenClassName}::with_{property.GetterName}{core}("
                );
                writer.EmitGreenFieldType(property);
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

            writer.Write($"GreenPtr<{node.GreenClassName}> {node.GreenClassName}::update(");
            foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
            {
                if (i > 0)
                    writer.Write(", ");

                writer.EmitGreenFieldType(property);
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
            writer.Write($"return make_ref_counted<const {node.GreenClassName}>(");
            foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
            {
                if (i > 0)
                    writer.Write(", ");

                writer.Write($"std::move({property.ParameterName})");
            }

            writer.WriteLine(");");
        }

        private void EmitGreenCloneMethod(CppNode node)
        {
            writer.WriteLine(
                $"RefCountPtr<{GreenNodeClass}> {node.GreenClassName}::clone_internal() const"
            );
            using var scope = writer.EnterBlockScope();
            writer.Write($"return make_ref_counted<{node.GreenClassName}>(");
            foreach (var (i, property) in node.Properties.AsValueEnumerable().Index())
            {
                if (i > 0)
                    writer.Write(", ");

                writer.Write(property.FieldName);
            }

            writer.WriteLine(");");
        }

        #endregion

        private void EmitGreenGetterType(CppProperty property)
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
                    writer.Write($"const GreenSyntaxList<{property.Type.GreenClassName}>&");
                    break;
                case PropertyShape.SeparatedList:
                    writer.Write($"const GreenSeparatedList<{property.Type.GreenClassName}>&");
                    break;
                default:
                    throw new InvalidOperationException("Unknown shape");
            }
        }

        private void EmitGreenRawType(CppProperty property)
        {
            switch (property.Shape)
            {
                case PropertyShape.Single or PropertyShape.Optional:
                    writer.Write($"{property.Type.GreenClassName}");
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

        private void EmitGreenFieldType(CppProperty property)
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

        #region Green Node Vistor

        public void EmitGreenVisitorFunctions(CppSyntaxModel model)
        {
            writer.WriteLine("module;");
            writer.WriteLine();
            writer.WriteLine("#include <libassert/assert-macros.hpp>");
            writer.WriteLine();
            writer.WriteLine($"export module {BaseModuleName}:{GreenFragmentName}.visit;");
            writer.WriteLine();
            writer.WriteLine("import libassert;");
            writer.WriteLine("import :type_traits.visitor;");
            foreach (var module in model.Modules)
            {
                writer.WriteLine($"import :{GreenFragmentName}.{module.CppName};");
            }
            writer.WriteLine();
            using var namespaceScope = writer.EnterNamespaceScope(PrismNamespace);
            foreach (var group in model.DispatchGroups)
            {
                writer.EmitGreenVisitorGroup(group);
                writer.WriteLine();
            }

            foreach (
                var node in model
                    .Modules.AsValueEnumerable()
                    .SelectMany(m => m.Nodes)
                    .Where(n => !n.IsAbstract)
            )
            {
                writer.WriteLine(
                    $"template <std::invocable<const {node.GreenClassName}&> Functor>"
                );
                writer.WriteLine(
                    $"constexpr decltype(auto) visit(const {node.GreenClassName}& node, Functor&& functor)"
                );
                using (writer.EnterBlockScope())
                {
                    writer.WriteLine("return std::invoke(std::forward<Functor>(functor), node);");
                }
                writer.WriteLine();

                writer.WriteLine($"template <std::invocable<{node.GreenClassName}&> Functor>");
                writer.WriteLine(
                    $"constexpr decltype(auto) visit({node.GreenClassName}& node, Functor&& functor)"
                );
                using (writer.EnterBlockScope())
                {
                    writer.WriteLine("return std::invoke(std::forward<Functor>(functor), node);");
                }
                writer.WriteLine();
            }

            writer.EmitSyntaxVisitorClass(model, isGreen: true);
            writer.WriteLine();
            writer.EmitSyntaxVisitorClass(model, isGreen: true, isConst: false);
        }

        private void EmitGreenVisitorGroup(CppDispatchGroup group)
        {
            writer.EmitSyntaxVisitor(group, isGreen: true, isReturning: false);
            writer.WriteLine();
            writer.EmitSyntaxVisitor(group, isGreen: true, isReturning: false, isConst: false);
            writer.WriteLine();
            writer.EmitSyntaxVisitor(group, isGreen: true, isReturning: true);
            writer.WriteLine();
            writer.EmitSyntaxVisitor(group, isGreen: true, isReturning: true, isConst: false);
        }
        #endregion

        private void EmitSyntaxVisitor(
            CppDispatchGroup group,
            bool isGreen,
            bool isReturning,
            bool isConst = true
        )
        {
            var mutablePrefix = isConst ? "" : "Mutable";
            var constQualifier = isConst ? "const " : "";
            var export = isGreen ? "" : "export ";
            var baseName = isGreen ? group.GreenClassName : group.RedClassName;
            var conceptName = isReturning ? "ConvertibleVisitor" : "ExhaustiveVisitor";
            var returnValueParam = isReturning ? ", typename R" : "";
            var returnValueArg = isReturning ? ", R" : "";
            var returningSuffix = isReturning ? "Returning" : "";

            writer.WriteLine($"{export}template <typename Functor{returnValueParam}>");
            writer.Write(
                $"concept {mutablePrefix}VisitorFor{baseName}{returningSuffix} = {conceptName}<Functor{returnValueArg}"
            );
            if (group.IncludesListNode && isGreen)
            {
                writer.Write($", {constQualifier}{GreenListNodeClass}&");
            }
            foreach (
                var className in group
                    .Nodes.AsValueEnumerable()
                    .Select(node => isGreen ? node.GreenClassName : node.RedClassName)
            )
            {
                writer.Write($", {constQualifier}{className}&");
            }
            writer.WriteLine(">;");
            writer.WriteLine();

            returnValueParam = isReturning ? "typename R, " : "";
            writer.WriteLine(
                $"{export}template <{returnValueParam}{mutablePrefix}VisitorFor{baseName}{returningSuffix} Functor>"
            );

            var returnValueType = isReturning ? "R" : "decltype(auto)";
            writer.WriteLine(
                $"constexpr {returnValueType} visit({constQualifier}{baseName}& node, Functor&& functor)"
            );
            using var functionScope = writer.EnterBlockScope();
            writer.WriteLine("switch (node.kind())");
            using var switchScope = writer.EnterBlockScope();
            if (group.IncludesListNode && isGreen)
            {
                writer.WriteLine("case SyntaxKind::list:");
                using var caseScope = writer.EnterIndentationScope();
                writer.WriteLine(
                    $"return std::invoke(std::forward<Functor>(functor), static_cast<{constQualifier}GreenListNode&>(node));"
                );
            }

            foreach (var node in group.Nodes)
            {
                Debug.Assert(node.Kind is not null);
                writer.WriteLine($"case SyntaxKind::{node.Kind.CppName}:");
                using var caseScope = writer.EnterIndentationScope();
                var className = isGreen ? node.GreenClassName : node.RedClassName;
                writer.WriteLine(
                    $"return std::invoke(std::forward<Functor>(functor), static_cast<{constQualifier}{className}&>(node));"
                );
            }
            writer.WriteLine("default:");
            using var defaultScope = writer.EnterIndentationScope();
            if (isGreen && group.IncludesListNode)
            {
                writer.WriteLine("if (node.is_token())");
                using (writer.EnterBlockScope())
                {
                    writer.WriteLine(
                        $"return std::invoke(std::forward<Functor>(functor), static_cast<{constQualifier}{GreenTokenClass}&>(node));"
                    );
                }
                writer.WriteLine();

                writer.WriteLine("if (node.is_trivia())");
                using (writer.EnterBlockScope())
                {
                    writer.WriteLine(
                        $"return std::invoke(std::forward<Functor>(functor), static_cast<{constQualifier}{GreenTriviaClass}&>(node));"
                    );
                }
                writer.WriteLine();
            }
            writer.WriteLine("UNREACHABLE(\"Invalid node type passed into visit\");");
        }

        private void EmitSyntaxVisitorClass(CppSyntaxModel model, bool isGreen, bool isConst = true)
        {
            var nodeClassName = isGreen ? GreenNodeClass : SyntaxNodeClass;
            var export = isGreen ? "" : "export ";
            var interfaceName = isGreen ? "GreenSyntaxVisitor" : "SyntaxVisitor";
            var mutablePrefix = isConst ? "" : "Mutable";
            var constQualifier = isConst ? "const " : "";
            writer.WriteLine($"{export}template <typename R = void>");
            writer.WriteLine($"class {mutablePrefix}{interfaceName}");
            using (writer.EnterBlockScope(true))
            {
                writer.WriteAccessSpecifier(CppAccessSpecifier.Public);
                writer.WriteLine($"virtual ~{mutablePrefix}{interfaceName}() = default;");
                writer.WriteLine();
                writer.WriteLine($"template <std::derived_from<{nodeClassName}> T, typename Self>");
                writer.WriteLine($"constexpr R visit(this Self&& self, {constQualifier}T& node)");
                using (writer.EnterBlockScope())
                {
                    writer.WriteLine("return prism::visit(node, std::forward<Self>(self));");
                }
                writer.WriteLine();
                writer.EmitVisitorMethodList(model, isGreen, constQualifier, "virtual ", " = 0;");
            }
            writer.WriteLine();
            writer.WriteLine($"{export} template <typename R = void>");
            using (writer.EnterIndentationScope())
            {
                writer.WriteLine(
                    "requires (std::same_as<R, void> || std::is_default_constructible_v<R>)"
                );
            }
            writer.WriteLine(
                $"class {mutablePrefix}{interfaceName}Base : public {mutablePrefix}{interfaceName}<R>"
            );
            using (writer.EnterBlockScope(true))
            {
                writer.WriteAccessSpecifier(CppAccessSpecifier.Public);
                writer.EmitVisitorMethodList(
                    model,
                    isGreen,
                    constQualifier,
                    "constexpr ",
                    " override",
                    static w =>
                    {
                        using var scope = w.EnterBlockScope();
                        w.WriteLine("if constexpr (!std::same_as<R, void>)");
                        using (w.EnterBlockScope())
                        {
                            w.WriteLine("return R{};");
                        }
                        w.WriteLine("else");
                        using (w.EnterBlockScope())
                        {
                            // This extra return is just to shut up IntelliSense
                            w.WriteLine("return;");
                        }
                    },
                    static w => w.WriteLine()
                );
            }
        }

        private void EmitVisitorMethodList(
            CppSyntaxModel model,
            bool isGreen,
            string constQualifier,
            string prefix,
            string suffix,
            Action<CodeWriter>? emitBody = null,
            Action<CodeWriter>? optionalLineBreak = null
        )
        {
            if (isGreen)
            {
                writer.WriteLine(
                    $"{prefix}R operator()({constQualifier}{GreenListNodeClass}& node) const{suffix}"
                );
                emitBody?.Invoke(writer);
                optionalLineBreak?.Invoke(writer);
                writer.WriteLine(
                    $"{prefix}R operator()({constQualifier}{GreenTokenClass}& node) const{suffix}"
                );
                emitBody?.Invoke(writer);
                optionalLineBreak?.Invoke(writer);
                writer.WriteLine(
                    $"{prefix}R operator()({constQualifier}{GreenTriviaClass}& node) const{suffix}"
                );
                emitBody?.Invoke(writer);
            }

            foreach (
                var node in model
                    .Modules.AsValueEnumerable()
                    .SelectMany(m => m.Nodes)
                    .Where(n => !n.IsAbstract)
            )
            {
                optionalLineBreak?.Invoke(writer);
                var className = isGreen ? node.GreenClassName : node.RedClassName;
                writer.WriteLine(
                    $"{prefix}R operator()({constQualifier}{className}& node) const{suffix}"
                );
                emitBody?.Invoke(writer);
            }
        }

        #region Red Node Interface

        public void EmitRedNodeInterface(CppModule module)
        {
            writer.WriteLine("module;");
            writer.WriteLine();
            writer.WriteLine($"#include \"{PrismCoreExportHeader}\"");
            writer.WriteLine();
            writer.WriteLine($"export module {BaseModuleName}:{RedFragmentName}.{module.CppName};");
            writer.WriteLine();
            writer.WriteLine($"import :{RedFragmentName}.node;");
            writer.WriteLine($"import :{RedFragmentName}.token;");
            writer.WriteLine($"import :{RedFragmentName}.trivia;");
            writer.WriteLine($"import :{RedFragmentName}.list;");
            writer.WriteLine($"import :{GreenFragmentName}.{module.CppName};");
            writer.WriteLine("");

            using var namespaceScope = writer.EnterNamespaceScope(PrismNamespace);
            foreach (
                var declaration in module
                    .ForwardDeclarations.AsValueEnumerable()
                    .OrderBy(x => x.RedClassName)
            )
            {
                var export = declaration.Module == module ? "export " : "";
                writer.WriteLine($"{export}class {declaration.RedClassName};");
            }

            if (module.ForwardDeclarations.Count > 0)
                writer.WriteLine();

            foreach (var (i, node) in module.Nodes.AsValueEnumerable().Index())
            {
                if (i > 0)
                    writer.WriteLine();

                writer.EmitRedNodeDeclaration(node);
            }
        }

        private void EmitRedNodeDeclaration(CppNode node)
        {
            var export = node.Module.ForwardDeclarations.Contains(node) ? "" : "export ";
            var final = node.IsAbstract ? "" : " final";
            var baseName =
                node.Base?.RedClassName
                ?? node.Module.Kind switch
                {
                    ModuleKind.Node => SyntaxNodeClass,
                    ModuleKind.StructuredTrivia => StructuredTriviaSyntaxClass,
                    _ => throw new InvalidOperationException("Invalid module kind"),
                };
            writer.WriteLine(
                $"{export}class {PrismCoreExport} {node.RedClassName}{final} : public {baseName}"
            );
            using var scope = writer.EnterBlockScope(true);
            writer.WriteAccessSpecifier(
                node.IsAbstract ? CppAccessSpecifier.Protected : CppAccessSpecifier.Public
            );

            writer.WriteLine(
                $"constexpr {node.RedClassName}({SyntaxLifetimeClass}& lifetime, const {node.GreenClassName}& node, const {SyntaxNodeClass}* parent, const std::uint32_t position) : "
                    + $"{baseName}{{lifetime, node, parent, position}} {{ }}"
            );

            writer.WriteLine();
            if (node.IsAbstract)
            {
                writer.WriteLine($"~{node.RedClassName}() = default;");
            }

            if (node.IsAbstract)
            {
                writer.WriteAccessSpecifier(CppAccessSpecifier.Public);
            }

            foreach (var property in node.Properties)
            {
                var @virtual = node.IsAbstract ? " virtual " : "";
                writer.Write($"[[nodiscard]] {@virtual}");
                writer.EmitRedGetterType(property);
                var @override = property.IsOverride ? " override" : "";
                var noImpl = node.IsAbstract ? " = 0" : "";
                writer.WriteLine($" {property.Name}() const{@override}{noImpl};");
            }

            if (node.Properties.Length > 0)
                writer.WriteLine();

            writer.WriteLine(
                $"[[nodiscard]] static constexpr bool instance_of(const {SyntaxNodeClass} &node) noexcept"
            );
            if (node.IsAbstract)
                writer.EmitAbstractInstanceOfMethod(node);
            else
                writer.EmitConcreteInstanceOfMethod(node);

            if (node.IsAbstract)
                return;

            writer.WriteLine();
            writer.WriteAccessSpecifier(CppAccessSpecifier.Protected);
            writer.WriteLine(
                $"[[nodiscard]] Optional<const {SyntaxNodeClass} &> get_node_slot(std::size_t index) const override;"
            );
            writer.WriteLine(
                $"[[nodiscard]] Optional<const {SyntaxNodeClass} &> get_cached_slot(std::size_t index) const override;"
            );

            foreach (
                var (i, property) in node
                    .Properties.AsValueEnumerable()
                    .Where(p => p.Type.Name != "Token")
                    .Index()
            )
            {
                if (i == 0)
                {
                    writer.WriteLine();
                    writer.WriteAccessSpecifier(CppAccessSpecifier.Private);
                }

                writer.Write("mutable ");
                writer.EmitRedFieldType(property);
                writer.WriteLine($" {property.FieldName};");
            }
        }

        #endregion

        #region Red Node Implementation

        public void EmitRedNodeImplementation(CppModule module)
        {
            var moduleName = module.CppName;
            writer.WriteLine($"module {BaseModuleName}:{RedFragmentName}.{moduleName}.impl;");
            writer.WriteLine();
            writer.WriteLine($"import :{RedFragmentName}.{moduleName};");
            foreach (var import in module.Dependencies.AsValueEnumerable().OrderBy(x => x.CppName))
            {
                writer.WriteLine($"import :{RedFragmentName}.{import.CppName};");
            }

            writer.WriteLine();
            using var namespaceScope = writer.EnterNamespaceScope(PrismNamespace);

            foreach (
                var (i, node) in module.Nodes.AsValueEnumerable().Where(x => !x.IsAbstract).Index()
            )
            {
                if (i > 0)
                    writer.WriteLine();

                foreach (var (j, property) in node.Properties.AsValueEnumerable().Index())
                {
                    writer.EmitRedNodePropertyGetter(property, j);
                    writer.WriteLine();
                }

                writer.EmitGetNodeSlot(node);
                writer.WriteLine();
                writer.EmitGetCachedSlot(node);
            }
        }

        private void EmitRedNodePropertyGetter(CppProperty property, int index)
        {
            writer.EmitRedGetterType(property);
            writer.WriteLine($" {property.Owner.RedClassName}::{property.GetterName}() const");
            using var scope = writer.EnterBlockScope();
            if (property.Type.Name == "Token")
            {
                switch (property.Shape)
                {
                    case PropertyShape.Single:
                        writer.Write($"return {SyntaxTokenClass}{{");
                        writer.EmitInvokeGreenGetter(property);
                        writer.Write(", this, ");
                        if (index == 0)
                            writer.WriteLine("position()};");
                        else
                            writer.WriteLine($"get_slot_position({index})}};");
                        break;
                    case PropertyShape.Optional:
                        writer.Write("return ");
                        writer.EmitInvokeGreenGetter(property);
                        writer.Write(
                            $".transform([this](const {property.Type.GreenClassName} & g) {{ "
                                + $"return {SyntaxTokenClass}{{g, this, "
                        );
                        if (index == 0)
                            writer.WriteLine("position()}; });");
                        else
                            writer.WriteLine($"get_slot_position({index})}}; }});");
                        break;
                    case PropertyShape.List:
                        writer.Write("return SyntaxTokenList{this, ");
                        writer.EmitInvokeGreenGetter(property);
                        if (index == 0)
                            writer.WriteLine(", position()};");
                        else
                            writer.WriteLine($", get_slot_position({index})}};");
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
                    case PropertyShape.Single:
                        writer.Write($"return *get_red({property.FieldName}");
                        if (index > 0)
                            writer.Write($", {index}");
                        writer.WriteLine(");");
                        break;
                    case PropertyShape.Optional:
                        writer.Write($"return get_red({property.FieldName}");
                        if (index > 0)
                            writer.Write($", {index}");
                        writer.WriteLine(");");
                        break;
                    case PropertyShape.List:
                        writer.Write($"const auto red = get_red({property.FieldName}");
                        if (index > 0)
                            writer.Write($", {index}");
                        writer.WriteLine(");");
                        writer.WriteLine(
                            $"return make_syntax_list<{property.Type.RedClassName}>(red);"
                        );
                        break;
                    case PropertyShape.SeparatedList:
                        writer.Write($"const auto red = get_red({property.FieldName}");
                        if (index > 0)
                            writer.Write($", {index}");
                        writer.WriteLine(");");
                        writer.WriteLine(
                            $"return make_separated_syntax_list<{property.Type.RedClassName}>(red);"
                        );
                        break;
                    default:
                        throw new InvalidOperationException("Unknown shape");
                }
            }
        }

        private void EmitInvokeGreenGetter(CppProperty property)
        {
            writer.Write(
                $"static_cast<const {property.Owner.GreenClassName} &>(green()).{property.GetterName}()"
            );
        }

        private void EmitGetNodeSlot(CppNode node)
        {
            using var targetNodes = node
                .Properties.AsValueEnumerable()
                .Index()
                .Where(p => p.Item.Type.Name != "Token")
                .ToArrayPool();
            var targetSpan = targetNodes.Span;
            var index = targetSpan.Length > 0 ? " index" : "";
            writer.WriteLine(
                $"Optional<const {SyntaxNodeClass}&> {node.RedClassName}::get_node_slot(const std::size_t{index}) const"
            );
            using var scope = writer.EnterBlockScope();
            switch (targetSpan.Length)
            {
                case 0:
                    writer.WriteLine("return std::nullopt;");
                    break;
                case 1:
                {
                    var (i, property) = targetSpan[0];
                    writer.WriteLine(
                        $"return index == {i} ? get_red({property.FieldName}) : std::nullopt;"
                    );
                    break;
                }
                default:
                {
                    writer.WriteLine("switch (index)");
                    using var switchScope = writer.EnterBlockScope();
                    foreach (var (i, property) in targetNodes.Span)
                    {
                        writer.WriteLine($"case {i}:");
                        using var indentScope = writer.EnterIndentationScope();
                        if (i == 0)
                        {
                            writer.WriteLine($"return get_red({property.FieldName});");
                        }
                        else
                        {
                            writer.WriteLine($"return get_red({property.FieldName}, {i});");
                        }
                    }

                    writer.WriteLine("default:");
                    using (writer.EnterIndentationScope())
                    {
                        writer.WriteLine("return std::nullopt;");
                    }

                    break;
                }
            }
        }

        private void EmitGetCachedSlot(CppNode node)
        {
            using var targetNodes = node
                .Properties.AsValueEnumerable()
                .Index()
                .Where(p => p.Item.Type.Name != "Token")
                .ToArrayPool();
            var targetSpan = targetNodes.Span;
            var index = targetSpan.Length > 0 ? " index" : "";
            writer.WriteLine(
                $"Optional<const {SyntaxNodeClass}&> {node.RedClassName}::get_cached_slot(const std::size_t{index}) const"
            );
            using var scope = writer.EnterBlockScope();
            switch (targetSpan.Length)
            {
                case 0:
                    writer.WriteLine("return std::nullopt;");
                    break;
                case 1:
                {
                    var (i, property) = targetSpan[0];
                    writer.WriteLine(
                        $"return index == {i} ? "
                            + $"Optional<const {SyntaxNodeClass}&>{{{property.FieldName}"
                            + $".load(std::memory_order_acquire)}} : std::nullopt;"
                    );
                    break;
                }
                default:
                {
                    writer.WriteLine("switch (index)");
                    using var switchScope = writer.EnterBlockScope();
                    foreach (var (i, property) in targetSpan)
                    {
                        writer.WriteLine($"case {i}:");
                        using var indentScope = writer.EnterIndentationScope();
                        writer.WriteLine(
                            $"return {property.FieldName}.load(std::memory_order_acquire);"
                        );
                    }
                    writer.WriteLine("default:");
                    using (writer.EnterIndentationScope())
                    {
                        writer.WriteLine("return std::nullopt;");
                    }

                    break;
                }
            }
        }

        #endregion

        private void EmitRedGetterType(CppProperty property)
        {
            if (property.Type.Name == "Token")
            {
                switch (property.Shape)
                {
                    case PropertyShape.Single:
                        writer.Write(SyntaxTokenClass);
                        break;
                    case PropertyShape.Optional:
                        writer.Write($"Optional<{SyntaxTokenClass}>");
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
                        writer.Write($"const {property.Type.RedClassName}&");
                        break;
                    case PropertyShape.Optional:
                        writer.Write($"Optional<const {property.Type.RedClassName}&>");
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

        private void EmitRedFieldType(CppProperty property)
        {
            switch (property.Shape)
            {
                case PropertyShape.Single or PropertyShape.Optional:
                    writer.Write($"std::atomic<const {property.Type.RedClassName} *>");
                    break;
                case PropertyShape.List or PropertyShape.SeparatedList:
                    writer.Write($"std::atomic<const {SyntaxNodeClass} *>");
                    break;
            }
        }

        private void EmitAbstractInstanceOfMethod(CppNode node)
        {
            using var scope = writer.EnterBlockScope();
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

                writer.Write($"node.kind() == {SyntaxKindClass}::{derived.Kind!.CppName}");
            }

            writer.WriteLine(";");
        }

        private void EmitConcreteInstanceOfMethod(CppNode node)
        {
            using var scope = writer.EnterBlockScope();
            writer.WriteLine($"return node.kind() == {SyntaxKindClass}::{node.Kind!.CppName};");
        }

        #region Diagnostic Codes

        public void EmitDiagnosticCodes(CppSyntaxModel model)
        {
            writer.WriteLine($"export module {BaseModuleName}:diagnostics.code;");
            writer.WriteLine();
            writer.WriteLine("import std;");
            writer.WriteLine();
            using var namespaceScope = writer.EnterNamespaceScope(PrismNamespace);
            writer.EmitDiagnosticCategoryEnum(model);
            writer.WriteLine();
            writer.EmitDiagnosticCategoryGetDisplayName(model);

            writer.WriteLine();
            writer.EmitDiagnosticCodeEnum(model);
        }

        private void EmitDiagnosticCategoryEnum(CppSyntaxModel model)
        {
            writer.WriteLine("export enum class DiagnosticCategory : std::uint8_t");
            using var scope = writer.EnterBlockScope(true);
            writer.WriteLine("general = 0,");
            foreach (var category in model.Diagnostics)
            {
                writer.WriteLine($"{category.CppName},");
            }
        }

        private void EmitDiagnosticCategoryGetDisplayName(CppSyntaxModel model)
        {
            writer.WriteLine(
                "export [[nodiscard]] constexpr std::string_view "
                    + "get_display_name(const DiagnosticCategory category)"
            );
            using var scope = writer.EnterBlockScope();
            writer.WriteLine("switch (category)");
            using var switchScope = writer.EnterBlockScope();
            writer.WriteLine("case DiagnosticCategory::general:");
            using (writer.EnterIndentationScope())
            {
                writer.WriteLine("return \"General\";");
            }
            foreach (var category in model.Diagnostics)
            {
                writer.WriteLine($"case DiagnosticCategory::{category.CppName}:");
                using (writer.EnterIndentationScope())
                {
                    writer.WriteLine($"return \"{category.DisplayName}\";");
                }
            }
            writer.WriteLine("default:");
            using (writer.EnterIndentationScope())
            {
                writer.WriteLine("throw std::invalid_argument(\"Unknown diagnostic category\");");
            }
        }

        private void EmitDiagnosticCodeEnum(CppSyntaxModel model)
        {
            writer.WriteLine("export enum class DiagnosticCode : std::uint32_t");
            using var scope = writer.EnterBlockScope(true);
            writer.WriteLine("unknown = 0,");
            foreach (var category in model.Diagnostics)
            {
                writer.WriteLine();
                writer.WriteLine(
                    $"// -- {category.DisplayName} ({category.Start}-{category.End}) ---"
                );
                foreach (var diagnostic in category.Diagnostics)
                {
                    writer.WriteLine($"{diagnostic.CppName} = {diagnostic.Value},");
                }
            }
        }

        #endregion

        #region DiagnosticDescriptors

        public void EmitDiagnosticDescriptors(CppSyntaxModel model)
        {
            writer.WriteLine($"export module {BaseModuleName}:diagnostics.registry;");
            writer.WriteLine();
            writer.WriteLine("import :diagnostics.descriptor;");
            writer.WriteLine("import :util.optional;");
            writer.WriteLine();
            using var namespaceScope = writer.EnterNamespaceScope(DiagnosticsNamespace);
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

        private void EmitDiagnosticDescriptorConstant(CppDiagnostic diagnostic)
        {
            writer.Write($"export constexpr DiagnosticDescriptor {diagnostic.CppName}");
            using var scope = writer.EnterBlockScope(true);
            writer.WriteLine($".code = DiagnosticCode::{diagnostic.CppName},");
            writer.WriteLine($".category = DiagnosticCategory::{diagnostic.Category.CppName},");
            writer.WriteLine(
                $".default_severity = DiagnosticSeverity::{GetCppSeverity(diagnostic.Severity)},"
            );
            writer.WriteLine($".id = \"E{diagnostic.Value:04}\",");
            writer.WriteLine($".symbol = \"{diagnostic.SymbolName}\",");
            writer.WriteLine($".title = \"{diagnostic.Title}\",");
            writer.Write(".format_message = \"");
            writer.EmitCppFormatMessage(diagnostic);
            writer.WriteLine("\",");
            if (!string.IsNullOrEmpty(diagnostic.Explanation))
            {
                writer.WriteLine($".explanation = \"{diagnostic.Explanation}\"");
            }
        }

        private void EmitCppFormatMessage(CppDiagnostic diagnostic)
        {
            foreach (var part in diagnostic.MessageParts)
            {
                switch (part)
                {
                    case CppDiagnosticMessageTextPart text:
                        // TODO: We need to re-escape the string
                        writer.Write(text.Text);
                        break;
                    case CppDiagnosticMessageArgumentPart:
                        writer.Write("{}");
                        break;
                }
            }
        }

        private void EmitDescriptorLookupFunction(CppSyntaxModel model)
        {
            writer.WriteLine(
                "export [[nodiscard]] constexpr Optional<const DiagnosticDescriptor&> "
                    + "get_descriptor(const DiagnosticCode code)"
            );
            using var scope = writer.EnterBlockScope();
            writer.WriteLine("switch (code)");
            using var switchScope = writer.EnterBlockScope();
            foreach (
                var diagnostic in model
                    .Diagnostics.AsValueEnumerable()
                    .SelectMany(x => x.Diagnostics)
            )
            {
                writer.WriteLine($"case DiagnosticCode::{diagnostic.CppName}:");
                using var caseScope = writer.EnterIndentationScope();
                writer.WriteLine($"return {diagnostic.CppName};");
            }
            writer.WriteLine("default:");
            using var defaultScope = writer.EnterIndentationScope();
            writer.WriteLine("return std::nullopt;");
        }

        #endregion

        #region Diagnostic Traits

        public void EmitDiagnosticTraits(CppSyntaxModel model)
        {
            writer.WriteLine($"export module {BaseModuleName}:diagnostics.traits;");
            writer.WriteLine();
            writer.WriteLine("import :diagnostics.code;");
            writer.WriteLine("import :syntax.kind;");
            writer.WriteLine("import :text.name;");
            writer.WriteLine();
            using var namespaceScope = writer.EnterNamespaceScope(PrismNamespace);
            writer.WriteLine("template <DiagnosticCode Code>");
            writer.WriteLine("struct DiagnosticTraits;");

            foreach (
                var diagnostic in model
                    .Diagnostics.AsValueEnumerable()
                    .SelectMany(x => x.Diagnostics)
            )
            {
                writer.WriteLine();

                writer.WriteLine("template <>");
                writer.WriteLine($"struct DiagnosticTraits<DiagnosticCode::{diagnostic.CppName}>");
                using var scope = writer.EnterBlockScope(true);
                writer.Write("using Args = std::tuple<");
                foreach (var (i, arg) in diagnostic.Arguments.AsValueEnumerable().Index())
                {
                    if (i > 0)
                        writer.Write(", ");

                    writer.Write(arg.CppType);
                }
                writer.WriteLine(">;");
            }
        }
        #endregion
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

    private sealed class TrieNode
    {
        public Dictionary<char, TrieNode> Children { get; } = new();
        public CppToken? Value { get; set; }
    }

    private static TrieNode ConstructPunctuationTrie(CppSyntaxModel model)
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

    private static string GetCppSeverity(DiagnosticSeverity severity)
    {
        return severity switch
        {
            DiagnosticSeverity.Error => "error",
            DiagnosticSeverity.Warning => "warning",
            DiagnosticSeverity.Info => "info",
            DiagnosticSeverity.Hint => "hint",
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null),
        };
    }
}
