/**
 * @file immutable_array.ixx
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
module;

#include "prism/core/macros.hpp"

export module prism.core:collections.immutable_array;

import :memory.default_allocator;
import :type_traits.basic;

namespace prism
{
    export template <typename T, SimpleAllocator Allocator = DefaultAllocator<T>>
    class ImmutableArray final
    {
        struct Storage;

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

        constexpr ImmutableArray() noexcept = default;

        explicit(false) constexpr ImmutableArray(std::nullptr_t) noexcept
        {
        }

        constexpr ImmutableArray(const ImmutableArray &other) noexcept : storage_{other.storage_}
        {
            add_ref();
        }

        constexpr ImmutableArray(ImmutableArray &&other) noexcept : storage_{std::exchange(other.storage_, nullptr)}
        {
        }

        constexpr ImmutableArray(std::initializer_list<T> values)
            requires std::copy_constructible<T>
            : ImmutableArray{values, Allocator{}}
        {
        }

        constexpr ImmutableArray(std::initializer_list<T> values, const Allocator &allocator)
            requires std::copy_constructible<T>
            : storage_{Storage::create(values.begin(), values.end(), allocator)}
        {
        }

        template <std::input_iterator Iterator, std::sentinel_for<Iterator> Sentinel>
            requires std::constructible_from<T, std::iter_reference_t<Iterator>>
        constexpr ImmutableArray(Iterator first, Sentinel last, const Allocator &allocator = Allocator{})
            : storage_{Storage::create(first, last, allocator)}
        {
        }

        template <std::ranges::input_range Range>
            requires std::constructible_from<T, std::ranges::range_reference_t<Range>>
        constexpr explicit ImmutableArray(std::from_range_t, Range &&range, const Allocator &allocator = Allocator{})
            : ImmutableArray{std::ranges::begin(range), std::ranges::end(range), allocator}
        {
        }

        constexpr ~ImmutableArray() noexcept
        {
            sub_ref();
        }

        constexpr ImmutableArray &operator=(const ImmutableArray &other) noexcept
        {
            if (this == std::addressof(other) || storage_ == other.storage_)
                return *this;

            sub_ref();
            storage_ = other.storage_;
            add_ref();
            return *this;
        }

        constexpr ImmutableArray &operator=(ImmutableArray &&other) noexcept
        {
            if (this == std::addressof(other))
                return *this;

            sub_ref();
            storage_ = std::exchange(other.storage_, nullptr);
            return *this;
        }

        [[nodiscard]] constexpr bool empty() const noexcept
        {
            return size() == 0;
        }

        [[nodiscard]] constexpr size_type size() const noexcept
        {
            return storage_ != nullptr ? storage_->size : 0;
        }

        [[nodiscard]] constexpr size_type max_size() const noexcept
        {
            return std::allocator_traits<Allocator>::max_size(Allocator{});
        }

        [[nodiscard]] constexpr const_pointer data() const noexcept
        {
            return storage_ != nullptr ? storage_->data() : nullptr;
        }

        [[nodiscard]] constexpr const_reference operator[](const size_type index) const noexcept
        {
            return data()[index];
        }

        [[nodiscard]] constexpr const_reference at(const size_type index) const
        {
            if (index >= size())
                throw std::out_of_range{"ImmutableArray index out of range"};

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
            return storage_ != nullptr ? storage_->allocator : Allocator{};
        }

        [[nodiscard]] std::uint32_t use_count() const noexcept
        {
            return storage_ != nullptr ? storage_->ref_count.load(std::memory_order_relaxed) : 0;
        }
        template <std::convertible_to<T> U>
            requires std::copy_constructible<T>
        [[nodiscard]] constexpr ImmutableArray add(U &&value) const
        {
            auto source = as_span();
            return create_with_size(size() + 1,
                                    get_allocator(),
                                    [&](T *output)
                                    {
                                        output =
                                            ImmutableArray::uninitialized_copy(source.begin(), source.end(), output);
                                        std::construct_at(output, std::forward<U>(value));
                                        return size() + 1;
                                    });
        }

        template <std::ranges::input_range Range>
            requires std::copy_constructible<T> && std::convertible_to<std::ranges::range_reference_t<Range>, T>
        [[nodiscard]] constexpr ImmutableArray add_range(Range &&range) const
        {
            if constexpr (std::ranges::sized_range<Range>)
            {
                const auto range_size = static_cast<size_type>(std::ranges::size(range));
                if (range_size == 0)
                    return *this;

                auto source = as_span();
                return create_with_size(size() + range_size,
                                        get_allocator(),
                                        [&](T *output)
                                        {
                                            output = uninitialized_copy(source.begin(), source.end(), output);
                                            for (auto &&item : range)
                                            {
                                                std::construct_at(output, std::forward<decltype(item)>(item));
                                                ++output;
                                            }

                                            return size() + range_size;
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

                return ImmutableArray{std::make_move_iterator(values.begin()),
                                      std::make_move_iterator(values.end()),
                                      get_allocator()};
            }
        }

        template <std::convertible_to<T> U>
            requires std::copy_constructible<T>
        [[nodiscard]] constexpr ImmutableArray insert(const size_type index, U &&value) const
        {
            if (index > size())
                throw std::out_of_range{"ImmutableArray index out of range"};

            auto source = as_span();
            return create_with_size(
                size() + 1,
                get_allocator(),
                [&](T *output)
                {
                    output =
                        ImmutableArray::uninitialized_copy(source.begin(), std::next(source.begin(), index), output);
                    std::construct_at(output, std::forward<U>(value));
                    ++output;
                    output = ImmutableArray::uninitialized_copy(std::next(source.begin(), index), source.end(), output);
                    return size() + 1;
                });
        }

        template <std::ranges::input_range Range>
            requires std::copy_constructible<T> && std::convertible_to<std::ranges::range_reference_t<Range>, T>
        [[nodiscard]] constexpr ImmutableArray insert_range(const size_type index, Range &&range) const
        {
            if (index > size())
                throw std::out_of_range{"ImmutableArray index out of range"};

            if constexpr (std::ranges::sized_range<Range>)
            {
                auto source = as_span();
                return create_with_size(
                    size() + std::ranges::size(range),
                    get_allocator(),
                    [&](T *output)
                    {
                        output = ImmutableArray::uninitialized_copy(source.begin(),
                                                                    std::next(source.begin(), index),
                                                                    output);
                        for (auto &&item : range)
                        {
                            std::construct_at(output, std::forward<decltype(item)>(item));
                            ++output;
                        }
                        output =
                            ImmutableArray::uninitialized_copy(std::next(source.begin(), index), source.end(), output);
                        return size() + 1;
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

                return ImmutableArray{std::make_move_iterator(values.begin()),
                                      std::make_move_iterator(values.end()),
                                      get_allocator()};
            }
        }

        template <std::convertible_to<T> U>
            requires std::copy_constructible<T>
        [[nodiscard]] constexpr ImmutableArray set(const size_type index, U &&value) const
        {
            if (index >= size())
                throw std::out_of_range{"ImmutableArray index out of range"};

            auto source = as_span();
            return create_with_size(
                size(),
                get_allocator(),
                [&](T *output)
                {
                    output =
                        ImmutableArray::uninitialized_copy(source.begin(), std::next(source.begin(), index), output);
                    std::construct_at(output, std::forward<U>(value));
                    ++output;
                    output =
                        ImmutableArray::uninitialized_copy(std::next(source.begin(), index + 1), source.end(), output);
                    return size();
                });
        }

        [[nodiscard]] constexpr ImmutableArray remove_at(const size_type index) const
        {
            if (index >= size())
                throw std::out_of_range{"ImmutableArray remove index out of range"};

            if (size() == 1)
                return {};

            auto source = as_span();
            return ImmutableArray::create_with_size(
                size() - 1,
                get_allocator(),
                [&](T *output)
                {
                    output =
                        ImmutableArray::uninitialized_copy(source.begin(), std::next(source.begin(), index), output);
                    ImmutableArray::uninitialized_copy(std::next(source.begin(), index + 1), source.end(), output);
                    return size() - 1;
                });
        }

        // ReSharper disable once CppMemberFunctionMayBeStatic
        [[nodiscard]] constexpr ImmutableArray clear() const noexcept
        {
            return {};
        }

        void swap(ImmutableArray &other) noexcept
        {
            std::swap(storage_, other.storage_);
        }

        [[nodiscard]] friend bool operator==(const ImmutableArray &lhs, const ImmutableArray &rhs)
            requires std::equality_comparable<T>
        {
            return std::ranges::equal(lhs.as_span(), rhs.as_span());
        }

        [[nodiscard]] friend auto operator<=>(const ImmutableArray &lhs, const ImmutableArray &rhs)
            requires std::three_way_comparable<T>
        {
            return std::lexicographical_compare_three_way(lhs.begin(), lhs.end(), rhs.begin(), rhs.end());
        }

      private:
        using AllocatorTraits = std::allocator_traits<Allocator>;
        using StorageAllocator = AllocatorTraits::template rebind_alloc<std::byte>;
        using StorageAllocatorTraits = std::allocator_traits<StorageAllocator>;

        explicit constexpr ImmutableArray(Storage *storage) noexcept : storage_{storage}
        {
        }

        template <typename First, typename Last>
        static T *uninitialized_copy(First first, Last last, T *output)
        {
            for (; first != last; ++first, ++output)
            {
                std::construct_at(output, *first);
            }

            return output;
        }

        template <std::invocable<T *> Initializer>
            requires std::convertible_to<std::invoke_result_t<Initializer, T *>, size_type>
        [[nodiscard]] static ImmutableArray create_with_size(const size_type size,
                                                             const Allocator &allocator,
                                                             Initializer &&initializer)
        {
            if (size == 0)
                return {};

            auto *storage = Storage::create_uninitialized(size, allocator);

            size_type constructed = 0;
            try
            {
                constructed = std::invoke(std::forward<Initializer>(initializer), storage->data());
            }
            catch (...)
            {
                std::destroy_n(storage->data(), constructed);
                Storage::destroy_uninitialized(storage);
                throw;
            }

            return ImmutableArray{storage};
        }

        struct Storage
        {
            explicit Storage(const size_type size,
                             const size_type allocated,
                             const Allocator &allocator) noexcept(std::is_nothrow_copy_constructible_v<Allocator>)
                : size{size}, allocated_size{allocated}, allocator{allocator}
            {
            }

            [[nodiscard]] T *data() noexcept
            {
                return reinterpret_cast<T *>(reinterpret_cast<std::byte *>(this) + data_offset());
            }

            [[nodiscard]] const T *data() const noexcept
            {
                return reinterpret_cast<const T *>(reinterpret_cast<const std::byte *>(this) + data_offset());
            }

            template <std::input_iterator Iterator, std::sentinel_for<Iterator> Sentinel>
                requires std::constructible_from<T, std::iter_reference_t<Iterator>>
            [[nodiscard]] static Storage *create(Iterator first, Sentinel last, const Allocator &allocator)
            {
                if constexpr (std::sized_sentinel_for<Sentinel, Iterator>)
                {
                    return create_sized(first, last, static_cast<size_type>(last - first), allocator);
                }
                else
                {
                    std::vector<T, Allocator> values{allocator};
                    for (; first != last; ++first)
                    {
                        values.emplace_back(*first);
                    }

                    return create_sized(std::make_move_iterator(values.begin()),
                                        std::make_move_iterator(values.end()),
                                        values.size(),
                                        allocator);
                }
            }

            template <std::input_iterator Iterator, std::sentinel_for<Iterator> Sentinel>
                requires std::constructible_from<T, std::iter_reference_t<Iterator>>
            [[nodiscard]] static Storage *create_sized(Iterator first,
                                                       Sentinel last,
                                                       const size_type size,
                                                       const Allocator &allocator)
            {
                if (size == 0)
                    return nullptr;

                const auto bytes = allocation_size(size);
                StorageAllocator storage_allocator{allocator};
                auto [memory, allocated] = StorageAllocatorTraits::allocate_at_least(storage_allocator, bytes);

                auto *storage = reinterpret_cast<Storage *>(memory);
                std::construct_at(storage, size, allocated, allocator);

                size_type constructed = 0;
                try
                {
                    for (auto *output = storage->data(); first != last; ++first, ++output, ++constructed)
                    {
                        std::construct_at(output, *first);
                    }
                }
                catch (...)
                {
                    std::destroy_n(storage->data(), constructed);
                    std::destroy_at(storage);
                    StorageAllocatorTraits::deallocate(storage_allocator, memory, bytes);
                    throw;
                }

                return storage;
            }

            [[nodiscard]] static Storage *create_uninitialized(const size_type size, const Allocator &allocator)
            {
                const auto bytes = allocation_size(size);
                StorageAllocator storage_allocator{allocator};
                const auto allocation = StorageAllocatorTraits::allocate_at_least(storage_allocator, bytes);
                auto *memory = allocation.ptr;

                auto *storage = reinterpret_cast<Storage *>(memory);
                std::construct_at(storage, size, allocation.count, allocator);
                return storage;
            }

            static void destroy_uninitialized(Storage *storage) noexcept
            {
                auto storage_allocator = StorageAllocator{storage->allocator};
                const auto allocated_bytes = storage->allocated_size;

                std::destroy_at(storage);
                StorageAllocatorTraits::deallocate(storage_allocator,
                                                   reinterpret_cast<std::byte *>(storage),
                                                   allocated_bytes);
            }

            static void destroy(Storage *storage) noexcept
            {
                auto storage_allocator = StorageAllocator{storage->allocator};
                const auto bytes = storage->allocated_size;

                std::destroy_n(storage->data(), storage->size);
                std::destroy_at(storage);
                StorageAllocatorTraits::deallocate(storage_allocator, reinterpret_cast<std::byte *>(storage), bytes);
            }

            [[nodiscard]] static consteval size_type data_offset() noexcept
            {
                constexpr auto alignment = alignof(T);
                return (sizeof(Storage) + alignment - 1) / alignment * alignment;
            }

            [[nodiscard]] static constexpr size_type allocation_size(const size_type count) noexcept
            {
                return data_offset() + sizeof(T) * count;
            }

            std::atomic<std::uint32_t> ref_count{1};
            size_type size = 0;
            size_type allocated_size = 0;
            PRISM_NO_UNIQUE_ADDRESS Allocator allocator;
        };

        constexpr void add_ref() const noexcept
        {
            if (storage_ != nullptr)
                storage_->ref_count.fetch_add(1, std::memory_order_relaxed);
        }

        constexpr void sub_ref() noexcept
        {
            if (storage_ == nullptr)
                return;

            if (storage_->ref_count.fetch_sub(1, std::memory_order_acq_rel) == 1)
                Storage::destroy(storage_);

            storage_ = nullptr;
        }

        Storage *storage_ = nullptr;
    };

    export template <typename InputIt,
                     typename Allocator = DefaultAllocator<typename std::iterator_traits<InputIt>::value_type>>
    ImmutableArray(InputIt, InputIt, Allocator = Allocator{})
        -> ImmutableArray<typename std::iterator_traits<InputIt>::value_type, Allocator>;

    export template <std::ranges::input_range Range,
                     typename Allocator = DefaultAllocator<std::ranges::range_value_t<Range>>>
    ImmutableArray(std::from_range_t, Range &&, Allocator = Allocator{})
        -> ImmutableArray<std::ranges::range_value_t<Range>, Allocator>;

    export template <typename T, SimpleAllocator Allocator>
    void swap(ImmutableArray<T, Allocator> &lhs, ImmutableArray<T, Allocator> &rhs) noexcept
    {
        lhs.swap(rhs);
    }

    export template <std::ranges::input_range Range,
                     SimpleAllocator Allocator = DefaultAllocator<std::ranges::range_value_t<Range>>>
        requires std::copy_constructible<std::ranges::range_value_t<Range>>
    [[nodiscard]] auto make_immutable_array(Range &&range, const Allocator &allocator = Allocator{})
    {
        using T = std::ranges::range_value_t<Range>;
        return ImmutableArray<T, Allocator>{std::from_range, std::forward<Range>(range), allocator};
    }
} // namespace prism
