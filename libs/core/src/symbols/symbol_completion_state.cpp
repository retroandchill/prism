/**
 * @file symbol_completion_state.cpp
 * @author Francesco Corso
 * @date 8/27/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:symbols.symbol_completion_state.impl;

import libassert;
import :symbols.symbol_completion_state;

namespace prism
{

    bool SymbolCompletionState::has_complete(const CompletionPart part) const noexcept
    {
        return (complete_parts_.load(std::memory_order_acquire) & std::to_underlying(part)) == std::to_underlying(part);
    }

    bool SymbolCompletionState::mark_part_complete(const CompletionPart part)
    {
        std::uint32_t old_state;
        std::uint32_t new_state;
        do
        {
            old_state = complete_parts_.load(std::memory_order_acquire);
            new_state = old_state | std::to_underlying(part);
            if (new_state == old_state)
            {
                return false;
            }
        } while (!complete_parts_.compare_exchange_weak(old_state,
                                                        new_state,
                                                        std::memory_order_release,
                                                        std::memory_order_acquire));

        complete_parts_.notify_all();
        return true;
    }

    CompletionPart SymbolCompletionState::next_incomplete_part() const noexcept
    {
        const auto incomplete = incomplete_parts();
        auto next = incomplete & ~(incomplete - 1);
        DEBUG_ASSERT(has_at_most_one_bit_set(next),
                     "force_complete won't handle the result correctly if more than one bit is set.");
        return static_cast<CompletionPart>(next);
    }

    void SymbolCompletionState::wait_part_complete(const CompletionPart part) const
    {
        if (has_complete(part)) [[likely]]
            return;

        auto current = complete_parts_.load(std::memory_order_acquire);
        while (!has_complete(part))
        {
            complete_parts_.wait(current, std::memory_order_acquire);
            current = complete_parts_.load(std::memory_order_acquire);
        }
    }

    std::uint32_t SymbolCompletionState::incomplete_parts() const noexcept
    {
        return ~complete_parts_.load(std::memory_order_acquire) & std::to_underlying(CompletionPart::all);
    }

    bool SymbolCompletionState::has_at_most_one_bit_set(const std::uint32_t bits) noexcept
    {
        return (bits & bits - 1) == 0;
    }
} // namespace prism
