/**
 * @file symbol.ixx
 * @author Francesco Corso
 * @date 7/30/2026
 * @brief
 */
export module prism.core:symbols.symbol;

import :symbols.kind;
import :text.name;
import :util.noncopyable;
import :util.ref;
import :text.string_writer;
import :syntax.reference;

namespace prism
{
    export class Symbol;
    class AssemblySymbol;
    class NamespaceSymbol;
    class TypeSymbol;

    export template <typename T>
    using SymbolSpan = std::span<const Ref<const T>>;

    template <typename T>
    concept SymbolLike = std::same_as<T, Symbol> || (std::derived_from<T, Symbol> && requires(const Symbol &symbol) {
                             {
                                 T::instance_of(symbol)
                             } -> std::convertible_to<bool>;
                         });

    class Symbol : NonCopyable
    {
      protected:
        constexpr Symbol(const SymbolKind kind, const Name name, const Symbol *containing = nullptr)
            : kind_{kind}, name_{name}, containing_symbol_{containing}
        {
        }

        ~Symbol() = default;

      public:
        [[nodiscard]] constexpr SymbolKind kind() const noexcept
        {
            return kind_;
        }

        [[nodiscard]] constexpr Name name() const noexcept
        {
            return name_;
        }

        [[nodiscard]] constexpr Optional<const Symbol &> containing_symbol() const noexcept
        {
            return containing_symbol_;
        }

        [[nodiscard]] Optional<const AssemblySymbol &> containing_assembly() const noexcept;
        [[nodiscard]] Optional<const NamespaceSymbol &> containing_namespace() const noexcept;
        [[nodiscard]] Optional<const TypeSymbol &> containing_type() const noexcept;

        [[nodiscard]] virtual std::span<const SyntaxReference> declaring_syntax_references() const = 0;

        template <SymbolLike T, typename Self>
            requires std::derived_from<T, Self>
        [[nodiscard]] constexpr bool is(this const Self &self) noexcept
        {
            if constexpr (std::same_as<T, Symbol>)
            {
                return true;
            }
            else
            {
                return T::instance_of(self);
            }
        }

        template <SymbolLike T, typename Self>
            requires std::derived_from<T, Self>
        [[nodiscard]] constexpr Optional<const T &> as(this const Self &self) noexcept
        {
            return T::instance_of(self) ? Optional<const T &>{static_cast<const T &>(self)} : std::nullopt;
        }

      private:
        SymbolKind kind_;
        Name name_;
        const Symbol *containing_symbol_ = nullptr;
    };
} // namespace prism
