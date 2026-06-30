/**
 * @file text_cursor.ixx
 * @author Francesco Corso
 * @date 6/20/2026
 * @brief Defines the class used to pass over a source string.
 */
module;

#include <cassert>

export module prism.core.lexer.text_cursor;

import std;
import prism.core.source.source_file;

namespace prism
{
    export class TextCursor
    {
      public:
        constexpr explicit TextCursor(const std::string_view text) : text_(text)
        {
        }

        [[nodiscard]] constexpr std::size_t position() const
        {
            return position_;
        }

        // ReSharper disable once CppDFAUnreachableFunctionCall
        [[nodiscard]] constexpr bool at_end() const
        {
            return position_ >= text_.size();
        }

        [[nodiscard]] constexpr char current() const
        {
            return text_[position_];
        }

        [[nodiscard]] constexpr bool any(std::span<const char> options) const
        {
            auto ch = current();
            return std::ranges::any_of(options, [&](const auto &option) { return ch == option; });
        }

        [[nodiscard]] constexpr char peek(const std::size_t offset = 1) const
        {
            const auto pos = position_ + offset;
            return pos >= text_.size() ? '\0' : text_[pos];
        }

        constexpr void advance(const std::size_t characters = 1)
        {
            position_ += characters;
        }

        [[nodiscard]] constexpr std::string_view remaining() const
        {
            return text_.substr(position_);
        }

      private:
        std::string_view text_;
        std::size_t position_ = 0;
    };
} // namespace prism
