/**
 * @file diagnostic_bag.ixx
 * @author Francesco Corso
 * @date 8/3/2026
 * @brief
 */
export module prism.core:diagnostics.diagnostic_bag;

import std;
import :diagnostics.diagnostic;

namespace prism
{
    template <typename Container>
    concept DiagnosticBaggable = requires(Container &c, const Diagnostic &diagnostic) {
        requires
            // ReSharper disable once CppRedundantParentheses
            (
                requires { c.emplace_back(diagnostic); } || requires { c.push_back(diagnostic); } ||
                requires { c.emplace(c.end(), diagnostic); } || requires { c.insert(c.end(), diagnostic); });
    };

    template <typename Container>
    concept DiagnosticAppendable = requires(Container &c, const std::vector<Diagnostic> &diagnostics) {
        {
            c.append_range(diagnostics)
        };
    };

    class DiagnosticBag
    {
      public:
        explicit DiagnosticBag(std::size_t initial_capacity = 0);

        [[nodiscard]] bool empty() const;

        [[nodiscard]] bool has_any_errors() const;

        void add(Diagnostic diagnostic);

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, Diagnostic>
        void add_range(Range &&range)
        {
            std::scoped_lock lock{mutex_};
            diagnostics_.append_range(std::forward<Range>(range));
        }

        void add_range(const DiagnosticBag &other);

        void add_range_and_clear(DiagnosticBag &other);

        void clear();

        template <DiagnosticAppendable Container>
        void copy_to(Container &container) const
        {
            std::scoped_lock lock{mutex_};
            container.append_range(diagnostics_);
        }

        template <DiagnosticBaggable Container, typename... Args>
        Container copy_to(Args &&...args) const
        {
            std::scoped_lock lock{mutex_};
            return diagnostics_ | std::ranges::to<Container>(std::forward<Args>(args)...);
        }

        template <template <typename...> typename Container, typename... Args>
            requires DiagnosticBaggable<Container<Diagnostic>>
        auto copy_to(Args &&...args) const
        {
            std::scoped_lock lock{mutex_};
            return diagnostics_ | std::ranges::to<Container>(std::forward<Args>(args)...);
        }

        [[nodiscard]] std::vector<Diagnostic> drain();

        template <DiagnosticBaggable Container, typename... Args>
        Container move_to(Args &&...args)
        {
            std::scoped_lock lock{mutex_};
            auto result =
                diagnostics_ | std::views::as_rvalue | std::ranges::to<Container>(std::forward<Args>(args)...);
            diagnostics_.clear();
            return result;
        }

        template <template <typename...> typename Container, typename... Args>
            requires DiagnosticBaggable<Container<Diagnostic>>
        auto move_to(Args &&...args)
        {
            std::scoped_lock lock{mutex_};
            auto result =
                diagnostics_ | std::views::as_rvalue | std::ranges::to<Container>(std::forward<Args>(args)...);
            diagnostics_.clear();
            return result;
        }

        template <DiagnosticAppendable Container>
        void move_to(Container &container)
        {
            std::scoped_lock lock{mutex_};
            container.append_range(diagnostics_ | std::views::as_rvalue);
            diagnostics_.clear();
        }

      private:
        std::vector<Diagnostic> diagnostics_;
        mutable std::mutex mutex_;
    };
} // namespace prism
