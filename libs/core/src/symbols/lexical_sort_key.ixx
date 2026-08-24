/**
 * @file lexical_sort_key.ixx
 * @author Francesco Corso
 * @date 8/13/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:symbols.lexical_sort_key;

import std;
import libassert;
import :util.numerics;

namespace prism
{
    export class SourceLocation;
    export class SyntaxReference;
    export class Compilation;
    export class SyntaxTree;

    class LexicalSortKey final
    {
        struct NoCheck
        {
        };

        static constexpr NoCheck no_check;

        static constexpr std::uint32_t invalid_index = index_none<std::uint32_t>;
        static constexpr std::uint32_t synthesized_index = invalid_index - 1;

        constexpr explicit LexicalSortKey(NoCheck,
                                          const std::uint32_t tree_ordinal,
                                          const std::uint32_t position) noexcept
            : tree_ordinal_{tree_ordinal}, position_{position}
        {
        }

        constexpr explicit LexicalSortKey(const std::uint32_t tree_ordinal, const std::uint32_t position) noexcept
            : tree_ordinal_{tree_ordinal}, position_{position}
        {
            DEBUG_ASSERT(tree_ordinal != invalid_index);
            DEBUG_ASSERT(position != invalid_index);
        }

      public:
        LexicalSortKey(const SyntaxTree &tree, std::uint32_t position, const Compilation &compilation);

        LexicalSortKey(const SyntaxReference &reference, const Compilation &compilation);

        LexicalSortKey(const SourceLocation &location, const Compilation &compilation);

        [[nodiscard]] static constexpr LexicalSortKey not_in_source()
        {
            return LexicalSortKey{no_check, invalid_index, 0};
        }

        [[nodiscard]] static constexpr LexicalSortKey not_initialized()
        {
            return LexicalSortKey{no_check, invalid_index, invalid_index};
        }

        [[nodiscard]] static constexpr LexicalSortKey synthesized(const std::uint32_t offset)
        {
            return LexicalSortKey{no_check, invalid_index, synthesized_index - offset};
        }

        [[nodiscard]] constexpr std::uint32_t tree_ordinal() const noexcept
        {
            return tree_ordinal_;
        }

        [[nodiscard]] constexpr std::uint32_t position() const noexcept
        {
            return position_;
        }

        [[nodiscard]] constexpr bool is_initialized() const noexcept
        {
            return position_ != invalid_index;
        }

        [[nodiscard]] constexpr friend bool operator==(LexicalSortKey lhs, LexicalSortKey rhs) noexcept = default;

        [[nodiscard]] constexpr friend std::strong_ordering operator<=>(const LexicalSortKey lhs,
                                                                        const LexicalSortKey rhs) noexcept
        {
            if (lhs.tree_ordinal_ != rhs.tree_ordinal_)
            {
                if (lhs.tree_ordinal_ == invalid_index)
                    return std::strong_ordering::greater;

                if (rhs.tree_ordinal_ == invalid_index)
                    return std::strong_ordering::less;

                return lhs.tree_ordinal_ <=> rhs.tree_ordinal_;
            }

            return lhs.position_ <=> rhs.position_;
        }

        static constexpr LexicalSortKey first(const LexicalSortKey lhs, const LexicalSortKey rhs) noexcept
        {
            auto comparison = lhs <=> rhs;
            return comparison == std::strong_ordering::less ? lhs : rhs;
        }

      private:
        std::uint32_t tree_ordinal_ = 0;
        std::uint32_t position_ = 0;
    };
} // namespace prism
