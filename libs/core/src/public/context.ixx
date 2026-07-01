/**
 * @file context.ixx
 * @author Francesco Corso
 * @date 7/1/2026
 * @brief
 */
export module prism.core.context;

import std;
import prism.core.memory.persistent_allocator;
import prism.core.diagnostic;
import prism.core.source.source_file;

namespace prism
{
    export class CompilationContext
    {
      public:
        constexpr explicit CompilationContext(std::string text) : source_file_{std::move(text)}
        {
        }

        [[nodiscard]] constexpr const SourceFile &source_file() const noexcept
        {
            return source_file_;
        }

        [[nodiscard]] constexpr std::string_view slice(const SourceRange range) const noexcept
        {
            return source_file_.slice(range);
        }

        [[nodiscard]] constexpr const std::vector<Diagnostic> &diagnostics() const noexcept
        {
            return diagnostics_;
        }

        template <typename T, typename... Args>
            requires std::constructible_from<T, Args...>
        T &create(Args &&...args)
        {
            return allocator_.create<T>(std::forward<Args>(args)...);
        }

        template <typename... GivenArgs>
        constexpr void report(const DiagnosticDescriptor descriptor, SourceRange range, GivenArgs &&...args)
        {
            diagnostics_.emplace_back(descriptor,
                                      range,
                                      std::vformat(descriptor.message_format, std::make_format_args(args...)));
        }

      private:
        SourceFile source_file_;
        PersistentAllocator allocator_;
        std::vector<Diagnostic> diagnostics_;
    };
} // namespace prism
