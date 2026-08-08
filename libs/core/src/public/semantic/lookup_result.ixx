/**
 * @file lookup_result.ixx
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
export module prism.core:semantic.lookup_result;

import :symbols.symbol;
import :memory.buffer_pool;

namespace prism
{
    using SymbolList = PooledVector<Ref<const Symbol>>;

    export class LookupResult final
    {
      public:
        constexpr LookupResult() = default;

        constexpr explicit LookupResult(SymbolList symbols) : symbols_{std::move(symbols)}
        {
        }

        [[nodiscard]] constexpr bool found() const noexcept
        {
            return !symbols_.empty();
        }

        [[nodiscard]] constexpr bool ambiguous() const noexcept
        {
            return symbols_.size() > 1;
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

        [[nodiscard]] constexpr SymbolSpan<Symbol> symbols() const noexcept
        {
            return symbols_;
        }

      private:
        SymbolList symbols_{};
    };
} // namespace prism
