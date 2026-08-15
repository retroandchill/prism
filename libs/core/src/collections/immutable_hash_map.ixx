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
                     SimpleAllocator Allocator = DefaultAllocator<std::pair<const Key, Value>>>
    class ImmutableHashMap final
    {
        using Traits = HamtMapTraits<Key, Value, Hash, KeyEqual, Allocator>;
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

        [[nodiscard]] bool contains(const Key &key) const noexcept
        {
            return table_.contains(key);
        }

      private:
        Table table_;
    };

} // namespace prism
