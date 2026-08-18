/**
 * @file lookup_context.ixx
 * @author Francesco Corso
 * @date 8/18/2026
 * @brief
 */

export module prism.core:binder.lookup_context;

import std;
import :diagnostics.diagnostic;

namespace prism
{
    class DiagnosticBag;
    class AssemblySymbol;

    class LookupContext final
    {
      public:
        constexpr LookupContext() noexcept = default;

        explicit constexpr LookupContext(DiagnosticBag &diagnostics) noexcept : diagnostics_{&diagnostics}
        {
        }

        void report_diagnostic(Diagnostic diagnostic) const;

      private:
        DiagnosticBag *diagnostics_ = nullptr;
    };
} // namespace prism
