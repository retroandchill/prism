/**
 * @file symbol.ixx
 * @author Francesco Corso
 * @date 7/30/2026
 * @brief
 */
export module prism.core:symbols.symbol;

import :text.name;
import :util.noncopyable;

namespace prism
{
    export class Symbol : NonCopyable
    {
      protected:
        Symbol() = default;
        ~Symbol() = default;

      public:
        virtual Name name() const noexcept = 0;
    };
} // namespace prism
