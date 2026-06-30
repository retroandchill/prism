/**
 * @file name.ixx
 * @author Francesco Corso
 * @date 6/28/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

#include <cstddef>

export module prism.core.strings.name;

import std;

namespace prism
{
    export enum class KnownName : std::uint32_t
    {
#define REGISTER_NAME(name, str) name,
#include "names.inl"
#undef REGISTER_NAME
        count
    };

    [[nodiscard]] constexpr std::string_view to_string(const KnownName name) noexcept
    {
        switch (name)
        {
#define REGISTER_NAME(name, str)                                                                                       \
    case KnownName::name:                                                                                              \
        return str;
#include "names.inl"
#undef
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

        [[nodiscard]] constexpr friend bool operator==(NameEntryHeader lhs, NameEntryHeader rhs) noexcept = default;
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

        [[nodiscard]] std::string_view get_name() const noexcept;

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

        [[nodiscard]] std::string_view as_string_view() const;

        [[nodiscard]] std::optional<KnownName> to_known_name() const;

        [[nodiscard]] std::strong_ordering compare_semantic(Name other) const;

        [[nodiscard]] std::strong_ordering compare_lexical(Name other) const;

        [[nodiscard]] std::strong_ordering compare_lexical(std::string_view other) const;

        [[nodiscard]] constexpr friend bool operator==(Name lhs, Name rhs) noexcept = default;

        [[nodiscard]] friend bool operator==(Name lhs, std::string_view rhs) noexcept;

        [[nodiscard]] friend bool operator==(std::string_view lhs, Name rhs) noexcept;

        [[nodiscard]] friend bool operator==(Name lhs, KnownName rhs) noexcept;

        [[nodiscard]] friend bool operator==(KnownName lhs, Name rhs) noexcept;

        [[nodiscard]] constexpr friend std::strong_ordering operator<=>(Name lhs, Name rhs) noexcept = default;

      private:
        static const NameEntry &resolve_entry(NameEntryId lookup_id);

        static NameEntryId lookup_name(std::string_view str, FindName find_type);

        NameEntryId id_;
    };

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
