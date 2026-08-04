/**
 * @file semantic_lifetime.ixx
 * @author Francesco Corso
 * @date 8/1/2026
 * @brief
 */
export module prism.core:semantic.semantic_lifetime;

import :symbols.symbol;
import :memory.persistent_allocator;

namespace prism
{
    class SemanticLifetime final
    {
      public:
        template <typename T, typename... Args>
            requires std::constructible_from<T, Args...>
        T &create(Args &&...args)
        {
            std::scoped_lock lock{mutex_};
            return allocator_.create<T>(std::forward<Args>(args)...);
        }

      private:
        std::mutex mutex_;
        PersistentAllocator allocator_;
    };
} // namespace prism
