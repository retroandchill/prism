/**
 * @file immutable_string.ixx
 * @author Francesco Corso
 * @date 8/21/2026
 * @brief
 */
export module prism.core:collections.immutable_string;

import :collections.immutable_array;

namespace prism
{

    export template <typename CharT,
                     typename Traits = std::char_traits<CharT>,
                     SimpleAllocator Allocator = DefaultAllocator<CharT>>
    class ImmutableBasicString final
    {
      public:
        using value_type = CharT;
        using traits_type = Traits;
        using allocator_type = Allocator;
        using size_type = ImmutableArray<CharT, Allocator>::size_type;
        using difference_type = ImmutableArray<CharT, Allocator>::difference_type;
        using reference = const CharT &;
        using const_reference = const CharT &;
        using pointer = const CharT *;
        using const_pointer = const CharT *;
        using iterator = const CharT *;
        using const_iterator = const CharT *;
        using reverse_iterator = std::reverse_iterator<const_iterator>;
        using const_reverse_iterator = std::reverse_iterator<const_iterator>;
        using view_type = std::basic_string_view<CharT, Traits>;

        static constexpr size_type npos = static_cast<size_type>(-1);

        constexpr ImmutableBasicString() noexcept(std::is_nothrow_default_constructible_v<Allocator>) = default;

        explicit constexpr ImmutableBasicString(const Allocator &allocator) noexcept : data_{allocator}
        {
        }

        constexpr ImmutableBasicString(std::initializer_list<CharT> chars, const Allocator &allocator = Allocator{})
            : data_{chars, allocator}
        {
        }

        constexpr explicit(false) ImmutableBasicString(view_type view, const Allocator &allocator = Allocator{})
            : data_{view.begin(), view.end(), allocator}
        {
        }

        constexpr explicit(false) ImmutableBasicString(const CharT *text, const Allocator &allocator = Allocator{})
            : ImmutableBasicString{view_type{text}, allocator}
        {
        }

        template <std::input_iterator Iterator, std::sentinel_for<Iterator> Sentinel>
            requires std::constructible_from<CharT, std::iter_reference_t<Iterator>>
        constexpr ImmutableBasicString(Iterator first, Sentinel last, const Allocator &allocator = Allocator{})
            : data_{first, last, allocator}
        {
        }

        [[nodiscard]] constexpr bool empty() const noexcept
        {
            return data_.empty();
        }

        [[nodiscard]] constexpr size_type size() const noexcept
        {
            return data_.size();
        }

        [[nodiscard]] constexpr size_type length() const noexcept
        {
            return size();
        }

        [[nodiscard]] constexpr size_type max_size() const noexcept
        {
            return data_.max_size();
        }

        [[nodiscard]] constexpr const_pointer data() const noexcept
        {
            return data_.data();
        }

        [[nodiscard]] constexpr const_reference operator[](const size_type index) const noexcept
        {
            return data_[index];
        }

        [[nodiscard]] constexpr const_reference at(const size_type index) const
        {
            return data_.at(index);
        }

        [[nodiscard]] constexpr const_reference front() const noexcept
        {
            return data_.front();
        }

        [[nodiscard]] constexpr const_reference back() const noexcept
        {
            return data_.back();
        }

        [[nodiscard]] constexpr const_iterator begin() const noexcept
        {
            return data_.begin();
        }

        [[nodiscard]] constexpr const_iterator end() const noexcept
        {
            return data_.end();
        }

        [[nodiscard]] constexpr const_iterator cbegin() const noexcept
        {
            return begin();
        }

        [[nodiscard]] constexpr const_iterator cend() const noexcept
        {
            return end();
        }

        [[nodiscard]] constexpr const_reverse_iterator rbegin() const noexcept
        {
            return const_reverse_iterator{end()};
        }

        [[nodiscard]] constexpr const_reverse_iterator rend() const noexcept
        {
            return const_reverse_iterator{begin()};
        }

        [[nodiscard]] constexpr const_reverse_iterator crbegin() const noexcept
        {
            return rbegin();
        }

        [[nodiscard]] constexpr const_reverse_iterator crend() const noexcept
        {
            return rend();
        }

        [[nodiscard]] constexpr allocator_type get_allocator() const noexcept
        {
            return data_.get_allocator();
        }

        [[nodiscard]] constexpr std::span<const CharT> as_span() const noexcept
        {
            return data_.as_span();
        }

        [[nodiscard]] constexpr view_type view() const noexcept
        {
            return view_type{data(), size()};
        }

        [[nodiscard]] constexpr operator view_type() const noexcept
        {
            return view();
        }

        [[nodiscard]] std::uint32_t use_count() const noexcept
        {
            return data_.use_count();
        }

        [[nodiscard]] constexpr ImmutableBasicString clear() const noexcept
        {
            return ImmutableBasicString{get_allocator()};
        }

        [[nodiscard]] constexpr ImmutableBasicString substr(const size_type pos = 0, const size_type count = npos) const
        {
            if (pos > size())
                throw std::out_of_range{"ImmutableBasicString position out of range"};

            const auto actual_count = std::min(count, size() - pos);
            return ImmutableBasicString{view().substr(pos, actual_count), get_allocator()};
        }

        [[nodiscard]] constexpr ImmutableBasicString append(view_type suffix) const
        {
            return ImmutableBasicString{data_.add_range(suffix), get_allocator()};
        }

        [[nodiscard]] constexpr ImmutableBasicString append(const ImmutableBasicString &suffix) const
        {
            return append(suffix.view());
        }

        template <size_type N>
        [[nodiscard]] constexpr ImmutableBasicString append(const CharT (&suffix)[N]) const
        {
            return append(view_type{suffix});
        }

        [[nodiscard]] constexpr ImmutableBasicString push_back(const CharT ch) const
        {
            return ImmutableBasicString{data_.add(ch), get_allocator()};
        }

        [[nodiscard]] constexpr ImmutableBasicString insert(const size_type index, view_type text) const
        {
            return ImmutableBasicString{data_.insert_range(index, text), get_allocator()};
        }

        [[nodiscard]] constexpr ImmutableBasicString insert(const size_type index, const CharT ch) const
        {
            return ImmutableBasicString{data_.insert(index, ch), get_allocator()};
        }

        [[nodiscard]] constexpr ImmutableBasicString erase(const size_type index = 0,
                                                           const size_type count = npos) const
        {
            if (index > size())
                throw std::out_of_range{"ImmutableBasicString erase position out of range"};

            const auto actual_count = std::min(count, size() - index);
            if (actual_count == 0)
                return *this;

            std::vector<CharT> result;
            result.reserve(size() - actual_count);

            for (size_type i = 0; i < index; ++i)
                result.push_back((*this)[i]);

            for (size_type i = index + actual_count; i < size(); ++i)
                result.push_back((*this)[i]);

            return ImmutableBasicString{result.begin(), result.end(), get_allocator()};
        }

        [[nodiscard]] constexpr ImmutableBasicString replace(const size_type index,
                                                             const size_type count,
                                                             view_type replacement) const
        {
            if (index > size())
                throw std::out_of_range{"ImmutableBasicString replace position out of range"};

            const auto actual_count = std::min(count, size() - index);
            return erase(index, actual_count).insert(index, replacement);
        }

        [[nodiscard]] constexpr ImmutableBasicString set(const size_type index, const CharT ch) const
        {
            return ImmutableBasicString{data_.set(index, ch), get_allocator()};
        }

        [[nodiscard]] constexpr size_type find(view_type needle, const size_type pos = 0) const noexcept
        {
            return view().find(needle, pos);
        }

        [[nodiscard]] constexpr size_type find(const CharT ch, const size_type pos = 0) const noexcept
        {
            return view().find(ch, pos);
        }

        [[nodiscard]] constexpr size_type rfind(view_type needle, const size_type pos = npos) const noexcept
        {
            return view().rfind(needle, pos);
        }

        [[nodiscard]] constexpr bool starts_with(view_type prefix) const noexcept
        {
            return view().starts_with(prefix);
        }

        [[nodiscard]] constexpr bool starts_with(const CharT ch) const noexcept
        {
            return view().starts_with(ch);
        }

        [[nodiscard]] constexpr bool ends_with(view_type suffix) const noexcept
        {
            return view().ends_with(suffix);
        }

        [[nodiscard]] constexpr bool ends_with(const CharT ch) const noexcept
        {
            return view().ends_with(ch);
        }

        [[nodiscard]] constexpr bool contains(view_type needle) const noexcept
        {
            return view().contains(needle);
        }

        [[nodiscard]] constexpr bool contains(const CharT ch) const noexcept
        {
            return view().contains(ch);
        }

        void swap(ImmutableBasicString &other) noexcept
        {
            data_.swap(other.data_);
        }

        [[nodiscard]] friend bool operator==(const ImmutableBasicString &lhs, const ImmutableBasicString &rhs) noexcept
        {
            return lhs.view() == rhs.view();
        }

        [[nodiscard]] friend auto operator<=>(const ImmutableBasicString &lhs, const ImmutableBasicString &rhs) noexcept
        {
            return lhs.view() <=> rhs.view();
        }

        [[nodiscard]] friend ImmutableBasicString operator+(const ImmutableBasicString &lhs,
                                                            const ImmutableBasicString &rhs)
        {
            return lhs.append(rhs);
        }

        [[nodiscard]] friend ImmutableBasicString operator+(const ImmutableBasicString &lhs, view_type rhs)
        {
            return lhs.append(rhs);
        }

        [[nodiscard]] friend ImmutableBasicString operator+(view_type lhs, const ImmutableBasicString &rhs)
        {
            return ImmutableBasicString{lhs, rhs.get_allocator()}.append(rhs);
        }

      private:
        constexpr explicit ImmutableBasicString(ImmutableArray<CharT, Allocator> data, const Allocator &allocator)
            : data_{std::move(data)}
        {
            (void)allocator;
        }

        ImmutableArray<CharT, Allocator> data_;
    };

    export template <typename CharT, SimpleAllocator Allocator, typename Traits>
    void swap(ImmutableBasicString<CharT, Allocator, Traits> &lhs,
              ImmutableBasicString<CharT, Allocator, Traits> &rhs) noexcept
    {
        lhs.swap(rhs);
    }

    export using ImmutableString = ImmutableBasicString<char>;
    export using ImmutableWString = ImmutableBasicString<wchar_t>;
    export using ImmutableU8String = ImmutableBasicString<char8_t>;
    export using ImmutableU16String = ImmutableBasicString<char16_t>;
    export using ImmutableU32String = ImmutableBasicString<char32_t>;

} // namespace prism

template <typename CharT, typename Allocator, typename Traits>
struct std::formatter<prism::ImmutableBasicString<CharT, Allocator, Traits>, CharT>
    : std::formatter<std::basic_string_view<CharT, Traits>, CharT>
{
    template <typename FormatContext>
    auto format(const prism::ImmutableBasicString<CharT, Allocator, Traits> &value, FormatContext &ctx) const
    {
        return std::formatter<std::basic_string_view<CharT, Traits>, CharT>::format(value.view(), ctx);
    }
};
