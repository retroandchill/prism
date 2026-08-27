/**
 * @file symbol.ixx
 * @author Francesco Corso
 * @date 7/30/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

#include <libassert/assert-macros.hpp>

export module prism.core:symbols.symbol;

import std;
import :symbols.kind;
import :text.name;
import :util.noncopyable;
import :util.ref;
import :text.string_writer;
import :syntax.reference;
import :util.function_ref;
import :symbols.completion_part;

namespace prism
{
    class DiagnosticBag;
    export class Compilation;
    export class Symbol;
    export class AssemblySymbol;
    export class NamespaceSymbol;
    export class TypeSymbol;

    export template <typename T>
    using SymbolSpan = std::span<const Ref<const T>>;

    template <typename T>
    concept SymbolLike = std::same_as<T, Symbol> || (std::derived_from<T, Symbol> && requires(const Symbol &symbol) {
                             {
                                 T::instance_of(symbol)
                             } -> std::convertible_to<bool>;
                         });

    using SymbolPredicate = FunctionRef<bool(const Symbol &)>;

    export enum class DeclaredVisibility : std::uint8_t
    {
        not_applicable,
        public_,
        internal,
        file
    };

    /**
     * @brief Base class for all symbols.
     */
    export class PRISM_CORE_API Symbol : NonCopyable
    {
      protected:
        constexpr Symbol(const SymbolKind kind, const Name name, const Symbol *containing = nullptr)
            : kind_{kind}, name_{name}, containing_symbol_{containing}
        {
        }

        ~Symbol() noexcept = default;

      public:
        /**
         * @brief Get the kind of symbol.
         * @return The kind of symbol
         */
        [[nodiscard]] constexpr SymbolKind kind() const noexcept
        {
            return kind_;
        }

        /**
         * @brief Get the name of the symbol.
         * @return The name of the symbol
         */
        [[nodiscard]] constexpr Name name() const noexcept
        {
            return name_;
        }

        [[nodiscard]] virtual DeclaredVisibility declared_visibility() const;

        [[nodiscard]] virtual const ImmutableArray<Location> &locations() const = 0;

        [[nodiscard]] virtual bool is_implicitly_declared() const noexcept;

        [[nodiscard]] Optional<Location> try_get_first_location() const;

        [[nodiscard]] Location first_location() const;

        [[nodiscard]] Location first_location_or_none() const;

        /**
         * @brief Get the containing symbol.
         * @return The containing symbol
         */
        [[nodiscard]] constexpr Optional<const Symbol &> containing_symbol() const noexcept
        {
            return containing_symbol_;
        }

        /**
         * @brief Get the containing assembly.
         * @return The containing assembly
         */
        [[nodiscard]] virtual Optional<const AssemblySymbol &> containing_assembly() const;

        /**
         * @brief Get the containing namespace.
         * @return The containing namespace
         */
        [[nodiscard]] virtual Optional<const NamespaceSymbol &> containing_namespace() const;

        /**
         * @brief Get the containing type.
         * @return The containing type
         */
        [[nodiscard]] virtual Optional<const TypeSymbol &> containing_type() const;

        /**
         * Get the syntax references that declare this symbol.
         * @return The span of references to this symbol.
         */
        [[nodiscard]] virtual std::span<const SyntaxReference> declaring_syntax_references() const = 0;

        [[nodiscard]] virtual bool is_defined_in_source_tree(const SyntaxTree &tree,
                                                             Optional<TextSpan> defined_within) const;

      protected:
        [[nodiscard]] static bool is_defined_in_source_tree(const SyntaxReference &reference,
                                                            const SyntaxTree &tree,
                                                            Optional<TextSpan> defined_within);

      public:
        /**
         * @brief Checks if this object is an instance of the target type.
         * @tparam T The target type
         * @return If this object is of the target type
         */
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

        template <SymbolLike... Ts, typename Self>
            requires(std::derived_from<Ts, Self> && ...)
        [[nodiscard]] constexpr bool is_any_of(this const Self &self) noexcept
        {
            return (self.template is<Ts>() || ...);
        }

        /**
         * @brief Casts this object to the target type if it is an instance of the target type.
         * @tparam T The target type
         * @return The casted object if it is of the target type, otherwise `nullopt`.
         */
        template <SymbolLike T, typename Self>
            requires std::derived_from<T, Self>
        [[nodiscard]] constexpr Optional<const T &> as(this const Self &self) noexcept
        {
            return T::instance_of(self) ? Optional<const T &>{static_cast<const T &>(self)} : std::nullopt;
        }

        /**
         * @brief Casts this object to the target type if it is an instance of the target type, performing an assertion
         *        in a debug build.
         * @tparam T The target type
         * @return The casted object if it is of the target type, otherwise `nullopt`.
         */
        template <SymbolLike T, typename Self>
            requires std::derived_from<T, Self>
        [[nodiscard]] constexpr const T &as_checked(this const Self &self) noexcept
        {
            DEBUG_ASSERT(T::instance_of(self));
            return static_cast<const T &>(self);
        }

        virtual void write_display_string(TextWriter &writer) const = 0;

        template <typename Self>
        [[nodiscard]] std::string to_display_string(this const Self &self)
        {
            std::string result;
            StringWriter writer{result};
            self.write_display_string(writer);
            return result;
        }

      protected:
        [[nodiscard]] virtual Optional<const Compilation &> declaring_compilation() const;
        void add_declaration_diagnostics(const DiagnosticBag &diagnostics) const;

        [[nodiscard]] virtual bool needs_completion() const noexcept;
        virtual void force_complete(const Optional<SourceLocation> &location,
                                    const Optional<SymbolPredicate> &filter) const;
        [[nodiscard]] virtual bool is_complete(CompletionPart part) const noexcept;

        static void force_complete_member_conditionally(const Optional<SourceLocation> &location,
                                                        const Optional<SymbolPredicate> &filter,
                                                        const Symbol &member);

      private:
        friend struct SymbolInternal;

        SymbolKind kind_;
        Name name_;
        const Symbol *containing_symbol_ = nullptr;
    };

    struct SymbolInternal
    {
        [[nodiscard]] static inline bool needs_completion(const Symbol &symbol) noexcept
        {
            return symbol.needs_completion();
        }

        static inline void force_complete(const Symbol &symbol,
                                          const Optional<SourceLocation> &location,
                                          const Optional<SymbolPredicate> &filter) noexcept
        {
            symbol.force_complete(location, filter);
        }

        [[nodiscard]] static inline bool is_complete(const Symbol &symbol, CompletionPart part) noexcept
        {
            return symbol.is_complete(part);
        }
    };
} // namespace prism
