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

        template <std::ranges::input_range Range>
            requires std::ranges::sized_range<Range> &&
                     std::is_lvalue_reference_v<std::ranges::range_reference_t<Range>>
        auto copy_refs(Range &&range)
        {
            std::scoped_lock lock{mutex_};
            return allocator_.copy(std::forward<Range>(range) |
                                   std::views::transform([](const auto &item) { return Ref{item}; }));
        }

      private:
        std::mutex mutex_;
        PersistentAllocator allocator_;
    };
} // namespace prism
