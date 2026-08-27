/**
 * @file terminal_binder.ixx
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */

export module prism.core:binder.terminal_binder;

import :binder;

namespace prism
{
    class TerminalBinder final : public Binder
    {
      public:
        explicit TerminalBinder(const Compilation &compilation);

        [[nodiscard]] Optional<const Binder &> get_binder(const SyntaxNode &node) const override;

        [[nodiscard]] Optional<const Symbol &> containing_symbol() const override;

        [[nodiscard]] Optional<const SyntaxNode &> scope_designator() const override;

        [[nodiscard]] VariablesSpan get_declared_local_variables_for_scope(const SyntaxNode &designator) const override;

      protected:
        void ensure_locals() const override;

        [[nodiscard]] LookupResult lookup_local(Name name,
                                                LookupOptions options,
                                                const LookupContext &context) const override;
    };
} // namespace prism
