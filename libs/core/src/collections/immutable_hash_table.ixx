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
                      const typename Traits::entry_type &other_entry,
                      const typename Traits::key_type &key,
                      const typename Traits::hasher_type &hasher,
                      const typename Traits::key_equal_type &key_equal) {
            {
                Traits::key_of(entry)
            } -> std::same_as<const typename Traits::key_type &>;
            {
                Traits::entry_equals(entry, other_entry)
            } -> std::convertible_to<bool>;
            {
                std::invoke(hasher, key)
            } -> std::convertible_to<std::size_t>;

            {
                std::invoke(key_equal, Traits::key_of(entry), key)
            } -> std::convertible_to<bool>;
        };

    template <HamtTraits Traits>
    class Hamt final
    {
      public:
        class Iterator;

        using traits_type = Traits;
        using key_type = Traits::key_type;
        using entry_type = Traits::entry_type;
        using hasher_type = Traits::hasher_type;
        using key_equal_type = Traits::key_equal_type;
        using allocator_type = Traits::allocator_type;
        using size_type = std::size_t;
        using hash_type = std::size_t;
        using iterator = Iterator;
        using const_iterator = Iterator;

      private:
        template <typename LookupKey>
        static constexpr bool can_lookup_with =
            std::invocable<hasher_type, const LookupKey &> &&
            std::convertible_to<std::invoke_result_t<hasher_type, const LookupKey &>, hash_type> &&
            std::invocable<key_equal_type, const key_type &, const LookupKey &> &&
            std::convertible_to<std::invoke_result_t<key_equal_type, const key_type &, const LookupKey &>, bool>;

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

        using SlotAllocator = std::allocator_traits<allocator_type>::template rebind_alloc<Slot>;
        using SlotArray = ImmutableArray<Slot, SlotAllocator>;

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
            Node(const std::uint32_t bitmap, SlotArray slots) : bitmap_{bitmap}, slots_{std::move(slots)}
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
            SlotArray slots_;
        };

        struct SetResult
        {
            NodePtr root;
            bool added = false;
            bool changed = false;
        };

        struct RemoveResult
        {
            NodePtr root;
            bool removed = false;
            bool changed = false;
        };

      public:
        class Iterator final
        {
            struct NodeFrame
            {
                const Node *node = nullptr;
                std::size_t index = 0;
            };

            struct CollisionFrame
            {
                const CollisionNode *collision = nullptr;
                std::size_t index = 0;
            };

            enum class FrameKind : std::uint8_t
            {
                node,
                collision
            };

            struct Frame
            {
                FrameKind kind = FrameKind::node;
                union
                {
                    NodeFrame node{};
                    CollisionFrame collision;
                };

                [[nodiscard]] static Frame for_node(const Node &node) noexcept
                {
                    return Frame{.kind = FrameKind::node, .node = NodeFrame{.node = std::addressof(node)}};
                }

                [[nodiscard]] static Frame for_collision(const CollisionNode &collision) noexcept
                {
                    return Frame{.kind = FrameKind::collision,
                                 .collision = CollisionFrame{.collision = std::addressof(collision)}};
                }
            };

          public:
            using iterator_category = std::forward_iterator_tag;
            using iterator_concept = std::forward_iterator_tag;
            using value_type = entry_type;
            using difference_type = std::ptrdiff_t;
            using reference = const entry_type &;
            using pointer = const entry_type *;

            constexpr Iterator() noexcept = default;

            explicit constexpr Iterator(NodePtr root) : root_{std::move(root)}
            {
                if (root_ == nullptr)
                    return;

                stack_.push_back(Frame::for_node(*root_));
                advance_to_next_entry();
            }

            [[nodiscard]] constexpr reference operator*() const noexcept
            {
                return *current_;
            }

            [[nodiscard]] constexpr pointer operator->() const noexcept
            {
                return current_;
            }

            const_iterator &operator++()
            {
                current_ = nullptr;
                advance_to_next_entry();
                return *this;
            }

            const_iterator operator++(int)
            {
                auto copy = *this;
                ++*this;
                return copy;
            }

            [[nodiscard]] friend bool operator==(const const_iterator &lhs, const const_iterator &rhs) noexcept
            {
                if (lhs.current_ == nullptr || rhs.current_ == nullptr)
                    return lhs.current_ == rhs.current_;

                return lhs.current_ == rhs.current_;
            }

          private:
            void advance_to_next_entry()
            {
                while (!stack_.empty())
                {
                    auto &frame = stack_.back();

                    if (frame.kind == FrameKind::collision)
                    {
                        auto &collision = *frame.collision.collision;
                        if (frame.collision.index < collision.entries().size())
                        {
                            current_ = std::addressof(collision.entries()[frame.collision.index]);
                            ++frame.collision.index;
                            return;
                        }

                        stack_.pop_back();
                        continue;
                    }

                    auto &node = *frame.node.node;
                    if (frame.node.index >= node.slots().size())
                    {
                        stack_.pop_back();
                        continue;
                    }

                    const auto &slot = node.slots()[frame.node.index];
                    ++frame.node.index;

                    switch (slot.kind())
                    {
                        case SlotKind::entry:
                            current_ = std::addressof(slot.entry());
                            return;

                        case SlotKind::node:
                            stack_.push_back(Frame::for_node(slot.node()));
                            break;

                        case SlotKind::collision:
                            stack_.push_back(Frame::for_collision(slot.collision()));
                            break;
                    }
                }

                current_ = nullptr;
            }

            NodePtr root_;
            std::vector<Frame> stack_;
            const entry_type *current_ = nullptr;
        };

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

        [[nodiscard]] constexpr Iterator begin() const noexcept
        {
            return Iterator{root_};
        }

        [[nodiscard]] constexpr Iterator end() const noexcept
        {
            return Iterator{};
        }

        [[nodiscard]] const_iterator cbegin() const
        {
            return begin();
        }

        [[nodiscard]] const_iterator cend() const noexcept
        {
            return end();
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
            requires can_lookup_with<LookupKey>
        [[nodiscard]] constexpr Optional<const entry_type &> find(const LookupKey &key) const noexcept
        {
            if (root_ == nullptr)
                return std::nullopt;

            return find_in_node(*root_, key, static_cast<hash_type>(std::invoke(hasher_, key)), 0);
        }

        template <typename LookupKey>
            requires can_lookup_with<LookupKey>
        [[nodiscard]] constexpr bool contains(const LookupKey &key) const noexcept
        {
            return find(key).has_value();
        }

        [[nodiscard]] constexpr Hamt set(entry_type entry) const
        {
            const auto hash = static_cast<hash_type>(std::invoke(hasher_, Traits::key_of(entry)));

            if (root_ == nullptr)
            {
                auto root = make_node(bit_for_index(hash_index(hash, 0)), make_slot_array({Slot{std::move(entry)}}));
                return Hamt{std::move(root), 1, hasher_, key_equal_, allocator_};
            }

            auto result = set_in_node(*root_, std::move(entry), hash, 0);
            if (!result.changed)
                return *this;

            return Hamt{std::move(result.root), size_ + (result.added ? 1 : 0), hasher_, key_equal_, allocator_};
        }

        template <typename LookupKey>
            requires can_lookup_with<LookupKey>
        [[nodiscard]] constexpr Hamt remove(const LookupKey &key) const
        {
            if (root_ == nullptr)
                return *this;

            const auto hash = static_cast<hash_type>(std::invoke(hasher_, key));
            auto result = remove_from_node(*root_, key, hash, 0);
            if (!result.changed)
                return *this;

            return Hamt{std::move(result.root), size_ - 1, hasher_, key_equal_, allocator_};
        }

        void swap(Hamt &other) noexcept
        {
            std::swap(root_, other.root_);
            std::swap(size_, other.size_);
            std::swap(hasher_, other.hasher_);
            std::swap(key_equal_, other.key_equal_);
            std::swap(allocator_, other.allocator_);
        }

      private:
        constexpr Hamt(NodePtr root,
                       const size_type size,
                       hasher_type hasher,
                       key_equal_type key_equal,
                       allocator_type allocator)
            : root_{std::move(root)}, size_{size}, hasher_{std::move(hasher)}, key_equal_{std::move(key_equal)},
              allocator_{std::move(allocator)}
        {
        }

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

        [[nodiscard]] constexpr SlotArray make_slot_array(std::initializer_list<Slot> slots) const
        {
            return SlotArray{slots, SlotAllocator{allocator_}};
        }

        [[nodiscard]] constexpr SlotArray make_slot_array(std::vector<Slot, SlotAllocator> &&slots) const
        {
            return SlotArray{std::make_move_iterator(slots.begin()),
                             std::make_move_iterator(slots.end()),
                             SlotAllocator{allocator_}};
        }

        [[nodiscard]] constexpr NodePtr make_node(const std::uint32_t bitmap, SlotArray slots) const
        {
            return make_ref_counted<const Node>(bitmap, std::move(slots));
        }

        [[nodiscard]] constexpr CollisionNodePtr make_collision_node(
            hash_type hash,
            ImmutableArray<entry_type, allocator_type> entries) const
        {
            return make_ref_counted<const CollisionNode>(hash, std::move(entries));
        }

        [[nodiscard]] constexpr SlotArray insert_slot(const Node &node, const std::uint32_t bit, Slot slot) const
        {
            const auto insert_index = compact_index(node.bitmap(), bit);
            std::vector<Slot, SlotAllocator> slots{SlotAllocator{allocator_}};
            slots.reserve(node.slots().size() + 1);

            for (std::size_t i = 0; i < insert_index; ++i)
            {
                slots.emplace_back(node.slots()[i]);
            }

            slots.emplace_back(std::move(slot));

            for (std::size_t i = insert_index; i < node.slots().size(); ++i)
            {
                slots.emplace_back(node.slots()[i]);
            }

            return make_slot_array(std::move(slots));
        }

        [[nodiscard]] constexpr SlotArray replace_slot(const Node &node, const std::uint32_t bit, Slot slot) const
        {
            const auto replace_index = compact_index(node.bitmap(), bit);
            std::vector<Slot, SlotAllocator> slots{SlotAllocator{allocator_}};
            slots.reserve(node.slots().size());

            for (std::size_t i = 0; i < node.slots().size(); ++i)
            {
                if (i == replace_index)
                {
                    slots.emplace_back(std::move(slot));
                }
                else
                {
                    slots.emplace_back(node.slots()[i]);
                }
            }

            return make_slot_array(std::move(slots));
        }

        [[nodiscard]] constexpr SlotArray remove_slot(const Node &node, const std::uint32_t bit) const
        {
            const auto remove_index = compact_index(node.bitmap(), bit);
            std::vector<Slot, SlotAllocator> slots{SlotAllocator{allocator_}};
            slots.reserve(node.slots().size() - 1);

            for (std::size_t i = 0; i < node.slots().size(); ++i)
            {
                if (i != remove_index)
                {
                    slots.emplace_back(node.slots()[i]);
                }
            }

            return make_slot_array(std::move(slots));
        }

        [[nodiscard]] constexpr CollisionNodePtr create_collision(hash_type hash,
                                                                  entry_type first,
                                                                  entry_type second) const
        {
            std::vector<entry_type, allocator_type> entries{allocator_};
            entries.reserve(2);
            entries.emplace_back(std::move(first));
            entries.emplace_back(std::move(second));

            return make_collision_node(
                hash,
                ImmutableArray<entry_type, allocator_type>{std::make_move_iterator(entries.begin()),
                                                           std::make_move_iterator(entries.end()),
                                                           allocator_});
        }

        [[nodiscard]] constexpr CollisionNodePtr collision_set(const CollisionNode &collision,
                                                               entry_type entry,
                                                               bool &added,
                                                               bool &changed) const
        {
            std::vector<entry_type, allocator_type> entries{allocator_};
            entries.reserve(collision.entries().size() + 1);

            bool replaced = false;
            bool identical = false;
            for (const auto &existing : collision.entries())
            {
                if (std::invoke(key_equal_, Traits::key_of(existing), Traits::key_of(entry)))
                {
                    replaced = true;
                    if (Traits::entry_equals(existing, entry))
                    {
                        identical = true;
                        entries.emplace_back(existing);
                    }
                    else
                    {
                        entries.emplace_back(std::move(entry));
                    }
                }
                else
                {
                    entries.emplace_back(existing);
                }
            }

            if (identical)
            {
                added = false;
                changed = false;
                return collision.shared_from_this();
            }

            if (!replaced)
            {
                entries.emplace_back(std::move(entry));
                added = true;
            }

            changed = true;
            return make_collision_node(
                collision.hash(),
                ImmutableArray<entry_type, allocator_type>{std::make_move_iterator(entries.begin()),
                                                           std::make_move_iterator(entries.end()),
                                                           allocator_});
        }

        template <typename LookupKey>
        [[nodiscard]] constexpr CollisionNodePtr collision_remove(const CollisionNode &collision,
                                                                  const LookupKey &key,
                                                                  bool &removed,
                                                                  bool &changed) const
        {
            std::vector<entry_type, allocator_type> entries{allocator_};
            entries.reserve(collision.entries().size());

            for (const auto &existing : collision.entries())
            {
                if (std::invoke(key_equal_, Traits::key_of(existing), key))
                {
                    removed = true;
                    changed = true;
                }
                else
                {
                    entries.emplace_back(existing);
                }
            }

            if (!removed)
                return collision.shared_from_this();

            return make_collision_node(
                collision.hash(),
                ImmutableArray<entry_type, allocator_type>{std::make_move_iterator(entries.begin()),
                                                           std::make_move_iterator(entries.end()),
                                                           allocator_});
        }

        [[nodiscard]] constexpr NodePtr merge_entries(entry_type existing,
                                                      hash_type existing_hash,
                                                      entry_type incoming,
                                                      hash_type incoming_hash,
                                                      std::uint32_t shift) const
        {
            if (shift >= max_shift || existing_hash == incoming_hash)
            {
                const auto index = hash_index(existing_hash, shift);
                const auto bit = bit_for_index(index);
                auto collision = create_collision(existing_hash, std::move(existing), std::move(incoming));
                return make_node(bit, make_slot_array({Slot{std::move(collision)}}));
            }

            const auto existing_index = hash_index(existing_hash, shift);
            const auto incoming_index = hash_index(incoming_hash, shift);

            if (existing_index != incoming_index)
            {
                const auto existing_bit = bit_for_index(existing_index);
                const auto incoming_bit = bit_for_index(incoming_index);
                const auto bitmap = existing_bit | incoming_bit;

                std::vector<Slot, SlotAllocator> slots{SlotAllocator{allocator_}};
                slots.reserve(2);

                if (existing_index < incoming_index)
                {
                    slots.emplace_back(std::move(existing));
                    slots.emplace_back(std::move(incoming));
                }
                else
                {
                    slots.emplace_back(std::move(incoming));
                    slots.emplace_back(std::move(existing));
                }

                return make_node(bitmap, make_slot_array(std::move(slots)));
            }

            const auto bit = bit_for_index(existing_index);
            auto child = merge_entries(std::move(existing),
                                       existing_hash,
                                       std::move(incoming),
                                       incoming_hash,
                                       shift + bits_per_level);
            return make_node(bit, make_slot_array({Slot{std::move(child)}}));
        }

        [[nodiscard]] constexpr SetResult set_in_node(const Node &node,
                                                      entry_type entry,
                                                      const hash_type hash,
                                                      const std::uint32_t shift) const
        {
            const auto index = hash_index(hash, shift);
            const auto bit = bit_for_index(index);

            if (!node.contains_bit(bit))
            {
                const auto bitmap = node.bitmap() | bit;
                auto slots = insert_slot(node, bit, Slot{std::move(entry)});
                return {.root = make_node(bitmap, std::move(slots)), .added = true, .changed = true};
            }

            const auto &slot = node.slot_for_bit(bit);

            switch (slot.kind())
            {
                case SlotKind::entry:
                    {
                        const auto &existing = slot.entry();
                        if (std::invoke(key_equal_, Traits::key_of(existing), Traits::key_of(entry)))
                        {
                            if (Traits::entry_equals(existing, entry))
                                return {.root = node.shared_from_this(), .added = false, .changed = false};

                            auto slots = replace_slot(node, bit, Slot{std::move(entry)});
                            return {.root = make_node(node.bitmap(), std::move(slots)),
                                    .added = false,
                                    .changed = true};
                        }

                        const auto existing_hash =
                            static_cast<hash_type>(std::invoke(hasher_, Traits::key_of(existing)));
                        auto child =
                            merge_entries(existing, existing_hash, std::move(entry), hash, shift + bits_per_level);
                        auto slots = replace_slot(node, bit, Slot{std::move(child)});
                        return {.root = make_node(node.bitmap(), std::move(slots)), .added = true, .changed = true};
                    }

                case SlotKind::node:
                    {
                        auto child_result = set_in_node(slot.node(), std::move(entry), hash, shift + bits_per_level);
                        if (!child_result.changed)
                            return {.root = node.shared_from_this(), .added = false, .changed = false};

                        auto slots = replace_slot(node, bit, Slot{std::move(child_result.root)});
                        return {.root = make_node(node.bitmap(), std::move(slots)),
                                .added = child_result.added,
                                .changed = true};
                    }

                case SlotKind::collision:
                    {
                        bool added = false;
                        bool changed = false;
                        auto collision = collision_set(slot.collision(), std::move(entry), added, changed);

                        if (!changed)
                            return {.root = node.shared_from_this(), .added = false, .changed = false};

                        auto slots = replace_slot(node, bit, Slot{std::move(collision)});
                        return {.root = make_node(node.bitmap(), std::move(slots)), .added = added, .changed = true};
                    }
            }

            std::unreachable();
        }

        template <typename LookupKey>
        [[nodiscard]] constexpr RemoveResult remove_from_node(const Node &node,
                                                              const LookupKey &key,
                                                              const hash_type hash,
                                                              const std::uint32_t shift) const
        {
            const auto index = hash_index(hash, shift);
            const auto bit = bit_for_index(index);

            if (!node.contains_bit(bit))
            {
                return {.root = node.shared_from_this(), .removed = false, .changed = false};
            }

            const auto &slot = node.slot_for_bit(bit);

            switch (slot.kind())
            {
                case SlotKind::entry:
                    {
                        if (!std::invoke(key_equal_, Traits::key_of(slot.entry()), key))
                            return {.root = node.shared_from_this(), .removed = false, .changed = false};

                        const auto bitmap = node.bitmap() & ~bit;
                        if (bitmap == 0)
                            return {.root = nullptr, .removed = true, .changed = true};

                        auto slots = remove_slot(node, bit);
                        return {.root = make_node(bitmap, std::move(slots)), .removed = true, .changed = true};
                    }

                case SlotKind::node:
                    {
                        auto child_result = remove_from_node(slot.node(), key, hash, shift + bits_per_level);
                        if (!child_result.changed)
                            return {.root = node.shared_from_this(), .removed = false, .changed = false};

                        if (child_result.root == nullptr)
                        {
                            const auto bitmap = node.bitmap() & ~bit;
                            if (bitmap == 0)
                                return {.root = nullptr, .removed = true, .changed = true};

                            auto slots = remove_slot(node, bit);
                            return {.root = make_node(bitmap, std::move(slots)), .removed = true, .changed = true};
                        }

                        auto slots = replace_slot(node, bit, Slot{std::move(child_result.root)});
                        return {.root = make_node(node.bitmap(), std::move(slots)), .removed = true, .changed = true};
                    }

                case SlotKind::collision:
                    {
                        bool removed = false;
                        bool changed = false;
                        auto collision = collision_remove(slot.collision(), key, removed, changed);
                        if (!changed)
                            return {.root = node.shared_from_this(), .removed = false, .changed = false};

                        if (collision->entries().empty())
                        {
                            const auto bitmap = node.bitmap() & ~bit;
                            if (bitmap == 0)
                                return {.root = nullptr, .removed = true, .changed = true};

                            auto slots = remove_slot(node, bit);
                            return {.root = make_node(bitmap, std::move(slots)), .removed = true, .changed = true};
                        }

                        auto slots = replace_slot(node, bit, Slot{std::move(collision)});
                        return {.root = make_node(node.bitmap(), std::move(slots)), .removed = true, .changed = true};
                    }
            }

            std::unreachable();
        }

        template <typename LookupKey>
        [[nodiscard]] constexpr Optional<const entry_type &> find_in_node(const Node &node,
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
                    if (std::invoke(key_equal_, Traits::key_of(slot.entry()), key))
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

        [[nodiscard]] static constexpr const key_type &key_of(const entry_type &entry) noexcept
        {
            return entry;
        }

        [[nodiscard]] static constexpr bool entry_equals(const entry_type &lhs, const entry_type &rhs) noexcept
        {
            if constexpr (std::equality_comparable<entry_type>)
            {
                return lhs == rhs;
            }
            else
            {
                // Fallback for cases where we can't compare the values for equality
                // In that case just assume everything is unique
                return false;
            }
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

        [[nodiscard]] static constexpr const key_type &key_of(const entry_type &entry) noexcept
        {
            return entry.first;
        }

        [[nodiscard]] static constexpr bool entry_equals(const entry_type &lhs, const entry_type &rhs) noexcept
        {
            if constexpr (std::equality_comparable<mapped_type>)
            {
                return lhs.second == rhs.second;
            }
            else
            {
                // Fallback for cases where we can't compare the values for equality
                // In that case just assume everything is unique
                return false;
            }
        }
    };
} // namespace prism
