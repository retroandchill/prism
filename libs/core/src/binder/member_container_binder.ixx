/**
 * @file member_container_binder.ixx
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */

export module prism.core:binder.member_container_binder;

import :binder;
import :symbols.member_container_symbol;

namespace prism
{
    class MemberContainerBinder final : public Binder
    {
      public:
        MemberContainerBinder(const Binder &next, const MemberContainerSymbol &symbol);

        [[nodiscard]] Optional<const Symbol &> containing_symbol() const override;

      protected:
        [[nodiscard]] LookupResult lookup_local(Name name,
                                                LookupOptions options,
                                                const LookupContext &context) const override;

      private:
        const MemberContainerSymbol &symbol_;
    };
} // namespace prism
