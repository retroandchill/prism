/**
 * @file source.ixx
 * @author Francesco Corso
 * @date 8/1/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:symbols.source;

import std;
import libassert;
import :symbols.namespace_symbol;
import :symbols.assembly_symbol;
import :symbols.variable_symbol;
import :symbols.function_symbol;
import :symbols.parameter_symbol;
import :symbols.named_type_symbol;
import :declarations.merged_namespace_declaration;

namespace prism
{
    class Binder;
    class ParameterSyntax;
    class FunctionDeclarationSyntax;
    class VariableDeclarationSyntax;

    class SourceAssemblySymbol final : public AssemblySymbol
    {
      public:
        explicit SourceAssemblySymbol(const Compilation &compilation);

        [[nodiscard]] const ImmutableArray<Location> &locations() const override;

        [[nodiscard]] const NamespaceSymbol &global_namespace() const override;

        [[nodiscard]] std::span<const SyntaxReference> declaring_syntax_references() const override;

      protected:
        [[nodiscard]] Optional<const Compilation &> declaring_compilation() const override;

      private:
        const Compilation &declaring_compilation_;
        mutable Lazy<ImmutableArray<Location>> locations_;
        mutable Lazy<const NamespaceSymbol &> global_namespace_;
    };

    class SourceNamespaceSymbol final : public NamespaceSymbol
    {
      public:
        SourceNamespaceSymbol(RefCountPtr<const MergedNamespaceDeclaration> declaration,
                              const AssemblySymbol &assembly,
                              const Symbol *containing);

        [[nodiscard]] constexpr NamespaceKind namespace_kind() const noexcept override
        {
            return NamespaceKind::assembly;
        }

        [[nodiscard]] Optional<const AssemblySymbol &> containing_assembly() const noexcept override;

        [[nodiscard]] const ImmutableArray<Location> &locations() const override;

        [[nodiscard]] Optional<const Compilation &> containing_compilation() const noexcept override;

        [[nodiscard]] SymbolSpan<Symbol> members() const override;

        [[nodiscard]] SymbolSpan<Symbol> members(Name name) const override;

        [[nodiscard]] std::span<const SyntaxReference> declaring_syntax_references() const override;

      private:
        [[nodiscard]] const std::unordered_map<Name, ImmutableArray<Ref<const Symbol>>> &get_name_to_members_map()
            const;
        [[nodiscard]] std::unordered_map<Name, ImmutableArray<Ref<const Symbol>>> make_name_to_members_map() const;

        [[nodiscard]] const Symbol &build_symbol(const MergedDeclaration &declaration) const;
        [[nodiscard]] const Symbol &build_symbol(const VariableDeclarationSyntax &declaration) const;
        [[nodiscard]] const Symbol &build_symbol(const FunctionDeclarationSyntax &declaration) const;

        [[nodiscard]] ImmutableArray<Ref<const Symbol>> compute_members() const;

        const AssemblySymbol &containing_assembly_;
        RefCountPtr<const MergedNamespaceDeclaration> merged_declaration_;
        mutable Lazy<ImmutableArray<Location>> locations_;
        mutable Lazy<std::unordered_map<Name, ImmutableArray<Ref<const Symbol>>>> name_to_members_map_;
        mutable Lazy<ImmutableArray<SyntaxReference>> syntax_references_;
        mutable Lazy<ImmutableArray<Ref<const Symbol>>> members_;
    };

    class SourceVariableSymbol : public VariableSymbol
    {
      protected:
        SourceVariableSymbol(Name name, const Symbol *containing, const VariableDeclarationSyntax &syntax);

        ~SourceVariableSymbol() = default;

      public:
        [[nodiscard]] const ImmutableArray<Location> &locations() const final;

        [[nodiscard]] const TypeSymbol &type() const final;
        [[nodiscard]] bool is_mutable() const noexcept final;
        [[nodiscard]] bool has_initializer() const noexcept final;
        [[nodiscard]] std::span<const SyntaxReference> declaring_syntax_references() const final;

      protected:
        [[nodiscard]] const VariableDeclarationSyntax &syntax() const noexcept;
        [[nodiscard]] virtual const TypeSymbol &compute_type(DiagnosticBag &diagnostics) const = 0;

      private:
        mutable Lazy<ImmutableArray<Location>> locations_;

        const VariableDeclarationSyntax &syntax_;
        SyntaxReference syntax_reference_;
        mutable Lazy<const TypeSymbol &> type_;
    };

    class SourceLocalVariableSymbol final : public SourceVariableSymbol
    {
      public:
        SourceLocalVariableSymbol(Name name,
                                  const Symbol *containing,
                                  const VariableDeclarationSyntax &syntax,
                                  const Binder &scope_binder,
                                  const Binder *initializer_binder);

      protected:
        [[nodiscard]] const TypeSymbol &compute_type(DiagnosticBag &diagnostics) const override;

        const Binder &scope_binder_;
        const Binder *initializer_binder_;
    };

    class SourceGlobalVariableSymbol final : public SourceVariableSymbol
    {
      public:
        SourceGlobalVariableSymbol(Name name, const Symbol *containing, const VariableDeclarationSyntax &syntax);

      protected:
        [[nodiscard]] const TypeSymbol &compute_type(DiagnosticBag &diagnostics) const override;
    };

    class SourceFunctionSymbol final : public FunctionSymbol
    {
      public:
        SourceFunctionSymbol(const Name &name, const Symbol *containing, const FunctionDeclarationSyntax &syntax);

        [[nodiscard]] const ImmutableArray<Location> &locations() const override;

        [[nodiscard]] const TypeSymbol &return_type() const override;

        [[nodiscard]] SymbolSpan<ParameterSymbol> parameters() const noexcept override;

        [[nodiscard]] std::span<const SyntaxReference> declaring_syntax_references() const override;

      private:
        [[nodiscard]] const TypeSymbol &compute_return_type(DiagnosticBag &diagnostics) const;

        [[nodiscard]] SymbolSpan<ParameterSymbol> compute_parameters() const;

        const FunctionDeclarationSyntax &syntax_;
        SyntaxReference syntax_reference_;
        mutable Lazy<SymbolSpan<ParameterSymbol>> parameters_;
        mutable Lazy<const TypeSymbol &> return_type_;
        mutable Lazy<ImmutableArray<Location>> locations_;
    };

    class SourceParameterSymbol final : public ParameterSymbol
    {
      public:
        SourceParameterSymbol(const Name &name, const Symbol *containing, const ParameterSyntax &syntax);

        [[nodiscard]] const ImmutableArray<Location> &locations() const override;

        [[nodiscard]] const TypeSymbol &type() const override;
        [[nodiscard]] bool is_mutable() const noexcept override;
        [[nodiscard]] std::span<const SyntaxReference> declaring_syntax_references() const override;

      private:
        [[nodiscard]] const TypeSymbol &compute_type(DiagnosticBag &diagnostics) const;

        const ParameterSyntax &syntax_;
        SyntaxReference syntax_reference_;
        mutable Lazy<const TypeSymbol &> type_;
        mutable Lazy<ImmutableArray<Location>> locations_;
    };
} // namespace prism
