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

      private:
        Table table_;
    };
} // namespace prism
