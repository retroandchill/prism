/**
 * @file cursor.ixx
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
module;

#include <cassert>

export module prism.core:text.cursor;

import std;

namespace prism
{
    class TextCursor
    {
      public:
        explicit constexpr TextCursor(const std::string_view text)
            : buffer_{text.data()}, position_{0}, length_{static_cast<std::uint32_t>(text.size())}
        {
            // Check to ensure large text buffers don't get truncated
            assert(text.size() == length_);
        }

        [[nodiscard]] constexpr std::uint32_t position() const noexcept
        {
            return position_;
        }

        [[nodiscard]] constexpr bool at_end() const noexcept
        {
            return position_ >= length_;
        }

        [[nodiscard]] constexpr char current() const noexcept
        {
            return buffer_[position_];
        }

        template <typename... Args>
            requires(std::equality_comparable_with<char, Args> && ...)
        [[nodiscard]] constexpr bool any(Args &&...args) const
        {
            auto c = current();
            return ((c == args) || ...);
        }

        [[nodiscard]] constexpr char peek(const std::uint32_t offset = 1) const noexcept
        {
            const auto pos = position_ + offset;
            return pos < length_ ? buffer_[pos] : '\0';
        }

        [[nodiscard]] constexpr std::string_view remaining() const noexcept
        {
            return {std::next(buffer_, position_), length_ - position_};
        }

        constexpr void advance(const std::uint32_t characters = 1)
        {
            assert(characters != 0);
            position_ += characters;
        }

      private:
        const char *buffer_;
        std::uint32_t position_;
        std::uint32_t length_;
    };
} // namespace prism
