/**
 * @file list_view.ixx
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
export module prism.core:syntax.list_view;

import std;

namespace prism
{
    template <typename T>
    class SyntaxListViewIterator
    {
      public:
        using difference_type = std::ptrdiff_t;
        using value_type = T::value_type;

        constexpr SyntaxListViewIterator() noexcept = default;

        constexpr explicit SyntaxListViewIterator(const T &view, const std::size_t index = 0)
            : view_{std::addressof(view)}, index_{index}
        {
        }

        [[nodiscard]] constexpr bool operator==(const SyntaxListViewIterator &other) const noexcept
        {
            return view_ == other.view_ && index_ == other.index_;
        }

        [[nodiscard]] constexpr std::strong_ordering operator<=>(const SyntaxListViewIterator &other) const noexcept
        {
            return std::strong_ordering{index_ <=> other.index_};
        }

        [[nodiscard]] constexpr decltype(auto) operator[](const std::size_t n) const noexcept
        {
            return (*view_)[index_ + n];
        }

        [[nodiscard]] constexpr decltype(auto) operator*() const noexcept
        {
            return (*view_)[index_];
        }

        [[nodiscard]] constexpr auto *operator->() const noexcept
            requires std::is_lvalue_reference_v<decltype((*this->view_)[this->index_])>
        {
            return std::addressof((*view_)[index_]);
        }

        [[nodiscard]] constexpr SyntaxListViewIterator operator+(const std::size_t n) const noexcept
        {
            return SyntaxListViewIterator{*view_, index_ + n};
        }

        [[nodiscard]] friend constexpr SyntaxListViewIterator operator+(const std::size_t n,
                                                                        const SyntaxListViewIterator &it) noexcept
        {
            return SyntaxListViewIterator{*it.view_, it.index_ + n};
        }

        constexpr SyntaxListViewIterator &operator+=(const std::size_t n) noexcept
        {
            index_ += n;
            return *this;
        }

        [[nodiscard]] constexpr SyntaxListViewIterator operator-(const std::size_t n) const noexcept
        {
            return SyntaxListViewIterator{*view_, index_ - n};
        }

        [[nodiscard]] constexpr difference_type operator-(const SyntaxListViewIterator &other) const noexcept
        {
            return static_cast<difference_type>(index_) - static_cast<difference_type>(other.index_);
        }

        constexpr SyntaxListViewIterator &operator-=(const std::size_t n) noexcept
        {
            index_ -= n;
            return *this;
        }

        constexpr SyntaxListViewIterator &operator++() noexcept
        {
            ++index_;
            return *this;
        }

        constexpr SyntaxListViewIterator operator++(int) noexcept
        {
            auto tmp = *this;
            ++*this;
            return tmp;
        }

        constexpr SyntaxListViewIterator &operator--() noexcept
        {
            --index_;
            return *this;
        }

        constexpr SyntaxListViewIterator operator--(int) noexcept
        {
            auto tmp = *this;
            --*this;
            return tmp;
        }

      private:
        const T *view_ = nullptr;
        std::size_t index_ = 0;
    };

    template <typename T>
    class SyntaxListView
    {
      protected:
        SyntaxListView() = default;
        ~SyntaxListView() = default;

      public:
        SyntaxListView(const SyntaxListView &) = default;
        SyntaxListView(SyntaxListView &&) = default;
        SyntaxListView &operator=(const SyntaxListView &) = default;
        SyntaxListView &operator=(SyntaxListView &&) = default;

        using value_type = T;

        template <typename Self>
        constexpr bool empty(this const Self &self) noexcept
        {
            return self.size() == 0;
        }

        template <typename Self>
        constexpr decltype(auto) first(this const Self &self)
        {
            return self[0];
        }

        template <typename Self>
        constexpr decltype(auto) last(this const Self &self)
        {
            return self[self.size() - 1];
        }

        template <typename Self>
            requires std::is_lvalue_reference_v<Self>
        constexpr auto begin(this Self &&self) noexcept
        {
            return SyntaxListViewIterator{self};
        }

        template <typename Self>
            requires std::is_lvalue_reference_v<Self>
        constexpr auto end(this Self &&self) noexcept
        {
            return SyntaxListViewIterator{self, self.size()};
        }
    };
} // namespace prism
