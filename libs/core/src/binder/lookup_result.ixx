/**
 * @file lookup_result.ixx
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:binder.lookup_result;

import :symbols.symbol;
import :memory.buffer_pool;

namespace prism
{
    using SymbolList = std::vector<Ref<const Symbol>>;

    export enum class LookupResultKind : std::uint8_t
    {
        not_found,
        viable,
        ambiguous,
        inaccessible,
        wrong_kind,
        error
    };

    export class LookupResult final
    {
      public:
        [[nodiscard]] constexpr LookupResultKind kind() const noexcept
        {
            return kind_;
        }

        [[nodiscard]] bool found() const noexcept
        {
            return kind_ != LookupResultKind::not_found;
        }

        [[nodiscard]] bool viable() const noexcept
        {
            return kind_ == LookupResultKind::viable;
        }

        [[nodiscard]] bool ambiguous() const noexcept
        {
            return kind_ == LookupResultKind::ambiguous;
        }

        [[nodiscard]] bool is_error() const noexcept
        {
            return kind_ == LookupResultKind::error;
        }

        [[nodiscard]] constexpr const SymbolList &symbols() const & noexcept
        {
            return symbols_;
        }

        [[nodiscard]] constexpr SymbolList &&symbols() && noexcept
        {
            return std::move(symbols_);
        }

        [[nodiscard]] constexpr const Symbol &symbol() const
        {
            // We'll mark this unlikely since the expectation is that you already
            // checked that found() is true and ambiguous() is false, that way if
            // we get a good branch prediction. (Assuming that this part doesn't
            // get completely optimized out that is)
            if (symbols_.size() != 1) [[unlikely]]
                throw InvalidStateException{"Can only call symbol() if ambiguous() is false and found() is true"};

            return symbols_.front().get();
        }

        static constexpr LookupResult not_found()
        {
            return LookupResult{LookupResultKind::not_found, SymbolList{}};
        }

        static constexpr LookupResult viable(SymbolList symbols)
        {
            DEBUG_ASSERT(symbols.size() > 0);
            return LookupResult{LookupResultKind::viable, std::move(symbols)};
        }

        static constexpr LookupResult ambiguous(SymbolList symbols)
        {
            return LookupResult{LookupResultKind::ambiguous, std::move(symbols)};
        }

        static constexpr LookupResult wrong_kind(SymbolList symbols)
        {
            return LookupResult{LookupResultKind::wrong_kind, std::move(symbols)};
        }

        static constexpr LookupResult inaccessible(SymbolList symbols)
        {
            return LookupResult{LookupResultKind::inaccessible, std::move(symbols)};
        }

        static constexpr LookupResult error()
        {
            return LookupResult{LookupResultKind::error, SymbolList{}};
        }

      private:
        constexpr LookupResult(const LookupResultKind kind, SymbolList symbols) noexcept
            : kind_{kind}, symbols_{std::move(symbols)}
        {
        }

        LookupResultKind kind_;
        SymbolList symbols_{};
    };
} // namespace prism
