/**
 * @file persistent_allocator.ixx
 * @author Francesco Corso
 * @date 7/1/2026
 * @brief
 */
module;

// CLion is being weird about std::start_lifetime_as in import std,
// so this is just to shut it up
#ifdef __JETBRAINS_IDE__
#include <memory>
#endif

export module prism.core:memory.persistent_allocator;

import std;
import :memory.arena_allocator;

namespace prism
{
    using DeleterFunc = void (*)(void *);

    struct DeleterInvocation
    {
        void *ptr = nullptr;
        DeleterFunc deleter = nullptr;
    };

    export class PersistentAllocator
    {
        static constexpr std::size_t block_size = 1024 * 1024;

      public:
        PersistentAllocator() = default;
        inline ~PersistentAllocator()
        {
            destruct_objects();
        }

        PersistentAllocator(const PersistentAllocator &) = delete;
        PersistentAllocator(PersistentAllocator &&) = delete;
        PersistentAllocator &operator=(const PersistentAllocator &) = delete;
        PersistentAllocator &operator=(PersistentAllocator &&) = delete;

        template <typename T, typename... Args>
            requires std::constructible_from<T, Args...>
        T &create(Args &&...args)
        {
            auto *data = arena_.allocate(sizeof(T), alignof(T));
            auto *ptr = std::construct_at(std::start_lifetime_as<T>(data), std::forward<Args>(args)...);
            if constexpr (!std::is_trivially_destructible_v<T>)
            {
                deleters_.emplace_back(
                    ptr,
                    +[](void *p) noexcept { std::destroy_at(static_cast<T *>(p)); });
            }
            return *ptr;
        }

        template <std::ranges::input_range Range>
            requires std::ranges::sized_range<Range> &&
                     std::convertible_to<std::ranges::range_reference_t<Range>, std::ranges::range_value_t<Range>>
        std::span<std::ranges::range_value_t<Range>> copy(Range &&range)
        {
            using T = std::ranges::range_value_t<Range>;
            auto *ptr = arena_.allocate(std::ranges::size(range) * sizeof(T), alignof(T));
            std::span target{static_cast<T *>(ptr), std::ranges::size(range)};
            if constexpr (!std::is_trivially_copyable_v<T>)
            {
                for (auto [i, items] : target | std::views::zip(std::forward<Range>(range)) | std::views::enumerate)
                {
                    auto [item, src] = items;
                    auto *item_ptr = std::addressof(item);
                    std::construct_at(item_ptr, src);
                    if constexpr (!std::is_trivially_destructible_v<T>)
                    {
                        deleters_.emplace_back(
                            item_ptr,
                            +[](void *p) noexcept { std::destroy_at(static_cast<T *>(p)); });
                    }
                }
            }
            else
            {
                std::ranges::copy(std::forward<Range>(range), target.begin());
                if constexpr (!std::is_trivially_destructible_v<T>)
                {
                    for (auto [i, item_ptr] : target | std::views::enumerate)
                    {
                        deleters_.emplace_back(
                            item_ptr,
                            +[](void *p) noexcept { std::destroy_at(static_cast<T *>(p)); });
                    }
                }
            }

            return target;
        }

        inline void reset() noexcept
        {
            destruct_objects();
            deleters_.clear();
            arena_.reset();
        }

      private:
        inline void destruct_objects() noexcept
        {
            for (auto [ptr, deleter] : deleters_ | std::views::reverse)
            {
                deleter(ptr);
            }
        }

        MultiArena arena_{block_size};
        std::vector<DeleterInvocation> deleters_;
    };
} // namespace prism
