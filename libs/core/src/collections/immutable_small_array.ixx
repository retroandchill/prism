/**
 * @file immutable_small_array.ixx
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
module;

#include "prism/core/macros.hpp"

export module prism.core:collections.immutable_small_array;

import :collections.immutable_array;

namespace prism
{
    export template <typename T, std::size_t InlineCapacity, SimpleAllocator Allocator = DefaultAllocator<T>>
    class ImmutableSmallArray
    {
        static_assert(InlineCapacity > 0, "Use ImmutableArray<T, Allocator> for zero inline capacity");

      public:
        using value_type = T;
        using allocator_type = Allocator;
        using size_type = std::size_t;
        using difference_type = std::ptrdiff_t;
        using reference = const T &;
        using const_reference = const T &;
        using pointer = const T *;
        using const_pointer = const T *;
        using iterator = const T *;
        using const_iterator = const T *;
        using reverse_iterator = std::reverse_iterator<iterator>;
        using const_reverse_iterator = std::reverse_iterator<const_iterator>;

        constexpr ImmutableSmallArray() noexcept
        {
        }

        explicit(false) constexpr ImmutableSmallArray(std::nullptr_t) noexcept
        {
        }

        constexpr ImmutableSmallArray(const ImmutableSmallArray &other)
            requires std::copy_constructible<T>
            : allocator_{other.get_allocator()}
        {
            copy_from(other);
        }

        constexpr ImmutableSmallArray(ImmutableSmallArray &&other) noexcept(
            std::is_nothrow_move_constructible_v<T> && std::is_nothrow_move_constructible_v<Allocator>)
            : allocator_{std::move(other.allocator_)}
        {
            move_from(std::move(other));
        }

        constexpr ImmutableSmallArray(std::initializer_list<T> values)
            requires std::copy_constructible<T>
            : ImmutableSmallArray{values, Allocator{}}
        {
        }

        constexpr ImmutableSmallArray(std::initializer_list<T> values, const Allocator &allocator)
            requires std::copy_constructible<T>
            : allocator_{allocator}
        {
            initialize(values.begin(), values.end());
        }

        template <std::input_iterator Iterator, std::sentinel_for<Iterator> Sentinel>
            requires std::constructible_from<T, std::iter_reference_t<Iterator>>
        constexpr ImmutableSmallArray(Iterator first, Sentinel last, const Allocator &allocator = Allocator{})
            : allocator_{allocator}
        {
            initialize(first, last);
        }

        template <std::ranges::input_range Range>
            requires std::constructible_from<T, std::ranges::range_reference_t<Range>>
        constexpr explicit ImmutableSmallArray(std::from_range_t,
                                               Range &&range,
                                               const Allocator &allocator = Allocator{})
            : ImmutableSmallArray{std::ranges::begin(range), std::ranges::end(range), allocator}
        {
        }

        constexpr ~ImmutableSmallArray() noexcept
        {
            destroy();
        }

        constexpr ImmutableSmallArray &operator=(const ImmutableSmallArray &other)
            requires std::copy_constructible<T>
        {
            if (this == std::addressof(other))
                return *this;

            destroy();
            allocator_ = other.get_allocator();
            copy_from(other);
            return *this;
        }

        constexpr ImmutableSmallArray &operator=(ImmutableSmallArray &&other) noexcept(
            std::is_nothrow_move_constructible_v<T> && std::is_nothrow_move_assignable_v<Allocator>)
        {
            if (this == std::addressof(other))
                return *this;

            destroy();
            allocator_ = std::move(other.allocator_);
            move_from(std::move(other));
            return *this;
        }

        [[nodiscard]] constexpr bool empty() const noexcept
        {
            return size() == 0;
        }

        [[nodiscard]] constexpr size_type size() const noexcept
        {
            return is_heap() ? heap().size() : inline_size_;
        }

        [[nodiscard]] constexpr size_type max_size() const noexcept
        {
            return std::allocator_traits<Allocator>::max_size(Allocator{});
        }

        [[nodiscard]] constexpr const_pointer data() const noexcept
        {
            return is_heap() ? heap().data() : inline_data();
        }

        [[nodiscard]] constexpr const_reference operator[](const size_type index) const noexcept
        {
            return data()[index];
        }

        [[nodiscard]] constexpr const_reference at(const size_type index) const
        {
            if (index >= size())
                throw std::out_of_range{"ImmutableSmallArray index out of range"};

            return data()[index];
        }

        [[nodiscard]] constexpr const_reference front() const noexcept
        {
            return (*this)[0];
        }

        [[nodiscard]] constexpr const_reference back() const noexcept
        {
            return (*this)[size() - 1];
        }

        [[nodiscard]] constexpr const_iterator begin() const noexcept
        {
            return data();
        }

        [[nodiscard]] constexpr const_iterator end() const noexcept
        {
            return data() + size();
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

        [[nodiscard]] constexpr std::span<const T> as_span() const noexcept
        {
            return {data(), size()};
        }

        [[nodiscard]] constexpr allocator_type get_allocator() const noexcept
        {
            return allocator_;
        }

        [[nodiscard]] std::uint32_t use_count() const noexcept
        {
            if (is_heap())
                return heap().use_count();

            return inline_size_ == 0 ? 0 : 1;
        }

        [[nodiscard]] constexpr bool is_inline() const noexcept
        {
            return !is_heap();
        }

        [[nodiscard]] constexpr bool is_heap_allocated() const noexcept
        {
            return is_heap();
        }

        template <std::convertible_to<T> U>
            requires std::copy_constructible<T>
        [[nodiscard]] constexpr ImmutableSmallArray add(U &&value) const
        {
            auto source = as_span();
            return create_with_size(size() + 1,
                                    get_allocator(),
                                    [&](auto &builder)
                                    {
                                        builder.append(source.begin(), source.end());
                                        builder.emplace_back(std::forward<U>(value));
                                    });
        }

        template <std::ranges::input_range Range>
            requires std::copy_constructible<T> && std::convertible_to<std::ranges::range_reference_t<Range>, T>
        [[nodiscard]] constexpr ImmutableSmallArray add_range(Range &&range) const
        {
            if constexpr (std::ranges::sized_range<Range>)
            {
                const auto range_size = static_cast<size_type>(std::ranges::size(range));
                if (range_size == 0)
                    return *this;

                auto source = as_span();
                return create_with_size(size() + range_size,
                                        get_allocator(),
                                        [&](auto &builder)
                                        {
                                            builder.append(source.begin(), source.end());
                                            for (auto &&item : range)
                                            {
                                                builder.emplace_back(std::forward<decltype(item)>(item));
                                            }
                                        });
            }
            else
            {
                std::vector<T, Allocator> values{get_allocator()};
                values.reserve(size());

                for (const auto &item : *this)
                {
                    values.emplace_back(item);
                }

                for (auto &&item : range)
                {
                    values.emplace_back(std::forward<decltype(item)>(item));
                }

                return ImmutableSmallArray{std::make_move_iterator(values.begin()),
                                           std::make_move_iterator(values.end()),
                                           get_allocator()};
            }
        }

        template <std::convertible_to<T> U>
            requires std::copy_constructible<T>
        [[nodiscard]] constexpr ImmutableSmallArray insert(const size_type index, U &&value) const
        {
            if (index > size())
                throw std::out_of_range{"ImmutableSmallArray index out of range"};

            auto source = as_span();
            return create_with_size(size() + 1,
                                    get_allocator(),
                                    [&](auto &builder)
                                    {
                                        builder.append(source.begin(), std::next(source.begin(), index));
                                        builder.emplace_back(std::forward<U>(value));
                                        builder.append(std::next(source.begin(), index), source.end());
                                    });
        }

        template <std::ranges::input_range Range>
            requires std::copy_constructible<T> && std::convertible_to<std::ranges::range_reference_t<Range>, T>
        [[nodiscard]] constexpr ImmutableSmallArray insert_range(const size_type index, Range &&range) const
        {
            if (index > size())
                throw std::out_of_range{"ImmutableSmallArray index out of range"};

            if constexpr (std::ranges::sized_range<Range>)
            {
                const auto range_size = static_cast<size_type>(std::ranges::size(range));
                if (range_size == 0)
                    return *this;

                auto source = as_span();
                return create_with_size(size() + range_size,
                                        get_allocator(),
                                        [&](auto &builder)
                                        {
                                            builder.append(source.begin(), std::next(source.begin(), index));
                                            for (auto &&item : range)
                                            {
                                                builder.emplace_back(std::forward<decltype(item)>(item));
                                            }
                                            builder.append(std::next(source.begin(), index), source.end());
                                        });
            }
            else
            {
                std::vector<T, Allocator> values{get_allocator()};
                values.reserve(size());

                for (const auto &item : *this | std::views::take(index))
                {
                    values.emplace_back(item);
                }

                for (auto &&item : range)
                {
                    values.emplace_back(std::forward<decltype(item)>(item));
                }

                for (const auto &item : *this | std::views::drop(index))
                {
                    values.emplace_back(item);
                }

                return ImmutableSmallArray{std::make_move_iterator(values.begin()),
                                           std::make_move_iterator(values.end()),
                                           get_allocator()};
            }
        }

        template <std::convertible_to<T> U>
            requires std::copy_constructible<T>
        [[nodiscard]] constexpr ImmutableSmallArray set(const size_type index, U &&value) const
        {
            if (index >= size())
                throw std::out_of_range{"ImmutableSmallArray index out of range"};

            auto source = as_span();
            return create_with_size(size(),
                                    get_allocator(),
                                    [&](auto &builder)
                                    {
                                        builder.append(source.begin(), std::next(source.begin(), index));
                                        builder.emplace_back(std::forward<U>(value));
                                        builder.append(std::next(source.begin(), index + 1), source.end());
                                    });
        }

        [[nodiscard]] constexpr ImmutableSmallArray remove_at(const size_type index) const
            requires std::copy_constructible<T>
        {
            if (index >= size())
                throw std::out_of_range{"ImmutableSmallArray remove index out of range"};

            if (size() == 1)
                return {};

            auto source = as_span();
            return create_with_size(size() - 1,
                                    get_allocator(),
                                    [&](auto &builder)
                                    {
                                        builder.append(source.begin(), std::next(source.begin(), index));
                                        builder.append(std::next(source.begin(), index + 1), source.end());
                                    });
        }

        // ReSharper disable once CppMemberFunctionMayBeStatic
        [[nodiscard]] constexpr ImmutableSmallArray clear() const noexcept
        {
            return {};
        }

        constexpr void swap(ImmutableSmallArray &other) noexcept(
            std::is_nothrow_move_constructible_v<ImmutableSmallArray> &&
            std::is_nothrow_move_assignable_v<ImmutableSmallArray>)
        {
            if (this == std::addressof(other))
                return;

            auto temp = std::move(*this);
            *this = std::move(other);
            other = std::move(temp);
        }

        [[nodiscard]] friend bool operator==(const ImmutableSmallArray &lhs, const ImmutableSmallArray &rhs)
            requires std::equality_comparable<T>
        {
            return std::ranges::equal(lhs.as_span(), rhs.as_span());
        }

        template <std::size_t OtherSize>
            requires(OtherSize != InlineCapacity)
        [[nodiscard]] friend bool operator==(const ImmutableSmallArray &lhs,
                                             const ImmutableSmallArray<T, OtherSize> &rhs)
        {
            return std::ranges::equal(lhs.as_span(), rhs.as_span());
        }

        [[nodiscard]] friend auto operator<=>(const ImmutableSmallArray &lhs, const ImmutableSmallArray &rhs)
            requires std::three_way_comparable<T>
        {
            return std::lexicographical_compare_three_way(lhs.begin(), lhs.end(), rhs.begin(), rhs.end());
        }

        template <std::size_t OtherSize>
            requires(OtherSize != InlineCapacity)
        [[nodiscard]] friend auto operator<=>(const ImmutableSmallArray &lhs,
                                              const ImmutableSmallArray<T, OtherSize> &rhs)
            requires std::three_way_comparable<T>
        {
            return std::lexicographical_compare_three_way(lhs.begin(), lhs.end(), rhs.begin(), rhs.end());
        }

      private:
        static constexpr size_type heap_marker = std::numeric_limits<size_type>::max();

        using InlineStorage = std::aligned_storage_t<sizeof(T), alignof(T)>;
        using HeapArray = ImmutableArray<T, Allocator>;

        class Builder
        {
          public:
            explicit constexpr Builder(T *data) noexcept : current_{data}
            {
            }

            template <typename... Args>
                requires std::constructible_from<T, Args...>
            constexpr void emplace_back(Args &&...args)
            {
                std::construct_at(current_, std::forward<Args>(args)...);
                ++current_;
                ++constructed_;
            }

            template <typename First, typename Last>
            constexpr void append(First first, Last last)
            {
                for (; first != last; ++first)
                {
                    emplace_back(*first);
                }
            }

            [[nodiscard]] constexpr size_type constructed() const noexcept
            {
                return constructed_;
            }

          private:
            T *current_;
            size_type constructed_ = 0;
        };

        [[nodiscard]] constexpr bool is_heap() const noexcept
        {
            return inline_size_ == heap_marker;
        }

        [[nodiscard]] constexpr T *inline_data() noexcept
        {
            return reinterpret_cast<T *>(inline_storage_);
        }

        [[nodiscard]] constexpr const T *inline_data() const noexcept
        {
            return reinterpret_cast<const T *>(inline_storage_);
        }

        [[nodiscard]] constexpr HeapArray &heap() noexcept
        {
            return heap_storage_;
        }

        [[nodiscard]] constexpr const HeapArray &heap() const noexcept
        {
            return heap_storage_;
        }

        template <std::input_iterator Iterator, std::sentinel_for<Iterator> Sentinel>
            requires std::constructible_from<T, std::iter_reference_t<Iterator>>
        constexpr void initialize(Iterator first, Sentinel last)
        {
            if constexpr (std::sized_sentinel_for<Sentinel, Iterator>)
            {
                initialize_sized(first, last, static_cast<size_type>(last - first));
            }
            else
            {
                std::vector<T, Allocator> values{allocator_};
                for (; first != last; ++first)
                {
                    values.emplace_back(*first);
                }

                initialize_sized(std::make_move_iterator(values.begin()),
                                 std::make_move_iterator(values.end()),
                                 values.size());
            }
        }

        template <std::input_iterator Iterator, std::sentinel_for<Iterator> Sentinel>
            requires std::constructible_from<T, std::iter_reference_t<Iterator>>
        constexpr void initialize_sized(Iterator first, Sentinel last, const size_type count)
        {
            if (count <= InlineCapacity)
            {
                inline_size_ = 0;
                auto *output = inline_data();

                try
                {
                    for (; first != last; ++first, ++output, ++inline_size_)
                    {
                        std::construct_at(output, *first);
                    }
                }
                catch (...)
                {
                    std::destroy_n(inline_data(), inline_size_);
                    inline_size_ = 0;
                    throw;
                }

                return;
            }

            std::construct_at(std::addressof(heap_storage_), first, last, allocator_);
            inline_size_ = heap_marker;
        }

        constexpr void copy_from(const ImmutableSmallArray &other)
            requires std::copy_constructible<T>
        {
            if (other.is_heap())
            {
                std::construct_at(std::addressof(heap_storage_), other.heap());
                inline_size_ = heap_marker;
                return;
            }

            inline_size_ = 0;
            try
            {
                for (const auto &item : other)
                {
                    std::construct_at(std::next(inline_data(), inline_size_), item);
                    ++inline_size_;
                }
            }
            catch (...)
            {
                std::destroy_n(inline_data(), inline_size_);
                inline_size_ = 0;
                throw;
            }
        }

        constexpr void move_from(ImmutableSmallArray &&other) noexcept(std::is_nothrow_move_constructible_v<T>)
        {
            if (other.is_heap())
            {
                std::construct_at(std::addressof(heap_storage_), std::move(other.heap()));
                inline_size_ = heap_marker;

                std::destroy_at(std::addressof(other.heap_storage_));
                other.inline_size_ = 0;
                return;
            }

            inline_size_ = 0;
            for (auto &item : other)
            {
                std::construct_at(std::next(inline_data(), inline_size_), std::move(item));
                ++inline_size_;
            }

            std::destroy_n(other.inline_data(), other.inline_size_);
            other.inline_size_ = 0;
        }

        constexpr void destroy() noexcept
        {
            if (is_heap())
            {
                std::destroy_at(std::addressof(heap_storage_));
                inline_size_ = 0;
                return;
            }

            std::destroy_n(inline_data(), inline_size_);
            inline_size_ = 0;
        }

        struct VectorBuilder
        {
            std::vector<T, Allocator> &values;

            template <typename... Args>
                requires std::constructible_from<T, Args...>
            void emplace_back(Args &&...args)
            {
                values.emplace_back(std::forward<Args>(args)...);
            }

            template <typename First, typename Last>
            void append(First first, Last last)
            {
                for (; first != last; ++first)
                {
                    emplace_back(*first);
                }
            }
        };

        template <typename Initializer>
            requires(std::invocable<Initializer, Builder &> && std::invocable<Initializer, VectorBuilder &>)
        [[nodiscard]] static constexpr ImmutableSmallArray create_with_size(const size_type count,
                                                                            const Allocator &allocator,
                                                                            Initializer &&initializer)
        {
            ImmutableSmallArray result{nullptr};
            result.allocator_ = allocator;

            if (count == 0)
                return result;

            if (count <= InlineCapacity)
            {
                result.inline_size_ = 0;
                Builder builder{result.inline_data()};

                try
                {
                    std::invoke(std::forward<Initializer>(initializer), builder);
                    result.inline_size_ = builder.constructed();
                }
                catch (...)
                {
                    std::destroy_n(result.inline_data(), builder.constructed());
                    result.inline_size_ = 0;
                    throw;
                }

                return result;
            }

            std::vector<T, Allocator> values{allocator};
            values.reserve(count);
            Builder builder{nullptr};

            values.clear();
            values.reserve(count);

            VectorBuilder vector_builder{values};
            std::invoke(std::forward<Initializer>(initializer), vector_builder);

            result.inline_size_ = heap_marker;
            std::construct_at(std::addressof(result.heap_storage_),
                              std::make_move_iterator(values.begin()),
                              std::make_move_iterator(values.end()),
                              allocator);
            return result;
        }

        [[nodiscard]] static constexpr ImmutableSmallArray from_heap(HeapArray heap, const Allocator &allocator)
        {
            ImmutableSmallArray result{nullptr};
            result.allocator_ = allocator;
            result.inline_size_ = heap_marker;
            std::construct_at(std::addressof(result.heap_storage_), std::move(heap));
            return result;
        }

        PRISM_NO_UNIQUE_ADDRESS Allocator allocator_;
        size_type inline_size_ = 0;

        union
        {
            InlineStorage inline_storage_[InlineCapacity];
            HeapArray heap_storage_;
        };
    };

    export template <typename T, std::size_t InlineCapacity, SimpleAllocator Allocator>
    constexpr void swap(ImmutableSmallArray<T, InlineCapacity, Allocator> &lhs,
                        ImmutableSmallArray<T, InlineCapacity, Allocator> &rhs) noexcept(noexcept(lhs.swap(rhs)))
    {
        lhs.swap(rhs);
    }

    export template <std::size_t InlineCapacity,
                     std::ranges::input_range Range,
                     SimpleAllocator Allocator = DefaultAllocator<std::ranges::range_value_t<Range>>>
        requires std::copy_constructible<std::ranges::range_value_t<Range>>
    [[nodiscard]] constexpr auto make_immutable_small_array(Range &&range, const Allocator &allocator = Allocator{})
    {
        using T = std::ranges::range_value_t<Range>;
        return ImmutableSmallArray<T, InlineCapacity, Allocator>{std::from_range,
                                                                 std::forward<Range>(range),
                                                                 allocator};
    }
} // namespace prism
