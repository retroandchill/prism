/**
 * @file name.cpp
 * @author Francesco Corso
 * @date 6/28/2026
 * @brief
 */
module;

#include "prism/core/names.hpp"

#include <cassert>

module prism.core.strings.name;

import prism.core.memory.alignment;
import xxhash;
import prism.core.util;

namespace prism
{
    namespace
    {
        constexpr std::uint32_t name_max_block_bits = 13;
        constexpr std::uint32_t name_block_offset_bits = 16;
        constexpr std::uint32_t name_max_blocks = 1 << name_max_block_bits;
        constexpr std::uint32_t name_block_offsets = 1 << name_block_offset_bits;
    } // namespace

    struct DefaultNameStore
    {
        static std::uint32_t try_place(std::byte *, const std::uint32_t size, const std::string_view name)
        {
            const auto bytes = get_default_size(static_cast<std::uint32_t>(name.size()));
            return bytes <= size ? bytes : 0;
        }

        static void finalize(std::byte *name_data, const std::string_view name)
        {
            std::memcpy(name_data, name.data(), name.length());
        }

        static std::uint32_t get_size(const NameEntry &entry)
        {
            return get_default_size(entry.name_length());
        }

      private:
        static std::uint32_t get_default_size(const std::uint32_t length)
        {
            const auto header_size = NameEntry::data_offset();
            const auto foot_size = length * sizeof(char);
            return header_size + foot_size;
        }
    };

#ifdef PRISM_CUSTOM_NAME_STORE
#include PRISM_CUSTOM_NAME_STORE
#else
    using NameStore = DefaultNameStore;
#endif

    struct NameEntryHandle
    {
        std::uint32_t block = 0;
        std::uint32_t offset = 0;

        constexpr NameEntryHandle(const std::uint32_t block, const std::uint32_t offset) : block(block), offset(offset)
        {
            assert(block < name_max_blocks);
            assert(offset < name_block_offsets);
        }

        constexpr explicit(false) NameEntryHandle(const NameEntryId id)
            : block{id.to_unstable_int() >> name_block_offset_bits},
              offset{id.to_unstable_int() & name_block_offset_bits - 1}
        {
        }

        constexpr explicit(false) operator NameEntryId() const
        {
            return NameEntryId::from_unstable_int(block << name_block_offset_bits | offset);
        }
    };

    struct NameEntryMemoryDeleter
    {
        void operator()(std::byte *ptr) const noexcept;
    };

    using NameEntryBlockPtr = std::unique_ptr<std::byte[], NameEntryMemoryDeleter>;

    struct NameSlot
    {
        static constexpr std::uint32_t entry_id_bits = name_max_block_bits + name_block_offset_bits;
        static constexpr std::uint32_t entry_id_mask = (1 << entry_id_bits) - 1;
        static constexpr std::uint32_t probe_hash_shift = entry_id_bits;
        static constexpr std::uint32_t probe_hash_mask = ~entry_id_mask;

        constexpr NameSlot() = default;

        constexpr NameSlot(const NameEntryId value, const std::uint32_t probe_hash)
            : id_and_hash_{value.to_unstable_int() | probe_hash}
        {
            assert((value.to_unstable_int() & probe_hash_mask) == 0 && (probe_hash & entry_id_mask) == 0 && used());
        }

        [[nodiscard]] constexpr NameEntryId id() const noexcept
        {
            return NameEntryId::from_unstable_int(id_and_hash_ & entry_id_mask);
        }

        [[nodiscard]] constexpr std::uint32_t probe_hash() const noexcept
        {
            return id_and_hash_ & probe_hash_mask;
        }

        [[nodiscard]] constexpr bool used() const noexcept
        {
            return id_and_hash_ != 0;
        }

        constexpr friend bool operator==(NameSlot lhs, NameSlot rhs) noexcept = default;

      private:
        std::uint32_t id_and_hash_ = 0;
    };

    class NameEntryAllocator
    {
      public:
        static constexpr std::uint32_t stride = alignof(NameEntry);
        static constexpr std::uint32_t block_size_bytes = stride * name_block_offsets;

        NameEntryAllocator()
        {
            blocks_[0] = alloc_block();
        }

        void reserve_blocks(const std::uint32_t count)
        {
            std::unique_lock lock{lock_};

            for (std::uint32_t i = count - 1; i > current_block_ && blocks_[i] == nullptr; --i)
            {
                blocks_[i] = alloc_block();
            }
        }

        [[nodiscard]] constexpr std::span<const NameEntryBlockPtr> debug_memory() const noexcept
        {
            return blocks_;
        }

        NameEntryHandle allocate(const std::uint32_t bytes)
        {
            assert(current_byte_cursor_ % stride == 0);
            assert(current_byte_cursor_ + bytes <= block_size_bytes);

            auto byte_offset = current_byte_cursor_;
            auto step = align(bytes, alignof(NameEntry));
            wasted_bytes_ += step - bytes;
            current_byte_cursor_ += step;
            return NameEntryHandle{current_block_, byte_offset / stride};
        }

        NameEntryHandle allocate_regular(const std::string_view name)
        {
            std::unique_lock lock{lock_};

            auto bytes = block_size_bytes < current_byte_cursor_
                             ? 0
                             : NameStore::try_place(std::next(blocks_[current_block_].get(), current_byte_cursor_),
                                                    block_size_bytes - current_byte_cursor_,
                                                    name);
            if (bytes == 0)
            {
                allocate_new_block();
                bytes = NameStore::try_place(std::next(blocks_[current_block_].get(), current_byte_cursor_),
                                             block_size_bytes - current_byte_cursor_,
                                             name);
                assert(bytes != 0);
            }

            return allocate(bytes);
        }

        NameEntryHandle create(std::string_view name, NameEntryHeader header)
        {
            NameEntryHandle handle = allocate_regular(name);
            auto &entry = resolve(handle);
            entry.header_ = header;
            NameStore::finalize(entry.data_, name);
            return handle;
        }

        NameEntry &resolve(const NameEntryHandle handle) const
        {
            return *reinterpret_cast<NameEntry *>(std::next(blocks_[handle.block].get(), stride * handle.offset));
        }

        [[nodiscard]] constexpr std::uint32_t num_blocks() const noexcept
        {
            return current_block_ + 1;
        }

        [[nodiscard]] constexpr std::size_t wasted_bytes() const noexcept
        {
            return wasted_bytes_;
        }

      private:
        static NameEntryBlockPtr alloc_block()
        {
            // ReSharper disable once CppRedundantQualifier
            NameEntryBlockPtr ptr{static_cast<std::byte *>(::operator new(block_size_bytes, std::align_val_t{stride}))};
            std::memset(ptr.get(), 0, block_size_bytes);
            return ptr;
        }

        void allocate_new_block()
        {
            if (current_byte_cursor_ + NameEntry::data_offset() <= block_size_bytes)
            {
                auto *terminator =
                    reinterpret_cast<NameEntry *>(std::next(blocks_[current_block_].get(), current_byte_cursor_));
                terminator->header_.length = 0;
            }

            if (current_byte_cursor_ < block_size_bytes)
            {
                wasted_bytes_ += block_size_bytes - current_byte_cursor_;
            }

            ++current_block_;
            current_byte_cursor_ = 0;

            assert(current_block_ < name_max_blocks);
            if (blocks_[current_block_] == nullptr)
            {
                blocks_[current_block_] = alloc_block();
            }
        }

        mutable std::shared_mutex lock_;
        std::uint32_t current_block_ = 0;
        std::uint32_t current_byte_cursor_ = 0;
        std::array<NameEntryBlockPtr, name_max_blocks> blocks_;
        std::size_t wasted_bytes_ = 0;
    };

    void NameEntryMemoryDeleter::operator()(std::byte *ptr) const noexcept
    {
        // ReSharper disable once CppRedundantQualifier
        ::operator delete(ptr, std::align_val_t{NameEntryAllocator::stride});
    }

    constexpr std::uint32_t name_pool_shard_bits = 10;
    constexpr std::uint32_t name_pool_shards = 1 << name_pool_shard_bits;
    constexpr std::uint32_t name_pool_initial_slot_bits = 8;
    constexpr std::uint32_t name_pool_initial_slots_per_shard = 1 << name_pool_initial_slot_bits;

    struct NameHash
    {
        std::uint32_t shard_index;
        std::uint32_t unmasked_slot_index;
        std::uint32_t slot_probe_hash;
        NameEntryHeader entry_probe_header;

        static constexpr std::uint32_t shard_mask = name_pool_shards - 1;

        explicit NameHash(std::string_view str) : NameHash(generate_hash(str), str.size(), is_none(str))
        {
        }

        constexpr explicit NameHash(const std::uint64_t hash, const std::size_t length, const bool is_none)
        {
            const auto hi = static_cast<std::uint32_t>(hash >> 32);
            const auto lo = static_cast<std::uint32_t>(hash);

            const auto is_none_bit = (is_none ? 1u : 0u) << NameSlot::probe_hash_shift;
            static_assert((shard_mask & NameSlot::probe_hash_mask) == 0, "Masks overlap");

            shard_index = hi & shard_mask;
            unmasked_slot_index = lo & name_pool_initial_slots_per_shard - 1;
            slot_probe_hash = hi & NameSlot::probe_hash_mask | is_none_bit;
            entry_probe_header.length = static_cast<std::uint16_t>(length);
        }

        static std::uint64_t generate_hash(const std::string_view str)
        {
            return XXH3_64bits(str.data(), str.size());
        }

        [[nodiscard]] constexpr static std::uint32_t get_probe_start(const std::uint32_t unmasked_slot_index,
                                                                     const std::uint32_t slot_mask)
        {
            return unmasked_slot_index & slot_mask;
        }

        [[nodiscard]] constexpr static bool is_none(std::string_view str) noexcept
        {
            using namespace std::string_view_literals;
            return str == "<none>"sv;
        }

        [[nodiscard]] constexpr friend bool operator==(NameHash lhs, NameHash rhs) noexcept = default;
    };

    struct NameValue
    {
        std::string_view name;
        NameHash hash;

        explicit NameValue(const std::string_view name) : name{name}, hash{name}
        {
        }
    };

    constexpr auto shard_align = std::hardware_destructive_interference_size;

    class alignas(shard_align) NamePoolShard
    {
      public:
        explicit NamePoolShard(NameEntryAllocator &entries) : entries_{entries}
        {
            slots_ = std::make_unique<NameSlot[]>(name_pool_initial_slots_per_shard);
        }

        NamePoolShard(const NamePoolShard &) = delete;
        NamePoolShard(NamePoolShard &&) = delete;
        ~NamePoolShard() = default;
        NamePoolShard &operator=(const NamePoolShard &) = delete;
        NamePoolShard &operator=(NamePoolShard &&) = delete;

        constexpr std::uint32_t capacity() const noexcept
        {
            return capacity_mask_ + 1;
        }

        constexpr std::uint32_t num_created() const noexcept
        {
            return num_created_entries_.load(std::memory_order_relaxed);
        }

        NameEntryId find(const NameValue &value) const
        {
            std::shared_lock lock{lock_};
            auto &slot = probe(value);
            return slot.id();
        }

        NameEntryId insert(const NameValue &value)
        {
            std::unique_lock lock{lock_};
            auto &slot = probe(value);
            if (slot.used())
            {
                return slot.id();
            }
            return create_and_insert_entry(slot, value);
        }

        void reserve(const std::uint32_t count)
        {
            const auto wanted_capacity = std::bit_ceil((count + 1) * load_factor_divisor / load_factor_quotient);

            std::unique_lock lock{lock_};
            if (wanted_capacity > capacity())
            {
                grow(wanted_capacity);
            }
        }

      private:
        static constexpr std::uint32_t load_factor_quotient = 9;
        static constexpr std::uint32_t load_factor_divisor = 10;

        static bool entry_equals_value(const NameEntry &entry, const NameValue &value) noexcept
        {
            return entry.header_ == value.hash.entry_probe_header && entry.get_name() == value.name;
        }

        void claim_slot(NameSlot &unused_slot, NameSlot new_value)
        {
            assert(!unused_slot.used());

            unused_slot = new_value;
            ++used_slots_;
            if (used_slots_ * load_factor_divisor > load_factor_quotient * capacity())
            {
                grow();
            }
        }

        NameEntryId create_and_insert_entry(NameSlot &slot, const NameValue &value)
        {
            const auto new_entry_id = entries_.create(value.name, value.hash.entry_probe_header);

            claim_slot(slot, NameSlot{new_entry_id, value.hash.slot_probe_hash});
            num_created_entries_.fetch_add(1, std::memory_order_relaxed);
            return new_entry_id;
        }

        void grow()
        {
            grow(capacity() * 2);
        }

        void grow(const std::uint32_t new_capacity) noexcept
        {
            std::span old_slots{slots_.get(), capacity()};
            const auto old_used_slots = used_slots_;

            auto new_slots = std::make_unique<NameSlot[]>(new_capacity);
            used_slots_ = 0;
            capacity_mask_ = new_capacity - 1;

            for (auto old_slot : old_slots)
            {
                if (!old_slot.used())
                    continue;

                rehash_and_insert(old_slot);
            }

            assert(old_used_slots == used_slots_);
            slots_ = std::move(new_slots);
        }

        NameSlot &probe(const NameValue &value) const
        {
            return probe(value.hash.unmasked_slot_index,
                         [&](NameSlot slot) {
                             return slot.probe_hash() == value.hash.slot_probe_hash &&
                                    entry_equals_value(entries_.resolve(slot.id()), value);
                         });
        }

        template <std::invocable<NameSlot> Predicate>
        NameSlot &probe(const std::uint32_t unmasked_slot_index, Predicate &&predicate) const
        {
            const std::uint32_t mask = capacity_mask_;
            for (std::uint32_t i = NameHash::get_probe_start(unmasked_slot_index, mask); true; i = i + 1 & mask)
            {
                if (auto &slot = slots_[i]; !slot.used() || predicate(slot))
                {
                    return slot;
                }
            }
        }

        void rehash_and_insert(const NameSlot old_slot)
        {
            assert(old_slot.used());

            const auto &entry = entries_.resolve(old_slot.id());
            const auto name = entry.get_name();
            const auto hash = NameHash{name};
            auto &new_slot = probe(hash.unmasked_slot_index, [](auto) { return false; });
            new_slot = old_slot;
            ++used_slots_;
        }

        mutable std::shared_mutex lock_;
        std::uint32_t used_slots_ = 0;
        std::uint32_t capacity_mask_ = name_pool_initial_slots_per_shard - 1;
        std::unique_ptr<NameSlot[]> slots_;
        NameEntryAllocator &entries_;
        std::atomic<std::uint32_t> num_created_entries_{0};
    };

    class NamePool
    {
        NamePool()
            : shards_(make_array<NamePoolShard, name_pool_shards>([this](auto) { return NamePoolShard{entries_}; }))
        {
#define REGISTER_NAME(name, str) known_names_[std::to_underlying(KnownName::name)] = store(str);
            PRISM_KNOWN_NAMES(REGISTER_NAME)
#undef REGISTER_NAME

            for (std::uint32_t i = 0; i < std::to_underlying(KnownName::count); ++i)
            {
                if (i != std::to_underlying(KnownName::none) && known_names_[i].is_none())
                    continue;

                entry_to_known_name_.emplace(known_names_[i], static_cast<KnownName>(i));
                largest_known_name_unstable_id_ =
                    std::max(largest_known_name_unstable_id_, known_names_[i].to_unstable_int());
            }
        }

      public:
        static NamePool &get()
        {
            static NamePool pool;
            return pool;
        }

        void reserve(const std::uint32_t num_bytes, const std::uint32_t entries)
        {
            const auto num_blocks = num_bytes / NameEntryAllocator::block_size_bytes + 1;
            entries_.reserve_blocks(num_blocks);

            if (num_entries() < entries)
            {
                const auto num_entries_per_shard = num_entries() / name_pool_shards + 1;
                for (auto &shard : shards_)
                {
                    shard.reserve(num_entries_per_shard);
                }
            }
        }
        NameEntryId store(const std::string_view name)
        {
            const NameValue value{name};
            return shards_[value.hash.shard_index].insert(value);
        }

        NameEntryId find(const std::string_view name) const
        {
            const NameValue value{name};
            return shards_[value.hash.shard_index].find(value);
        }

        NameEntryId find(const KnownName name) const
        {
            assert(std::to_underlying(name) < std::to_underlying(KnownName::count));
            return known_names_[std::to_underlying(name)];
        }

        std::optional<KnownName> find_known(const NameEntryId id) const
        {
            return id.to_unstable_int() <= std::to_underlying(KnownName::count)
                       ? std::make_optional(entry_to_known_name_.find(id)->second)
                       : std::nullopt;
        }

        NameEntry &resolve(const NameEntryHandle handle) const
        {
            return entries_.resolve(handle);
        }

        [[nodiscard]] constexpr std::uint32_t num_entries() const noexcept
        {
            return std::ranges::fold_left(shards_,
                                          0u,
                                          [](const std::uint32_t acc, const NamePoolShard &shard)
                                          { return acc + shard.num_created(); });
        }

        [[nodiscard]] constexpr std::uint32_t num_blocks() const noexcept
        {
            return entries_.num_blocks();
        }

        [[nodiscard]] constexpr std::span<const NameEntryBlockPtr> debug_memory() const noexcept
        {
            return entries_.debug_memory();
        }

        [[nodiscard]] constexpr std::uint32_t num_slots() const noexcept
        {
            return std::ranges::fold_left(shards_,
                                          0u,
                                          [](const std::uint32_t acc, const NamePoolShard &shard)
                                          { return acc + shard.capacity(); });
        }

        [[nodiscard]] constexpr std::uint32_t wasted_bytes() const noexcept
        {
            return entries_.wasted_bytes();
        }

      private:
        NameEntryAllocator entries_;
        std::array<NamePoolShard, name_pool_shards> shards_;
        alignas(shard_align) std::array<NameEntryId, std::to_underlying(KnownName::count)> known_names_;
        std::uint32_t largest_known_name_unstable_id_ = 0;
        std::unordered_map<NameEntryId, KnownName> entry_to_known_name_;
    };

    std::strong_ordering NameEntryId::compare_lexical(NameEntryId other) const
    {
        if (value_ == other.value_)
        {
            return std::strong_ordering::equal;
        }

        auto &pool = NamePool::get();
        const auto view_a = pool.resolve(*this).get_name();
        const auto view_b = pool.resolve(other).get_name();
        const auto min_length = std::min(view_a.size(), view_b.size());
        return std::strncmp(view_a.data(), view_b.data(), min_length) <=> 0;
    }

    std::strong_ordering NameEntryId::compare_semantic(NameEntryId other) const
    {
        return value_ <=> other.value_;
    }

    NameEntryId NameEntryId::from_valid_name(const KnownName name)
    {
        return NamePool::get().find(name);
    }

    std::string_view NameEntry::get_name() const noexcept
    {
        return std::string_view{name_, header_.length};
    }

    std::size_t NameEntry::size_in_bytes() const noexcept
    {
        return align(NameStore::get_size(*this), alignof(NameEntry));
    }

    Name::Name(const std::string_view name, const FindName find_type) : id_{lookup_name(name, find_type)}
    {
    }

    void Name::reserve(const std::size_t num_bytes, const std::size_t num_names)
    {
        NamePool::get().reserve(num_bytes, num_names);
    }

    std::size_t Name::name_entry_memory_size()
    {
        return NamePool::get().num_blocks() * NameEntryAllocator::block_size_bytes;
    }

    std::size_t Name::name_entry_memory_estimated_available()
    {
        return (name_max_blocks - NamePool::get().num_blocks()) * NameEntryAllocator::block_size_bytes;
    }

    std::size_t Name::name_table_memory_size()
    {
        return name_entry_memory_size() + sizeof(NamePool) + NamePool::get().num_slots() * sizeof(NameSlot);
    }

    std::size_t Name::name_table_wasted_bytes()
    {
        return NamePool::get().wasted_bytes();
    }

    std::uint32_t Name::name_count()
    {
        return NamePool::get().num_entries();
    }

    const NameEntry &Name::get_entry() const
    {
        return get_entry(id_);
    }

    const NameEntry &Name::get_entry(const KnownName name)
    {
        auto &pool = NamePool::get();
        return pool.resolve(pool.find(name));
    }

    const NameEntry &Name::get_entry(const NameEntryId id)
    {
        return resolve_entry(id);
    }

    std::string_view Name::as_string_view() const
    {
        return NamePool::get().resolve(id_).get_name();
    }

    std::optional<KnownName> Name::to_known_name() const
    {
        return NamePool::get().find_known(id_);
    }

    std::strong_ordering Name::compare_semantic(Name other) const
    {
        return id_.compare_semantic(other.id_);
    }

    std::strong_ordering Name::compare_lexical(Name other) const
    {
        return id_.compare_lexical(other.id_);
    }

    std::strong_ordering Name::compare_lexical(const std::string_view other) const
    {
        return as_string_view() <=> other;
    }

    bool operator==(const Name lhs, const std::string_view rhs) noexcept
    {
        return lhs.as_string_view() == rhs;
    }

    bool operator==(const std::string_view lhs, const Name rhs) noexcept
    {
        return lhs == rhs.as_string_view();
    }

    bool operator==(const Name lhs, const KnownName rhs) noexcept
    {
        return lhs == Name{rhs};
    }

    bool operator==(const KnownName lhs, const Name rhs) noexcept
    {
        return Name{lhs} == rhs;
    }

    const NameEntry &Name::resolve_entry(NameEntryId lookup_id)
    {
        return NamePool::get().resolve(lookup_id);
    }

    NameEntryId Name::lookup_name(std::string_view str, FindName find_type)
    {
        if (str.size() > max_name_size)
        {
            throw std::invalid_argument{"Name too long"};
        }

        auto &pool = NamePool::get();
        if (find_type == FindName::add)
        {
            return pool.store(str);
        }

        assert(find_type == FindName::find);
        return pool.find(str);
    }

    const std::byte *const *debug_get_name_memory()
    {
        static_assert(sizeof(std::byte *) == sizeof(NameEntryBlockPtr));
        return reinterpret_cast<const std::byte *const *>(NamePool::get().debug_memory().data());
    }
} // namespace prism

template <>
struct std::hash<prism::NameEntryHandle>
{
    constexpr std::size_t operator()(const prism::NameEntryHandle handle) const noexcept
    {
        using namespace prism;
        return (handle.block << (32 - name_max_block_bits)) + handle.block +
               (handle.offset << (name_block_offset_bits - 1)) + handle.offset + (handle.offset >> 4);
    }
};

std::size_t std::hash<prism::NameEntryId>::operator()(const prism::NameEntryId &id) const noexcept
{
    return std::hash<prism::NameEntryHandle>{}(id);
}
