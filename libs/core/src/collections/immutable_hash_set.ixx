/**
 * @file immutable_hash_set.ixx
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
export module prism.core:collections.immutable_hash_set;

import :collections.immutable_hash_table;

namespace prism
{
    export template <typename T,
                     typename Hash = std::hash<T>,
                     typename KeyEqual = std::equal_to<T>,
                     SimpleAllocator Allocator = DefaultAllocator<T>>
    class ImmutableHashSet final
    {
        using Traits = HamtSetTraits<T, Hash, KeyEqual, Allocator>;
        using Table = Hamt<Traits>;

      public:
        using key_type = T;
        using value_type = T;
        using size_type = std::size_t;
        using difference_type = std::ptrdiff_t;
        using hasher = Hash;
        using key_equal = KeyEqual;
        using allocator_type = Allocator;
        using reference = const T &;
        using const_reference = const T &;
        using pointer = const T *;
        using const_pointer = const T *;
        using iterator = Table::iterator;
        using const_iterator = Table::const_iterator;

        constexpr ImmutableHashSet() noexcept = default;

        constexpr explicit ImmutableHashSet(Hash hash, KeyEqual key_equal = {}, Allocator allocator = {}) noexcept
            : table_{std::move(hash), std::move(key_equal), std::move(allocator)}
        {
        }

        constexpr ImmutableHashSet(std::initializer_list<T> values)
            requires std::copy_constructible<T>
            : ImmutableHashSet{values, Hash{}, KeyEqual{}, Allocator{}}
        {
        }

        constexpr ImmutableHashSet(std::initializer_list<T> values,
                                   Hash hash,
                                   KeyEqual key_equal = KeyEqual{},
                                   Allocator allocator = Allocator{})
            requires std::copy_constructible<T>
            : table_{Hash{std::move(hash)}, KeyEqual{std::move(key_equal)}, Allocator{std::move(allocator)}}
        {
            table_ = table_.set_range(values);
        }

        template <std::input_iterator Iterator, std::sentinel_for<Iterator> Sentinel>
            requires std::convertible_to<std::iter_reference_t<Iterator>, T>
        constexpr ImmutableHashSet(Iterator first,
                                   Sentinel last,
                                   Hash hash = Hash{},
                                   KeyEqual key_equal = KeyEqual{},
                                   Allocator allocator = Allocator{})
            : table_{std::move(hash), std::move(key_equal), std::move(allocator)}
        {
            for (; first != last; ++first)
            {
                table_ = table_.set(static_cast<T>(*first));
            }
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, T>
        constexpr explicit ImmutableHashSet(std::from_range_t,
                                            Range &&range,
                                            Hash hash = Hash{},
                                            KeyEqual key_equal = KeyEqual{},
                                            Allocator allocator = Allocator{})
            : ImmutableHashSet{std::ranges::begin(range),
                               std::ranges::end(range),
                               std::move(hash),
                               std::move(key_equal),
                               std::move(allocator)}
        {
        }

        [[nodiscard]] bool empty() const noexcept
        {
            return table_.empty();
        }

        [[nodiscard]] std::size_t size() const noexcept
        {
            return table_.size();
        }

        [[nodiscard]] constexpr const_iterator begin() const noexcept
        {
            return table_.begin();
        }

        [[nodiscard]] constexpr const_iterator end() const noexcept
        {
            return table_.end();
        }

        [[nodiscard]] const_iterator cbegin() const
        {
            return begin();
        }

        [[nodiscard]] const_iterator cend() const noexcept
        {
            return end();
        }

        [[nodiscard]] bool contains(const T &key) const noexcept
        {
            return table_.contains(key);
        }

        template <typename Alt>
            requires AlternativeLookupFor<const Alt &, T, Hash, KeyEqual, size_type>
        [[nodiscard]] bool contains(const Alt &key) const noexcept
        {
            return table_.contains(key);
        }

        template <std::convertible_to<T> U>
        [[nodiscard]] constexpr ImmutableHashSet add(U &&value) const
        {
            return ImmutableHashSet{table_.set(std::forward<U>(value))};
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, T>
        [[nodiscard]] constexpr ImmutableHashSet add_range(Range &&range) const
        {
            return ImmutableHashSet{table_.set_range(std::forward<Range>(range))};
        }

        [[nodiscard]] constexpr ImmutableHashSet remove(const T &key) const
        {
            return ImmutableHashSet{table_.remove(key)};
        }

        template <typename Alt>
            requires AlternativeLookupFor<const Alt &, T, Hash, KeyEqual, size_type>
        [[nodiscard]] constexpr ImmutableHashSet remove(const Alt &key) const
        {
            return ImmutableHashSet{table_.remove(key)};
        }

        // ReSharper disable once CppMemberFunctionMayBeStatic
        [[nodiscard]] constexpr ImmutableHashSet clear() const noexcept
        {
            return {};
        }

        [[nodiscard]] constexpr bool same_storage(const ImmutableHashSet &other) const noexcept
        {
            return table_.same_storage(other.table_);
        }

        constexpr void swap(ImmutableHashSet &other) noexcept
        {
            table_.swap(other.table_);
        }

      private:
        constexpr explicit ImmutableHashSet(Table table) noexcept : table_{std::move(table)}
        {
        }

        Table table_;
    };

    export template <typename T, typename Hash, typename KeyEqual, typename Allocator>
    void swap(ImmutableHashSet<T, Hash, KeyEqual, Allocator> &lhs,
              ImmutableHashSet<T, Hash, KeyEqual, Allocator> &rhs) noexcept
    {
        lhs.swap(rhs);
    }

    export template <std::ranges::input_range Range,
                     typename Hash = std::hash<std::ranges::range_value_t<Range>>,
                     typename KeyEqual = std::equal_to<std::ranges::range_value_t<Range>>,
                     SimpleAllocator Allocator = DefaultAllocator<std::ranges::range_value_t<Range>>>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, std::ranges::range_value_t<Range>>
    [[nodiscard]] constexpr auto make_immutable_hash_set(Range &&range,
                                                         Hash hash = Hash{},
                                                         KeyEqual key_equal = KeyEqual{},
                                                         Allocator allocator = Allocator{})
    {
        using T = std::ranges::range_value_t<Range>;
        return ImmutableHashSet<T, Hash, KeyEqual, Allocator>{std::from_range,
                                                              std::forward<Range>(range),
                                                              std::move(hash),
                                                              std::move(key_equal),
                                                              std::move(allocator)};
    }
} // namespace prism
