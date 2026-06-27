/**
 * @file symbol.ixx
 * @author Francesco Corso
 * @date 6/27/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core.semantic.symbol;

import std;
import prism.core.util;
import prism.core.diagnostic;
import prism.core.source.source_file;
import prism.core.ast.statement_syntax;
import prism.core.ast.type_syntax;

namespace prism
{
    export enum class SymbolKind
    {
        variable,
        function,
        type,
    };

    export enum class LinkageKind
    {
        external,
        internal,
    };

    export class Scope;

    export class Symbol
    {
      protected:
        constexpr Symbol(SharedString name, const SymbolKind kind) : name_{std::move(name)}, kind_{kind}
        {
        }

      public:
        Symbol(const Symbol &) = default;
        Symbol(Symbol &&) noexcept = default;

        virtual ~Symbol() = default;
        Symbol &operator=(const Symbol &) = default;
        Symbol &operator=(Symbol &&) noexcept = default;

        [[nodiscard]] constexpr const SharedString &name() const
        {
            return name_;
        }

        [[nodiscard]] constexpr SymbolKind kind() const
        {
            return kind_;
        }

        [[nodiscard]] constexpr Scope *scope() const
        {
            return scope_;
        }

        [[nodiscard]] constexpr auto &declarations() const
        {
            return declarations_;
        }

      private:
        friend class Scope;

        SharedString name_;
        SymbolKind kind_;
        Scope *scope_ = nullptr;
        std::vector<std::reference_wrapper<const DeclarationSyntax>> declarations_;
    };

    export class VariableSymbol;
    export class FunctionSymbol;
    export class TypeSymbol;

    template <typename T>
    concept SymbolType = std::derived_from<T, Symbol> && requires {
        {
            T::static_kind
        } -> std::convertible_to<SymbolKind>;
    };

    template <SymbolType T>
    constexpr T *symbol_cast(Symbol *symbol) noexcept
    {
        if (symbol == nullptr)
        {
            return nullptr;
        }

        if (symbol->kind() == T::static_kind)
        {
            return static_cast<T *>(symbol);
        }

        return nullptr;
    }

    class PRISM_CORE_API Scope
    {
      public:
        constexpr explicit Scope(DiagnosticSink &diagnostic_sink) : diagnostic_sink_{diagnostic_sink}
        {
        }

        constexpr explicit Scope(DiagnosticSink &diagnostic_sink, Scope &parent)
            : diagnostic_sink_{diagnostic_sink}, parent_{&parent}
        {
        }

        Symbol &add_symbol(const DeclarationSyntax &declaration, std::unique_ptr<Symbol> symbol);

        template <std::derived_from<Symbol> T, typename... Args>
            requires std::constructible_from<T, Args...>
        T &emplace_symbol(const DeclarationSyntax &declaration,
                          Args &&...args) noexcept(std::is_nothrow_constructible_v<T, Args...>)
        {
            return static_cast<T &>(add_symbol(declaration, std::make_unique<T>(std::forward<Args>(args)...)));
        }

        [[nodiscard]] Symbol *resolve(const SharedString &symbol) const;

      private:
        DiagnosticSink &diagnostic_sink_;
        std::vector<std::unique_ptr<Symbol>> all_symbols_;
        std::unordered_map<SharedString, Symbol *> symbols_;
        Scope *parent_ = nullptr;
    };

    class PRISM_CORE_API VariableSymbol final : public Symbol
    {
      public:
        constexpr explicit VariableSymbol(SharedString name, const TypeSymbol &type)
            : Symbol{std::move(name), SymbolKind::variable}, type_{type}
        {
        }

        [[nodiscard]] constexpr const TypeSymbol &type() const
        {
            return type_;
        }

      private:
        const TypeSymbol &type_;
    };

    class PRISM_CORE_API TypeSymbol final : public Symbol
    {
      public:
        constexpr explicit TypeSymbol(SharedString name) : Symbol{std::move(name), SymbolKind::type}
        {
        }

        static const TypeSymbol &built_in(BuiltInType type);
    };

    class PRISM_CORE_API FunctionSymbol final : public Symbol
    {
      public:
        constexpr explicit FunctionSymbol(SharedString name, const TypeSymbol &return_type)
            : Symbol{std::move(name), SymbolKind::variable}, return_type_{return_type}
        {
        }

        [[nodiscard]] constexpr const TypeSymbol &return_type() const
        {
            return return_type_;
        }

      private:
        const TypeSymbol &return_type_;
    };
} // namespace prism
