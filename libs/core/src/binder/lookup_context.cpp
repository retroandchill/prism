/**
 * @file lookup_context.cpp
 * @author Francesco Corso
 * @date 8/18/2026
 * @brief
 */
module prism.core:binder.lookup_context.impl;

import :binder.lookup_context;
import :diagnostics.diagnostic_bag;

namespace prism
{

    void LookupContext::report_diagnostic(Diagnostic diagnostic) const
    {
        if (diagnostics_ == nullptr)
            return;

        diagnostics_->add(std::move(diagnostic));
    }
} // namespace prism
