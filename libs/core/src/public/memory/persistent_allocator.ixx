/**
 * @file persistent_allocator.ixx
 * @author Francesco Corso
 * @date 7/1/2026
 * @brief
 */
export module prism.core.memory.persistent_allocator;

import std;
import prism.core.memory.arena_allocator;

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
            for (auto [ptr, deleter] : deleters_ | std::views::reverse)
            {
                deleter(ptr);
            }
        }

        PersistentAllocator(const PersistentAllocator &) = delete;
        PersistentAllocator(PersistentAllocator &&) = delete;
        PersistentAllocator &operator=(const PersistentAllocator &) = delete;
        PersistentAllocator &operator=(PersistentAllocator &&) = delete;

        static PersistentAllocator &shared() noexcept;

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

      private:
        MultiArena arena_{block_size};
        std::vector<DeleterInvocation> deleters_;
    };
} // namespace prism
