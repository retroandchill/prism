/**
 * @file source.ixx
 * @author Francesco Corso
 * @date 8/1/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:symbols.source;

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

        [[nodiscard]] constexpr SymbolSpan<Symbol> members() const override
        {
            return members_;
        }

      private:
        constexpr void add_member(const Symbol &member)
        {
            members_.emplace_back(member);
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, Ref<const Symbol>>
        constexpr void add_members(Range &&range)
        {
            members_.append_range(std::forward<Range>(range));
        }

        friend class DeclarationMerger;

        std::vector<Ref<const Symbol>> members_;
    };

    class SourceVariableSymbol final : public VariableSymbol
    {
      public:
        constexpr SourceVariableSymbol(const Name &name,
                                       const Symbol *containing,
                                       const VariableDeclarationSyntax &syntax)
            : VariableSymbol(name, containing), syntax_{syntax}
        {
        }

        const TypeSymbol &type() const noexcept override;
        bool is_mutable() const noexcept override;

      private:
        const VariableDeclarationSyntax &syntax_;
    };

    class SourceFunctionSymbol final : public FunctionSymbol
    {
      public:
        constexpr SourceFunctionSymbol(const Name &name,
                                       const Symbol *containing,
                                       const FunctionDeclarationSyntax &syntax)
            : FunctionSymbol(name, containing), syntax_{syntax}
        {
        }

        const TypeSymbol &returnType() const noexcept override;

        constexpr SymbolSpan<ParameterSymbol> parameters() const noexcept override
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

        friend class DeclarationMerger;

        const FunctionDeclarationSyntax &syntax_;
        std::vector<Ref<const ParameterSymbol>> parameters_;
    };

    class SourceParameterSymbol final : public ParameterSymbol
    {
      public:
        constexpr SourceParameterSymbol(const Name &name, const Symbol *containing, const ParameterSyntax &syntax)
            : ParameterSymbol(name, containing), syntax_{syntax}
        {
        }

        const TypeSymbol &type() const noexcept override;
        bool is_mutable() const noexcept override;

      private:
        const ParameterSyntax &syntax_;
    };
} // namespace prism
