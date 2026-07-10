/**
 * @file name.ixx
 * @author Francesco Corso
 * @date 6/28/2026
 * @brief
 */
module;

#include "prism/core/exports.h"
#include "prism/core/names.hpp"

#include <cstddef>

export module prism.core:strings.name;

import std;
import :util.optional;

namespace prism
{
    export enum class KnownName : std::uint32_t
    {
#define REGISTER_NAME(name, str) name,
        PRISM_KNOWN_NAMES(REGISTER_NAME)
#undef REGISTER_NAME
        count
    };

    [[nodiscard]] constexpr std::string_view to_string(const KnownName name) noexcept
    {
        switch (name)
        {
#define NAME_TO_STRING(name, str)                                                                                      \
    case KnownName::name:                                                                                              \
        return str;
            PRISM_KNOWN_NAMES(NAME_TO_STRING)
#undef NAME_TO_STRING
            case KnownName::count:
                // We shouldn't pass this, but it is a valid enumerator
                break;
        }

        return "";
    }

    export constexpr inline std::size_t max_name_size = 1024;
    constexpr std::size_t name_entry_alignment = sizeof(std::max_align_t);

    export class PRISM_CORE_API NameEntryId
    {
      public:
        constexpr NameEntryId() = default;

      private:
        constexpr explicit NameEntryId(const std::uint32_t value) : value_{value}
        {
        }

      public:
        [[nodiscard]] constexpr bool is_none() const noexcept
        {
            return value_ == 0;
        }

        [[nodiscard]] std::strong_ordering compare_lexical(NameEntryId other) const;

        [[nodiscard]] std::strong_ordering compare_semantic(NameEntryId other) const;

        [[nodiscard]] constexpr std::uint32_t to_unstable_int() const noexcept
        {
            return value_;
        }

        [[nodiscard]] constexpr static NameEntryId from_unstable_int(const std::uint32_t value)
        {
            return NameEntryId{value};
        }

        [[nodiscard]] constexpr static NameEntryId from_known_name(const KnownName name)
        {
            return name == KnownName::none ? NameEntryId{} : from_valid_name(name);
        }

        [[nodiscard]] constexpr friend bool operator==(const NameEntryId &lhs,
                                                       const NameEntryId &rhs) noexcept = default;
        [[nodiscard]] constexpr friend std::strong_ordering operator<=>(const NameEntryId &lhs,
                                                                        const NameEntryId &rhs) noexcept = default;

      private:
        static NameEntryId from_valid_name(KnownName name);

        friend std::hash<NameEntryId>;

        std::uint32_t value_ = 0;
    };

    export enum class FindName : std::uint8_t
    {
        find,
        add
    };

    struct NameEntryHeader
    {
        std::uint16_t length = 0;

        [[nodiscard]] constexpr bool operator==(const NameEntryHeader &rhs) const noexcept = default;
    };

    export class alignas(name_entry_alignment) PRISM_CORE_API NameEntry
    {
      public:
        NameEntry() = default;
        NameEntry(const NameEntry &other) = delete;
        NameEntry(NameEntry &&other) noexcept = default;
        ~NameEntry() = default;
        NameEntry &operator=(const NameEntry &other) = delete;
        NameEntry &operator=(NameEntry &&other) noexcept = default;

        [[nodiscard]] constexpr std::uint32_t name_length() const noexcept
        {
            return header_.length;
        }

        [[nodiscard]] std::string_view get_name(bool escaped = false) const noexcept;

        [[nodiscard]] std::size_t size_in_bytes() const noexcept;

        [[nodiscard]] constexpr static std::uint32_t data_offset() noexcept
        {
            return offsetof(NameEntry, data_);
        }

      private:
        friend class NameEntryAllocator;
        friend class NamePoolShard;

        NameEntryHeader header_;
        union
        {
            char name_[max_name_size]{};
            std::byte data_[0];
        };
    };

    export class PRISM_CORE_API Name
    {
      public:
        constexpr Name() = default;

        explicit inline Name(const KnownName name) : id_{NameEntryId::from_known_name(name)}
        {
        }

        explicit(false) inline Name(const char *name, const FindName find_type = FindName::add)
            : Name{std::string_view{name}, find_type}
        {
        }

        explicit(false) inline Name(const std::string &name, const FindName find_type = FindName::add)
            : Name{std::string_view{name}, find_type}
        {
        }

        explicit(false) Name(std::string_view name, FindName find_type = FindName::add);

        [[nodiscard]] constexpr bool is_none() const noexcept
        {
            return id_.is_none();
        }

        [[nodiscard]] constexpr NameEntryId id() const noexcept
        {
            return id_;
        }

        static void reserve(std::size_t num_bytes, std::size_t num_names);

        [[nodiscard]] static std::size_t name_entry_memory_size();

        [[nodiscard]] static std::size_t name_entry_memory_estimated_available();

        [[nodiscard]] static std::size_t name_table_memory_size();

        [[nodiscard]] static std::size_t name_table_wasted_bytes();

        [[nodiscard]] static std::uint32_t name_count();

        [[nodiscard]] const NameEntry &get_entry() const;

        [[nodiscard]] static const NameEntry &get_entry(KnownName name);

        [[nodiscard]] static const NameEntry &get_entry(NameEntryId id);

        [[nodiscard]] std::string_view as_string_view(bool escaped = false) const;

        [[nodiscard]] Optional<KnownName> to_known_name() const;

        [[nodiscard]] std::strong_ordering compare_semantic(Name other) const;

        [[nodiscard]] std::strong_ordering compare_lexical(Name other) const;

        [[nodiscard]] std::strong_ordering compare_lexical(std::string_view other) const;

        [[nodiscard]] constexpr friend bool operator==(Name lhs, Name rhs) noexcept = default;

        [[nodiscard]] friend inline bool operator==(const Name lhs, const std::string_view rhs) noexcept
        {
            return lhs.as_string_view() == rhs;
        }

        [[nodiscard]] friend inline bool operator==(const std::string_view lhs, const Name rhs) noexcept
        {
            return lhs == rhs.as_string_view();
        }

        [[nodiscard]] friend inline bool operator==(const Name lhs, const KnownName rhs) noexcept
        {
            return lhs == Name{rhs};
        }

        [[nodiscard]] friend inline bool operator==(const KnownName lhs, const Name rhs) noexcept
        {
            return Name{lhs} == rhs;
        }

        [[nodiscard]] constexpr friend std::strong_ordering operator<=>(Name lhs, Name rhs) noexcept = default;

        friend inline std::ostream &operator<<(std::ostream &os, const Name name)
        {
            return os << name.as_string_view();
        }

      private:
        static const NameEntry &resolve_entry(NameEntryId lookup_id);

        static NameEntryId lookup_name(std::string_view str, FindName find_type);

        NameEntryId id_;
    };

    struct DebugNameMemoryGetter
    {
        PRISM_CORE_API static const std::byte *const *get();
    };

    export inline const std::byte *const *debug_get_name_memory()
    {
        return DebugNameMemoryGetter::get();
    }

} // namespace prism

template <>
struct std::hash<prism::NameEntryId>
{
    PRISM_CORE_API std::size_t operator()(const prism::NameEntryId &id) const noexcept;
};

template <>
struct std::hash<prism::Name>
{
    inline std::size_t operator()(const prism::Name &name) const noexcept
    {
        return std::hash<prism::NameEntryId>{}(name.id());
    }
};
