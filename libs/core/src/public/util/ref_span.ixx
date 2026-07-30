/**
 * @file ref_span.ixx
 * @author Francesco Corso
 * @date 7/30/2026
 * @brief
 */
export module prism.core:util.ref_span;

import std;

namespace prism
{
    template <typename T>
    class RefSpanIterator
    {
      public:
        using value_type = T;
        using difference_type = std::ptrdiff_t;

        constexpr RefSpanIterator() noexcept = default;

        explicit constexpr RefSpanIterator(std::span<T *>::iterator iterator) noexcept : iterator_{iterator}
        {
        }

        [[nodiscard]] constexpr T &operator*() const noexcept
        {
            return **iterator_;
        }

        [[nodiscard]] constexpr T *operator->() const noexcept
        {
            return *iterator_;
        }

        constexpr T &operator[](std::size_t n) const noexcept
        {
            return *iterator_[n];
        }

        constexpr RefSpanIterator operator+(std::ptrdiff_t n) const noexcept
        {
            return RefSpanIterator{iterator_ + n};
        }

        constexpr RefSpanIterator &operator+=(std::ptrdiff_t n) noexcept
        {
            iterator_ += n;
            return *this;
        }

        constexpr RefSpanIterator operator-(std::ptrdiff_t n) const noexcept
        {
            return RefSpanIterator{iterator_ - n};
        }

        constexpr difference_type operator-(const RefSpanIterator &other) const noexcept
        {
            return iterator_ - other.iterator_;
        }

        constexpr RefSpanIterator &operator-=(std::ptrdiff_t n) noexcept
        {
            iterator_ -= n;
            return *this;
        }

        constexpr RefSpanIterator &operator++() noexcept
        {
            ++iterator_;
            return *this;
        }

        constexpr RefSpanIterator operator++(int) noexcept
        {
            auto tmp = *this;
            ++*this;
            return tmp;
        }

        constexpr RefSpanIterator &operator--() noexcept
        {
            --iterator_;
            return *this;
        }

        constexpr RefSpanIterator operator--(int) noexcept
        {
            auto tmp = *this;
            --*this;
            return tmp;
        }

        constexpr friend std::strong_ordering operator<=>(const RefSpanIterator &,
                                                          const RefSpanIterator &) noexcept = default;

      private:
        std::span<T *>::iterator iterator_;
    };

    export template <typename T>
    class RefSpan
    {
      public:
        explicit constexpr RefSpan(std::span<T *> data) : data_{data}
        {
        }

        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            return data_.size();
        }

        [[nodiscard]] constexpr bool empty() const noexcept
        {
            return data_.empty();
        }

        [[nodiscard]] constexpr RefSpanIterator<T> begin() const noexcept
        {
            return RefSpanIterator<T>{data_.begin()};
        }

        [[nodiscard]] constexpr RefSpanIterator<T> end() const noexcept
        {
            return RefSpanIterator<T>{data_.end()};
        }

        [[nodiscard]] constexpr T &operator[](std::size_t n) const noexcept
        {
            return *data_[n];
        }

        [[nodiscard]] constexpr T &front() const noexcept
        {
            return *data_.front();
        }

        [[nodiscard]] constexpr T &back() const noexcept
        {
            return *data_.back();
        }

      private:
        std::span<T *> data_;
    };

    export template <typename T>
    RefSpan(std::span<T *> data) -> RefSpan<T>;
} // namespace prism
