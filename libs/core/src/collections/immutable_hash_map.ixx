/**
 * @file immutable_hash_map.ixx
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
export module prism.core:collections.immutable_hash_map;

import :memory.default_allocator;
import :type_traits.basic;
import :collections.immutable_hash_table;

namespace prism
{

    export template <typename Key,
                     typename Value,
                     typename Hash = std::hash<Key>,
                     typename KeyEqual = std::equal_to<Key>,
                     SimpleAllocator Allocator = DefaultAllocator<std::pair<Key, Value>>>
    class ImmutableHashMap final
    {
        using Traits = HamtMapTraits<Key, Value, Hash, KeyEqual, Allocator>;
        using Table = Hamt<Traits>;

      public:
        using key_type = Key;
        using mapped_type = Value;
        using value_type = std::pair<Key, Value>;
        using size_type = std::size_t;
        using difference_type = std::ptrdiff_t;
        using hasher = Hash;
        using key_equal = KeyEqual;
        using allocator_type = Allocator;
        using reference = const value_type &;
        using const_reference = const value_type &;
        using pointer = std::allocator_traits<Allocator>::const_pointer;
        using const_pointer = std::allocator_traits<Allocator>::const_pointer;
        using iterator = Table::iterator;
        using const_iterator = Table::const_iterator;

        constexpr ImmutableHashMap() noexcept = default;

        constexpr explicit ImmutableHashMap(Hash hash,
                                            KeyEqual key_equal = KeyEqual{},
                                            Allocator allocator = Allocator{})
            : table_{std::move(hash), std::move(key_equal), std::move(allocator)}
        {
        }

        constexpr ImmutableHashMap(std::initializer_list<value_type> entries)
            requires std::copy_constructible<value_type>
            : ImmutableHashMap{entries, Hash{}, KeyEqual{}, Allocator{}}
        {
        }

        constexpr ImmutableHashMap(std::initializer_list<value_type> entries,
                                   Hash hash,
                                   KeyEqual key_equal = KeyEqual{},
                                   Allocator allocator = Allocator{})
            requires std::copy_constructible<value_type>
            : table_{std::move(hash), std::move(key_equal), std::move(allocator)}
        {
            table_ = table_.set_range(entries);
        }

        template <std::input_iterator Iterator, std::sentinel_for<Iterator> Sentinel>
            requires std::convertible_to<std::iter_reference_t<Iterator>, value_type>
        constexpr ImmutableHashMap(Iterator first,
                                   Sentinel last,
                                   Hash hash = Hash{},
                                   KeyEqual key_equal = KeyEqual{},
                                   Allocator allocator = Allocator{})
            : table_{std::move(hash), std::move(key_equal), std::move(allocator)}
        {
            for (; first != last; ++first)
            {
                table_ = table_.set(static_cast<value_type>(*first));
            }
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, value_type>
        constexpr explicit ImmutableHashMap(std::from_range_t,
                                            Range &&range,
                                            Hash hash = Hash{},
                                            KeyEqual key_equal = KeyEqual{},
                                            Allocator allocator = Allocator{})
            : ImmutableHashMap{std::ranges::begin(range),
                               std::ranges::end(range),
                               std::move(hash),
                               std::move(key_equal),
                               std::move(allocator)}
        {
        }

        [[nodiscard]] constexpr bool empty() const noexcept
        {
            return table_.empty();
        }

        [[nodiscard]] constexpr size_type size() const noexcept
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

        [[nodiscard]] constexpr bool contains(const Key &key) const
        {
            return table_.contains(key);
        }

        template <typename Alt>
            requires AlternativeLookupFor<const Alt &, Key, Hash, KeyEqual, size_type>
        [[nodiscard]] constexpr bool contains(const Alt &key) const
        {
            return table_.contains(key);
        }

        [[nodiscard]] constexpr const mapped_type &operator[](const Key &key) const
        {
            return *try_get(key);
        }

        template <typename Alt>
            requires AlternativeLookupFor<const Alt &, Key, Hash, KeyEqual, size_type>
        [[nodiscard]] constexpr const mapped_type &operator[](const Alt &key) const
        {
            return *try_get(key);
        }

        [[nodiscard]] constexpr const mapped_type &get(const Key &key) const
        {
            auto result = try_get(key);
            if (!result.has_value())
                throw std::out_of_range{"ImmutableHashMap does not contain key"};

            return result.value();
        }

        template <typename Alt>
            requires AlternativeLookupFor<const Alt &, Key, Hash, KeyEqual, size_type>
        [[nodiscard]] constexpr const mapped_type &get(const Alt &key) const
        {
            auto result = try_get(key);
            if (!result.has_value())
                throw std::out_of_range{"ImmutableHashMap does not contain key"};

            return result.value();
        }

        [[nodiscard]] constexpr Optional<const mapped_type &> try_get(const Key &key) const
        {
            return table_.find(key).transform([](const value_type &value) -> const mapped_type &
                                              { return value.second; });
        }

        template <typename Alt>
            requires AlternativeLookupFor<const Alt &, Key, Hash, KeyEqual, size_type>
        [[nodiscard]] constexpr Optional<const mapped_type &> try_get(const Alt &key) const
        {
            return table_.find(key).transform([](const value_type &value) -> const mapped_type &
                                              { return value.second; });
        }

        template <std::convertible_to<Value> U>
        [[nodiscard]] constexpr ImmutableHashMap add(const Key &key, U &&value) const
        {
            if (contains(key))
                throw std::invalid_argument{"ImmutableHashMap already contains key"};

            return set(key, std::forward<U>(value));
        }

        template <std::convertible_to<Value> U>
        [[nodiscard]] constexpr ImmutableHashMap add(Key &&key, U &&value) const
        {
            if (contains(key))
                throw std::invalid_argument{"ImmutableHashMap already contains key"};

            return set(std::move(key), std::forward<U>(value));
        }

        template <std::convertible_to<Value> U>
        [[nodiscard]] constexpr ImmutableHashMap try_add(const Key &key, U &&value) const
        {
            if (contains(key))
                return *this;

            return set(key, std::forward<U>(value));
        }

        template <std::convertible_to<Value> U>
        [[nodiscard]] constexpr ImmutableHashMap try_add(Key &&key, U &&value) const
        {
            if (contains(key))
                return *this;

            return set(std::move(key), std::forward<U>(value));
        }

        template <std::convertible_to<Value> U>
        [[nodiscard]] constexpr ImmutableHashMap set(const Key &key, U &&value) const
        {
            return ImmutableHashMap{table_.set(value_type{key, std::forward<U>(value)})};
        }

        template <std::convertible_to<Value> U>
        [[nodiscard]] constexpr ImmutableHashMap set(Key &&key, U &&value) const
        {
            return ImmutableHashMap{table_.set(value_type{std::move(key), std::forward<U>(value)})};
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, value_type>
        [[nodiscard]] constexpr ImmutableHashMap set_range(Range &&range) const
        {
            return ImmutableHashMap{table_.set_range(std::forward<Range>(range))};
        }

        [[nodiscard]] constexpr ImmutableHashMap remove(const Key &key) const
        {
            return ImmutableHashMap{table_.remove(key)};
        }

        template <typename Alt>
            requires AlternativeLookupFor<const Alt &, Key, Hash, KeyEqual, size_type>
        [[nodiscard]] constexpr ImmutableHashMap remove(const Alt &key) const
        {
            return ImmutableHashMap{table_.remove(key)};
        }

        // ReSharper disable once CppMemberFunctionMayBeStatic
        [[nodiscard]] constexpr ImmutableHashMap clear() const noexcept
        {
            return {};
        }

        constexpr void swap(ImmutableHashMap &other) noexcept
        {
            table_.swap(other.table_);
        }

        [[nodiscard]] constexpr bool same_storage(const ImmutableHashMap &other) const noexcept
        {
            return table_.same_storage(other.table_);
        }

      private:
        constexpr explicit ImmutableHashMap(Table table) : table_{std::move(table)}
        {
        }

        Table table_;
    };

    export template <typename Key, typename Value, typename Hash, typename KeyEqual, typename Allocator>
    constexpr void swap(ImmutableHashMap<Key, Value, Hash, KeyEqual, Allocator> &lhs,
                        ImmutableHashMap<Key, Value, Hash, KeyEqual, Allocator> &rhs) noexcept
    {
        lhs.swap(rhs);
    }

    export template <std::ranges::input_range Range,
                     typename Entry = std::ranges::range_value_t<Range>,
                     typename Key = std::remove_cv_t<std::tuple_element_t<0, Entry>>,
                     typename Value = std::tuple_element_t<1, Entry>,
                     typename Hash = std::hash<Key>,
                     typename KeyEqual = std::equal_to<Key>,
                     SimpleAllocator Allocator = DefaultAllocator<std::pair<Key, Value>>>
        requires std::convertible_to<std::ranges::range_reference_t<Range>, std::pair<Key, Value>>
    [[nodiscard]] constexpr auto make_immutable_hash_map(Range &&range,
                                                         Hash hash = Hash{},
                                                         KeyEqual key_equal = KeyEqual{},
                                                         Allocator allocator = Allocator{})
    {
        return ImmutableHashMap<Key, Value, Hash, KeyEqual, Allocator>{std::from_range,
                                                                       std::forward<Range>(range),
                                                                       std::move(hash),
                                                                       std::move(key_equal),
                                                                       std::move(allocator)};
    }
} // namespace prism
