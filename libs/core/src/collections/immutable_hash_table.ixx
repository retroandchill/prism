/**
 * @file immutable_hash_table.ixx
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
module;

#include "prism/core/macros.hpp"

export module prism.core:collections.immutable_hash_table;

import std;
import :memory.ref_counted_ptr;
import :collections.immutable_array;

namespace prism
{
    template <typename Traits>
    concept HamtTraits =
        requires {
            typename Traits::key_type;
            typename Traits::entry_type;
            typename Traits::hasher_type;
            typename Traits::key_equal_type;
            typename Traits::allocator_type;
        } && requires(const typename Traits::entry_type &entry,
                      const typename Traits::key_type &key,
                      const typename Traits::hasher_type &hasher,
                      const typename Traits::key_equal_type &key_equal) {
            {
                Traits::key_of(entry)
            } -> std::same_as<const typename Traits::key_type &>;

            {
                hasher(key)
            } -> std::convertible_to<std::size_t>;

            {
                key_equal(Traits::key_of(entry), key)
            } -> std::convertible_to<bool>;
        };

    template <HamtTraits Traits>
    class Hamt final
    {
      public:
        using traits_type = Traits;
        using key_type = Traits::key_type;
        using entry_type = Traits::entry_type;
        using hasher_type = Traits::hasher_type;
        using key_equal_type = Traits::key_equal_type;
        using allocator_type = Traits::allocator_type;
        using size_type = std::size_t;
        using hash_type = std::size_t;

      private:
        static constexpr std::uint32_t bits_per_level = 5;
        static constexpr std::uint32_t fan_out = 1u << bits_per_level;
        static constexpr std::uint32_t index_mask = fan_out - 1;
        static constexpr std::uint32_t max_shift =
            sizeof(hash_type) * std::numeric_limits<unsigned char>::digits / bits_per_level * bits_per_level;

        class Node;
        class CollisionNode;

        using NodePtr = RefCountPtr<const Node>;
        using CollisionNodePtr = RefCountPtr<const CollisionNode>;

        enum class SlotKind : std::uint8_t
        {
            entry,
            node,
            collision
        };

        class Slot final
        {
          public:
            constexpr explicit(false) Slot(const entry_type &entry)
                requires std::copy_constructible<entry_type>
                : kind_{SlotKind::entry}, entry_{entry}
            {
            }

            constexpr explicit(false)
                Slot(entry_type &&entry) noexcept(std::is_nothrow_move_constructible_v<entry_type>)
                : kind_{SlotKind::entry}, entry_{std::move(entry)}
            {
            }

            constexpr explicit(false) Slot(NodePtr node) noexcept : kind_{SlotKind::node}, node_{std::move(node)}
            {
            }

            constexpr explicit(false) Slot(CollisionNodePtr collision) noexcept
                : kind_{SlotKind::collision}, collision_{std::move(collision)}
            {
            }

            constexpr Slot(const Slot &other)
                requires std::copy_constructible<entry_type>
                : kind_{other.kind_}
            {
                construct_from(other);
            }

            constexpr Slot(Slot &&other) noexcept(std::is_nothrow_move_constructible_v<entry_type>) : kind_{other.kind_}
            {
                move_from(std::move(other));
            }

            constexpr ~Slot() noexcept
            {
                destroy();
            }

            constexpr Slot &operator=(const Slot &other)
                requires std::copy_constructible<entry_type>
            {
                if (this == std::addressof(other))
                    return *this;

                destroy();
                kind_ = other.kind_;
                construct_from(other);
                return *this;
            }

            constexpr Slot &operator=(Slot &&other) noexcept(std::is_nothrow_move_constructible_v<entry_type>)
            {
                if (this == std::addressof(other))
                    return *this;

                destroy();
                kind_ = other.kind_;
                move_from(std::move(other));
                return *this;
            }

            [[nodiscard]] constexpr SlotKind kind() const noexcept
            {
                return kind_;
            }

            [[nodiscard]] constexpr bool is_entry() const noexcept
            {
                return kind_ == SlotKind::entry;
            }

            [[nodiscard]] constexpr bool is_node() const noexcept
            {
                return kind_ == SlotKind::node;
            }

            [[nodiscard]] constexpr bool is_collision() const noexcept
            {
                return kind_ == SlotKind::collision;
            }

            [[nodiscard]] constexpr const entry_type &entry() const noexcept
            {
                return entry_;
            }

            [[nodiscard]] constexpr const Node &node() const noexcept
            {
                return *node_;
            }

            [[nodiscard]] constexpr const CollisionNode &collision() const noexcept
            {
                return *collision_;
            }

            [[nodiscard]] constexpr const NodePtr &node_ptr() const noexcept
            {
                return node_;
            }

            [[nodiscard]] constexpr const CollisionNodePtr &collision_ptr() const noexcept
            {
                return collision_;
            }

          private:
            constexpr void construct_from(const Slot &other)
                requires std::copy_constructible<entry_type>
            {
                switch (kind_)
                {
                    case SlotKind::entry:
                        std::construct_at(std::addressof(entry_), other.entry_);
                        break;
                    case SlotKind::node:
                        std::construct_at(std::addressof(node_), other.node_);
                        break;
                    case SlotKind::collision:
                        std::construct_at(std::addressof(collision_), other.collision_);
                        break;
                }
            }

            constexpr void move_from(Slot &&other) noexcept(std::is_nothrow_move_constructible_v<entry_type>)
            {
                switch (kind_)
                {
                    case SlotKind::entry:
                        std::construct_at(std::addressof(entry_), std::move(other.entry_));
                        break;

                    case SlotKind::node:
                        std::construct_at(std::addressof(node_), std::move(other.node_));
                        break;

                    case SlotKind::collision:
                        std::construct_at(std::addressof(collision_), std::move(other.collision_));
                        break;
                }
            }

            constexpr void destroy() noexcept
            {
                switch (kind_)
                {
                    case SlotKind::entry:
                        std::destroy_at(std::addressof(entry_));
                        break;

                    case SlotKind::node:
                        std::destroy_at(std::addressof(node_));
                        break;

                    case SlotKind::collision:
                        std::destroy_at(std::addressof(collision_));
                        break;
                }
            }

            SlotKind kind_;
            union
            {
                entry_type entry_;
                NodePtr node_;
                CollisionNodePtr collision_;
            };
        };

        class CollisionNode final : public IntrusiveRefCounted
        {
          public:
            constexpr CollisionNode(hash_type hash, ImmutableArray<entry_type, allocator_type> entries)
                : hash_{std::move(hash)}, entries_{std::move(entries)}
            {
            }

            [[nodiscard]] constexpr hash_type hash() const noexcept
            {
                return hash_;
            }

            [[nodiscard]] constexpr std::span<const entry_type> entries() const noexcept
            {
                return entries_.as_span();
            }

            template <typename LookupKey>
            [[nodiscard]] Optional<const entry_type &> find(const LookupKey &key,
                                                            const key_equal_type &key_equal) const noexcept
            {
                for (const auto &entry : entries_)
                {
                    if (key_equal(Traits::key_of(entry), key))
                        return entry;
                }

                return std::nullopt;
            }

          private:
            hash_type hash_;
            ImmutableArray<entry_type, allocator_type> entries_;
        };

        class Node final : public IntrusiveRefCounted
        {
          public:
            Node(const std::uint32_t bitmap, ImmutableArray<Slot, typename Traits::slot_allocator_type> slots)
                : bitmap_{bitmap}, slots_{std::move(slots)}
            {
            }

            [[nodiscard]] constexpr std::uint32_t bitmap() const noexcept
            {
                return bitmap_;
            }

            [[nodiscard]] constexpr std::span<const Slot> slots() const noexcept
            {
                return slots_.as_span();
            }

            [[nodiscard]] constexpr bool empty() const noexcept
            {
                return bitmap_ == 0;
            }

            [[nodiscard]] constexpr bool contains_bit(const std::uint32_t bit) const noexcept
            {
                return (bitmap_ & bit) != 0;
            }

            [[nodiscard]] constexpr const Slot &slot_for_bit(const std::uint32_t bit) const noexcept
            {
                return slots_[compact_index(bitmap_, bit)];
            }

          private:
            std::uint32_t bitmap_;
            ImmutableArray<Slot, typename Traits::slot_allocator_type> slots_;
        };

      public:
        constexpr Hamt() = default;

        constexpr explicit Hamt(hasher_type hasher,
                                key_equal_type key_equal,
                                allocator_type allocator = allocator_type{})
            : hasher_{std::move(hasher)}, key_equal_{std::move(key_equal)}, allocator_{std::move(allocator)}
        {
        }

        [[nodiscard]] constexpr bool empty() const noexcept
        {
            return size_ == 0;
        }

        [[nodiscard]] constexpr size_type size() const noexcept
        {
            return size_;
        }

        [[nodiscard]] constexpr const allocator_type &get_allocator() const noexcept
        {
            return allocator_;
        }

        [[nodiscard]] constexpr const hasher_type &hash_function() const noexcept
        {
            return hasher_;
        }

        [[nodiscard]] constexpr const key_equal_type &key_eq() const noexcept
        {
            return key_equal_;
        }

        template <typename LookupKey>
        [[nodiscard]] Optional<const entry_type &> find(const LookupKey &key) const noexcept
        {
            if (root_ == nullptr)
                return std::nullopt;

            return find_in_node(*root_, key, static_cast<hash_type>(hasher_(key)), 0);
        }

        template <typename LookupKey>
        [[nodiscard]] bool contains(const LookupKey &key) const noexcept
        {
            return find(key).has_value();
        }

      private:
        [[nodiscard]] static constexpr std::uint32_t hash_index(const hash_type hash,
                                                                const std::uint32_t shift) noexcept
        {
            return static_cast<std::uint32_t>((hash >> shift) & index_mask);
        }

        [[nodiscard]] static constexpr std::uint32_t bit_for_index(const std::uint32_t index) noexcept
        {
            return 1u << index;
        }

        [[nodiscard]] static constexpr std::uint32_t compact_index(const std::uint32_t bitmap,
                                                                   const std::uint32_t bit) noexcept
        {
            return static_cast<std::uint32_t>(std::popcount(bitmap & (bit - 1u)));
        }

        template <typename LookupKey>
        [[nodiscard]] Optional<const entry_type &> find_in_node(const Node &node,
                                                                const LookupKey &key,
                                                                const hash_type hash,
                                                                const std::uint32_t shift) const noexcept
        {
            const auto index = hash_index(hash, shift);
            const auto bit = bit_for_index(index);

            if (!node.contains_bit(bit))
                return std::nullopt;

            switch (const auto &slot = node.slot_for_bit(bit); slot.kind())
            {
                case SlotKind::entry:
                    if (key_equal_(Traits::key_of(slot.entry()), key))
                        return slot.entry();

                    return std::nullopt;

                case SlotKind::node:
                    return find_in_node(slot.node(), key, hash, shift + bits_per_level);

                case SlotKind::collision:
                    return slot.collision().find(key, key_equal_);
            }

            std::unreachable();
        }

        NodePtr root_;
        size_type size_ = 0;
        PRISM_NO_UNIQUE_ADDRESS hasher_type hasher_{};
        PRISM_NO_UNIQUE_ADDRESS key_equal_type key_equal_{};
        PRISM_NO_UNIQUE_ADDRESS allocator_type allocator_{};
    };

    template <typename Key,
              typename Hash = std::hash<Key>,
              typename KeyEqual = std::equal_to<Key>,
              SimpleAllocator Allocator = DefaultAllocator<Key>>
    struct HamtSetTraits
    {
        using key_type = Key;
        using entry_type = Key;
        using hasher_type = Hash;
        using key_equal_type = KeyEqual;
        using allocator_type = Allocator;
        using slot_allocator_type = std::allocator_traits<Allocator>::template rebind_alloc<entry_type>;

        [[nodiscard]] static constexpr const key_type &key_of(const entry_type &entry) noexcept
        {
            return entry;
        }
    };

    template <typename Key,
              typename Value,
              typename Hash = std::hash<Key>,
              typename KeyEqual = std::equal_to<Key>,
              SimpleAllocator Allocator = DefaultAllocator<std::pair<Key, Value>>>
    struct HamtMapTraits
    {
        using key_type = Key;
        using mapped_type = Value;
        using entry_type = std::pair<Key, Value>;
        using hasher_type = Hash;
        using key_equal_type = KeyEqual;
        using allocator_type = Allocator;
        using slot_allocator_type = std::allocator_traits<Allocator>::template rebind_alloc<entry_type>;

        [[nodiscard]] static constexpr const key_type &key_of(const entry_type &entry) noexcept
        {
            return entry.first;
        }
    };
} // namespace prism
