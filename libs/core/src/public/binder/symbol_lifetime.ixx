/**
 * @file symbol_lifetime.ixx
 * @author Francesco Corso
 * @date 8/1/2026
 * @brief
 */
export module prism.core:binder.symbol_lifetime;

import :symbols.symbol;
import :memory.persistent_allocator;

namespace prism
{
    class SymbolLifetime final
    {
      public:
        template <std::derived_from<Symbol> Symbol, typename... Args>
            requires std::constructible_from<Symbol, Args...>
        Symbol &create(Args &&...args)
        {
            return allocator_.create<Symbol>(std::forward<Args>(args)...);
        }

      private:
        PersistentAllocator allocator_;
    };
} // namespace prism
