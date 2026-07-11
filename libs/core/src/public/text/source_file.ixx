/**
 * @file source_file.ixx
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
module;

#include <cassert>

export module prism.core:text.source_file;

import std;
import :text.text_span;

namespace prism
{
    export struct SourcePosition final
    {
        std::uint32_t line;
        std::uint32_t column;

        [[nodiscard]] friend constexpr std::strong_ordering operator<=>(SourcePosition lhs,
                                                                        SourcePosition rhs) noexcept = default;

        friend constexpr std::ostream &operator<<(std::ostream &os, const SourcePosition pos)
        {
            return os << pos.line << ":" << pos.column;
        }
    };

    export class SourceFile final
    {
      public:
        constexpr explicit SourceFile(std::string text) : text_{std::move(text)}
        {
            assert(text.size() <= std::numeric_limits<std::uint32_t>::max());

            line_offsets_.push_back(0);
            for (std::uint32_t i = 0; i < text_.size(); ++i)
            {
                if (const auto current = text_[i]; current == '\n')
                    line_offsets_.push_back(i + 1);
                else if (current == '\r')
                {
                    if (i + 1 < text_.size() && text_[i + 1] == '\n')
                        ++i;
                    line_offsets_.push_back(i + 1);
                }
            }
        }

        [[nodiscard]] constexpr std::string_view text() const
        {
            return text_;
        }

        [[nodiscard]] constexpr SourcePosition position_of(std::uint32_t index) const
        {
            if (index >= text_.size())
                throw std::out_of_range{"index out of range"};

            const auto it = std::ranges::upper_bound(line_offsets_, index);
            const auto line = static_cast<std::uint32_t>(std::distance(line_offsets_.begin(), it) - 1);
            const auto column = index - line_offsets_[line];
            return SourcePosition{line + 1, column + 1};
        }

        [[nodiscard]] constexpr std::string_view slice(const TextSpan span) const
        {
            return std::string_view{text_}.substr(span.start, span.length);
        }

      private:
        std::string text_;
        std::vector<std::uint32_t> line_offsets_;
    };
} // namespace prism

template <>
struct std::hash<prism::SourcePosition>
{
    constexpr std::size_t operator()(const prism::SourcePosition &pos) const noexcept
    {
        return prism::hash_combine(pos.line, pos.column);
    }
};
