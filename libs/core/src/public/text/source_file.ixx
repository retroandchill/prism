/**
 * @file source_file.ixx
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:text.source_file;

import std;
import :text.text_span;
import uni_algo;
import libassert;

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

    export class SourcePositionSpan final
    {
      public:
        constexpr SourcePositionSpan(const SourcePosition start, const SourcePosition end) : start_{start}, end_{end}
        {
            if (end < start)
            {
                throw std::invalid_argument{"end position must be greater than start position"};
            }
        }

        [[nodiscard]] constexpr SourcePosition start() const noexcept
        {
            return start_;
        }

        [[nodiscard]] constexpr SourcePosition end() const noexcept
        {
            return end_;
        }

        constexpr friend bool operator==(const SourcePositionSpan &lhs,
                                         const SourcePositionSpan &rhs) noexcept = default;

        friend constexpr std::ostream &operator<<(std::ostream &os, const SourcePositionSpan span)
        {
            return os << '(' << span.start_ << ")-(" << span.end_ << ')';
        }

      private:
        SourcePosition start_;
        SourcePosition end_;
    };

    export class FileSourcePositionSpan final
    {
      public:
        constexpr FileSourcePositionSpan(std::string_view path, const SourcePosition start, const SourcePosition end)
            : FileSourcePositionSpan{path, SourcePositionSpan{start, end}}
        {
        }

        constexpr FileSourcePositionSpan(std::string_view path, const SourcePositionSpan span)
            : path_{path}, span_{span}
        {
        }

        [[nodiscard]] constexpr std::string_view path() const noexcept
        {
            return path_;
        }

        [[nodiscard]] constexpr const SourcePositionSpan &span() const noexcept
        {
            return span_;
        }

        [[nodiscard]] constexpr SourcePosition start_line_position() const noexcept
        {
            return span_.start();
        }

        [[nodiscard]] constexpr SourcePosition end_line_position() const noexcept
        {
            return span_.end();
        }

        constexpr friend bool operator==(const FileSourcePositionSpan &lhs, const FileSourcePositionSpan &rhs) noexcept
        {
            return lhs.span_ == rhs.span_ && lhs.path_ == rhs.path_;
        }

        constexpr friend std::ostream &operator<<(std::ostream &os, const FileSourcePositionSpan span)
        {
            return os << span.path_ << ":" << span.span_;
        }

      private:
        std::string_view path_;
        SourcePositionSpan span_;
    };

    export class SourceText final
    {
      public:
        constexpr explicit SourceText(std::string text) : text_{std::move(text)}
        {
            DEBUG_ASSERT(text.size() <= std::numeric_limits<std::uint32_t>::max());

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

        [[nodiscard]] constexpr SourcePosition position_of(const std::uint32_t index) const
        {
            if (index >= text_.size())
                throw std::out_of_range{"index out of range"};

            const auto it = std::ranges::upper_bound(line_offsets_, index);
            const auto line = static_cast<std::uint32_t>(std::distance(line_offsets_.begin(), it) - 1);
            const auto utf_offset = index - line_offsets_[line];
            const auto column =
                std::ranges::distance(std::string_view{text_}.substr(index, utf_offset) | una::views::utf8);
            return SourcePosition{line + 1, static_cast<std::uint32_t>(column) + 1};
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

template <>
struct std::formatter<prism::SourcePosition>
{

    static constexpr auto parse(std::format_parse_context &ctx)
    {
        return ctx.begin();
    }

    static constexpr auto format(const prism::SourcePosition pos, auto &ctx)
    {
        return std::format_to(ctx.out(), "{:d}:{:d}", pos.line, pos.column);
    }
};

template <>
struct std::hash<prism::SourcePositionSpan>
{
    constexpr std::size_t operator()(const prism::SourcePositionSpan &span) const noexcept
    {
        return prism::hash_combine(span.start(), span.end());
    }
};

template <>
struct std::formatter<prism::SourcePositionSpan>
{
    static constexpr auto parse(std::format_parse_context &ctx)
    {
        return ctx.begin();
    }

    static constexpr auto format(const prism::SourcePositionSpan span, auto &ctx)
    {
        return std::format_to(ctx.out(), "({})-({})", span.start(), span.end());
    }
};

template <>
struct std::hash<prism::FileSourcePositionSpan>
{
    constexpr std::size_t operator()(const prism::FileSourcePositionSpan &span) const noexcept
    {
        return prism::hash_combine(span.path(), span.span());
    }
};

template <>
struct std::formatter<prism::FileSourcePositionSpan>
{
    static constexpr auto parse(std::format_parse_context &ctx)
    {
        return ctx.begin();
    }

    static constexpr auto format(const prism::FileSourcePositionSpan &span, auto &ctx)
    {
        return std::format_to(ctx.out(), "{}:{}", span.path(), span.span());
    }
};
