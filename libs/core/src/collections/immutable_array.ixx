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

namespace prism
{
    export template <typename T, typename Allocator = DefaultAllocator<T>>
    class ImmutableArray
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

        ImmutableArray(const ImmutableArray &other) noexcept : storage_{other.storage_}
        {
            add_ref();
        }

        ImmutableArray(ImmutableArray &&other) noexcept : storage_{std::exchange(other.storage_, nullptr)}
        {
        }

        ImmutableArray(std::initializer_list<T> values)
            requires std::copy_constructible<T>
            : ImmutableArray{values, Allocator{}}
        {
        }

        ImmutableArray(std::initializer_list<T> values, const Allocator &allocator)
            requires std::copy_constructible<T>
            : storage_{Storage::create(values.begin(), values.end(), allocator)}
        {
        }

        template <std::input_iterator Iterator, std::sentinel_for<Iterator> Sentinel>
            requires std::constructible_from<T, std::iter_reference_t<Iterator>>
        ImmutableArray(Iterator first, Sentinel last, const Allocator &allocator = Allocator{})
            : storage_{Storage::create(first, last, allocator)}
        {
        }

        template <std::ranges::input_range Range>
            requires std::constructible_from<T, std::ranges::range_reference_t<Range>>
        explicit ImmutableArray(Range &&range, const Allocator &allocator = Allocator{})
            : ImmutableArray{std::ranges::begin(range), std::ranges::end(range), allocator}
        {
        }

        ~ImmutableArray() noexcept
        {
            sub_ref();
        }

        ImmutableArray &operator=(const ImmutableArray &other) noexcept
        {
            if (this == std::addressof(other) || storage_ == other.storage_)
                return *this;

            sub_ref();
            storage_ = other.storage_;
            add_ref();
            return *this;
        }

        ImmutableArray &operator=(ImmutableArray &&other) noexcept
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

        [[nodiscard]] const_reference at(const size_type index) const
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

        [[nodiscard]] allocator_type get_allocator() const noexcept
        {
            return storage_ != nullptr ? storage_->allocator : Allocator{};
        }

        [[nodiscard]] std::uint32_t use_count() const noexcept
        {
            return storage_ != nullptr ? storage_->ref_count.load(std::memory_order_relaxed) : 0;
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
                return reinterpret_cast<T *>(this + 1);
            }

            [[nodiscard]] const T *data() const noexcept
            {
                return reinterpret_cast<const T *>(this + 1);
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
                auto [memory, allocated] = StorageAllocatorTraits::allocate(storage_allocator, bytes);

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
                return sizeof(Storage);
            }

            [[nodiscard]] static constexpr size_type allocation_size(const size_type count) noexcept
            {
                return sizeof(Storage) + sizeof(T) * count;
            }

            std::atomic<std::uint32_t> ref_count{1};
            size_type size = 0;
            size_type allocated_size = 0;
            PRISM_NO_UNIQUE_ADDRESS Allocator allocator;
        };

        void add_ref() const noexcept
        {
            if (storage_ != nullptr)
                storage_->ref_count.fetch_add(1, std::memory_order_relaxed);
        }

        void sub_ref() noexcept
        {
            if (storage_ == nullptr)
                return;

            if (storage_->ref_count.fetch_sub(1, std::memory_order_acq_rel) == 1)
                Storage::destroy(storage_);

            storage_ = nullptr;
        }

        Storage *storage_ = nullptr;
    };

    export template <typename T, typename Allocator>
    void swap(ImmutableArray<T, Allocator> &lhs, ImmutableArray<T, Allocator> &rhs) noexcept
    {
        lhs.swap(rhs);
    }

    export template <typename Range, typename Allocator = DefaultAllocator<std::ranges::range_value_t<Range>>>
    [[nodiscard]] auto make_immutable_array(Range &&range, const Allocator &allocator = Allocator{})
    {
        using T = std::ranges::range_value_t<Range>;
        return ImmutableArray<T, Allocator>{std::forward<Range>(range), allocator};
    }
} // namespace prism
