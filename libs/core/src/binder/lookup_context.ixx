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
        explicit constexpr LookupContext(const AssemblySymbol &assembly_symbol) noexcept
            : assembly_symbol_{assembly_symbol}
        {
        }

        explicit constexpr LookupContext(const AssemblySymbol &assembly_symbol, DiagnosticBag &diagnostics) noexcept
            : assembly_symbol_{assembly_symbol}, diagnostics_{&diagnostics}
        {
        }

        [[nodiscard]] constexpr const AssemblySymbol &assembly_symbol() const noexcept
        {
            return assembly_symbol_;
        }

        void report_diagnostic(Diagnostic diagnostic) const;

      private:
        const AssemblySymbol &assembly_symbol_;
        DiagnosticBag *diagnostics_ = nullptr;
    };
} // namespace prism
