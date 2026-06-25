/**
 * @file source_file.ixx
 * @author Francesco Corso
 * @date 6/20/2026
 * @brief Contains the storage information for a source file
 */
module;

#include <cassert>

export module prism.core.source.source_file;

import std;

namespace prism
{
    export struct SourcePosition
    {
        std::size_t line = 1;
        std::size_t column = 1;
    };

    export struct SourceRange
    {
        std::size_t start = 0;
        std::size_t end = 0;

        [[nodiscard]] constexpr std::size_t length() const noexcept
        {
            return end - start;
        }
    };

    export class LineTable
    {
      public:
        constexpr LineTable()
        {
            line_offsets_.push_back(0);
        }

        constexpr void mark_line_start(const std::size_t offset)
        {
            assert(offset > line_offsets_.back());
            line_offsets_.push_back(offset);
        }

        [[nodiscard]] constexpr SourcePosition get_position(const std::size_t offset) const
        {
            const auto it = std::ranges::upper_bound(line_offsets_, offset);
            const auto line = static_cast<std::size_t>(std::distance(line_offsets_.begin(), it) - 1);

            const auto line_start = line_offsets_[line];
            const auto column = offset - line_start;

            return SourcePosition{.line = line, .column = column};
        }

      private:
        std::vector<std::size_t> line_offsets_;
    };

    export class SourceFile
    {
      public:
        constexpr explicit SourceFile(std::string text) : text_(std::move(text))
        {
            create_line_table();
        }

        [[nodiscard]] constexpr const std::string &text() const
        {
            return text_;
        }

        [[nodiscard]] constexpr const LineTable &line_table() const
        {
            return line_table_;
        }

      private:
        constexpr void create_line_table()
        {
            for (const auto [i, current] : text_ | std::views::enumerate)
            {
                if (current == '\n')
                {
                    line_table_.mark_line_start(i);
                }
            }
        }

        std::string text_;
        LineTable line_table_;
    };
} // namespace prism
