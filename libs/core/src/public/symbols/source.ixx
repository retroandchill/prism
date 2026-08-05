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

namespace prism
{
    class ParameterSyntax;
    class FunctionDeclarationSyntax;
    class VariableDeclarationSyntax;

    class SourceAssemblySymbol final : public AssemblySymbol
    {
      public:
        constexpr explicit SourceAssemblySymbol(const Name name) : AssemblySymbol{name}
        {
        }

        [[nodiscard]] constexpr const NamespaceSymbol &global_namespace() const noexcept override
        {
            ASSUME(global_namespace_ != nullptr);
            return *global_namespace_;
        }

        [[nodiscard]] std::span<const SyntaxReference> declaring_syntax_references() const override;

      private:
        friend class DeclarationMerger;

        const NamespaceSymbol *global_namespace_ = nullptr;
    };

    class SourceNamespaceSymbol final : public NamespaceSymbol
    {
      public:
        constexpr SourceNamespaceSymbol(const Name name, const Symbol *containing) : NamespaceSymbol{name, containing}
        {
        }

        [[nodiscard]] Optional<const Compilation &> containing_compilation() const noexcept override;

        [[nodiscard]] constexpr SymbolSpan<Symbol> members() const override
        {
            return members_;
        }

        [[nodiscard]] constexpr std::span<const SyntaxReference> declaring_syntax_references() const override
        {
            return syntax_references_;
        }

      private:
        constexpr void add_member(const Symbol &member)
        {
            members_.emplace_back(member);
        }

        constexpr void add_syntax_reference(const SyntaxReference syntax_reference)
        {
            syntax_references_.push_back(syntax_reference);
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, Ref<const Symbol>>
        constexpr void add_members(Range &&range)
        {
            members_.append_range(std::forward<Range>(range));
        }

      private:
        friend class DeclarationMerger;

        std::vector<Ref<const Symbol>> members_{};
        std::vector<SyntaxReference> syntax_references_{};
    };

    class SourceVariableSymbol final : public VariableSymbol
    {
      public:
        SourceVariableSymbol(const Name &name, const Symbol *containing, const VariableDeclarationSyntax &syntax);

        [[nodiscard]] const TypeSymbol &type() const override;
        [[nodiscard]] bool is_mutable() const noexcept override;

        [[nodiscard]] std::span<const SyntaxReference> declaring_syntax_references() const override;

      private:
        const VariableDeclarationSyntax &syntax_;
        SyntaxReference syntax_reference_;
    };

    class SourceFunctionSymbol final : public FunctionSymbol
    {
      public:
        SourceFunctionSymbol(const Name &name, const Symbol *containing, const FunctionDeclarationSyntax &syntax);

        [[nodiscard]] const TypeSymbol &return_type() const override;

        [[nodiscard]] constexpr SymbolSpan<ParameterSymbol> parameters() const noexcept override
        {
            return parameters_;
        }

      private:
        constexpr void add_parameter(const ParameterSymbol &parameter)
        {
            parameters_.emplace_back(parameter);
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, Ref<const ParameterSymbol>>
        constexpr void add_parameters(Range &&range)
        {
            parameters_.append_range(std::forward<Range>(range));
        }

      public:
        [[nodiscard]] std::span<const SyntaxReference> declaring_syntax_references() const override;

      private:
        friend class DeclarationMerger;

        const FunctionDeclarationSyntax &syntax_;
        SyntaxReference syntax_reference_;
        std::vector<Ref<const ParameterSymbol>> parameters_;
    };

    class SourceParameterSymbol final : public ParameterSymbol
    {
      public:
        SourceParameterSymbol(const Name &name, const Symbol *containing, const ParameterSyntax &syntax);

        [[nodiscard]] const TypeSymbol &type() const override;
        [[nodiscard]] bool is_mutable() const noexcept override;
        [[nodiscard]] std::span<const SyntaxReference> declaring_syntax_references() const override;

      private:
        const ParameterSyntax &syntax_;
        SyntaxReference syntax_reference_;
    };
} // namespace prism
