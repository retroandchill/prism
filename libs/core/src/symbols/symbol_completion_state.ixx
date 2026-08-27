/**
 * @file symbol_completion_state.ixx
 * @author Francesco Corso
 * @date 8/27/2026
 * @brief
 */

export module prism.core:symbols.symbol_completion_state;

import :symbols.completion_part;

namespace prism
{
    class SymbolCompletionState final
    {
      public:
        [[nodiscard]] bool has_complete(CompletionPart part) const noexcept;
        bool mark_part_complete(CompletionPart part);
        [[nodiscard]] CompletionPart next_incomplete_part() const noexcept;

        void wait_part_complete(CompletionPart part) const;

      private:
        [[nodiscard]] std::uint32_t incomplete_parts() const noexcept;
        [[nodiscard]] static bool has_at_most_one_bit_set(std::uint32_t bits) noexcept;

        std::atomic<std::uint32_t> complete_parts_{0};
    };
} // namespace prism
