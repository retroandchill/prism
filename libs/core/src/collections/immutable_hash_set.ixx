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

        constexpr ImmutableHashSet() noexcept = default;

        [[nodiscard]] bool empty() const noexcept
        {
            return table_.empty();
        }

        [[nodiscard]] std::size_t size() const noexcept
        {
            return table_.size();
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
} // namespace prism
