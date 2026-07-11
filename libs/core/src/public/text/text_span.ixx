/**
 * @file text_span.ixx
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
export module prism.core:text.text_span;

import std;
import :util.optional;

namespace prism
{
    export struct TextSpan final
    {
        std::uint32_t start = 0;
        std::uint32_t length = 0;

        [[nodiscard]] constexpr std::uint32_t end() const
        {
            return start + length;
        }

        [[nodicard]] constexpr bool empty() const
        {
            return length == 0;
        }

        [[nodiscard]] constexpr bool contains(const std::uint32_t position) const
        {
            return position - start < length;
        }

        [[nodiscard]] constexpr bool contains(const TextSpan other) const
        {
            return other.start >= start && other.end() <= end();
        }

        [[nodiscard]] constexpr bool overlaps_with(const TextSpan other) const
        {
            const auto overlap_start = std::max(start, other.start);
            const auto overlap_end = std::min(end(), other.end());

            return overlap_start < overlap_end;
        }

        [[nodiscard]] constexpr Optional<TextSpan> overlap(const TextSpan other) const
        {
            const auto overlap_start = std::max(start, other.start);
            const auto overlap_end = std::min(end(), other.end());

            return overlap_start < overlap_end ? Optional{from_bounds(overlap_start, overlap_end)} : std::nullopt;
        }

        [[nodiscard]] constexpr bool intersects_with(const std::uint32_t position) const
        {
            return (position - start) <= length;
        }

        [[nodiscard]] constexpr Optional<TextSpan> intersection(const TextSpan other) const
        {
            auto intersection_start = std::max(start, other.start);
            auto intersection_end = std::min(end(), other.end());

            return intersection_start <= intersection_end ? Optional{from_bounds(intersection_start, intersection_end)}
                                                          : std::nullopt;
        }

        [[nodiscard]] static constexpr TextSpan from_bounds(const std::uint32_t start, const std::uint32_t end)
        {
            if (end < start)
                throw std::invalid_argument{"end must be greater than or equal to start"};

            return TextSpan{start, end - start};
        }

        [[nodiscard]] friend constexpr std::strong_ordering operator<=>(TextSpan lhs, TextSpan rhs) noexcept = default;

        constexpr friend std::ostream &operator<<(std::ostream &os, const TextSpan span)
        {
            os << "[" << span.start << ".." << span.end() << ")";
            return os;
        }
    };
} // namespace prism

template <>
struct std::hash<prism::TextSpan>
{
    constexpr std::size_t operator()(const prism::TextSpan span) const noexcept
    {
        return prism::hash_combine(span.start, span.length);
    }
};

template <>
struct std::formatter<prism::TextSpan>
{
    static constexpr auto format(const prism::TextSpan span, std::format_context &ctx)
    {
        return std::format_to(ctx.out(), "[{:d}..{:d})", span.start, span.end());
    }
};
