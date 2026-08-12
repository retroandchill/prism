/**
 * @file diagnostic_bag.cpp
 * @author Francesco Corso
 * @date 8/3/2026
 * @brief
 */
module prism.core:diagnostics.diagnostic_bag.impl;

import :diagnostics.diagnostic_bag;

namespace prism
{

    DiagnosticBag::DiagnosticBag(const std::size_t initial_capacity)
    {
        diagnostics_.reserve(initial_capacity);
    }

    bool DiagnosticBag::empty() const
    {
        std::scoped_lock lock{mutex_};
        return diagnostics_.empty();
    }

    bool DiagnosticBag::has_any_errors() const
    {
        std::scoped_lock lock{mutex_};
        return std::ranges::any_of(diagnostics_,
                                   [](const Diagnostic &diagnostic)
                                   { return diagnostic.severity() == DiagnosticSeverity::error; });
    }

    void DiagnosticBag::add(Diagnostic diagnostic)
    {
        std::scoped_lock lock{mutex_};
        diagnostics_.push_back(std::move(diagnostic));
    }

    void DiagnosticBag::add_range(const DiagnosticBag &other)
    {
        std::scoped_lock lock{other.mutex_};
        add_range(other.diagnostics_);
    }

    void DiagnosticBag::add_range_and_clear(DiagnosticBag &other)
    {
        std::scoped_lock lock_this{mutex_};
        std::scoped_lock lock_other{other.mutex_};
        diagnostics_.append_range(other.diagnostics_);
        other.diagnostics_.clear();
    }

    void DiagnosticBag::clear()
    {
        std::scoped_lock lock{mutex_};
        diagnostics_.clear();
    }

    std::vector<Diagnostic> DiagnosticBag::drain()
    {
        std::scoped_lock lock{mutex_};
        return std::exchange(diagnostics_, {});
    }
} // namespace prism
