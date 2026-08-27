/**
 * @file for_loop_binder.ixx
 * @author Francesco Corso
 * @date 8/27/2026
 * @brief
 */

export module prism.core:binder.for_loop_binder;

import :binder.loop_binder;
import :syntax.statements;

namespace prism
{
    class ForLoopBinder final : public LoopBinder
    {
      public:
        explicit constexpr ForLoopBinder(const Binder &enclosing, const ForStatementSyntax &syntax)
            : LoopBinder{enclosing}, syntax_{syntax}
        {
        }

      protected:
        [[nodiscard]] std::span<Ref<const VariableSymbol>> build_local_variables_impl() const override;

      private:
        const ForStatementSyntax &syntax_;
    };
} // namespace prism
