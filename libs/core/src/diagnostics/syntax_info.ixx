/**
 * @file syntax_info.ixx
 * @author Francesco Corso
 * @date 7/25/2026
 * @brief
 */
module;

export module prism.core:diagnostics.syntax_info;

import :diagnostics.info;

namespace prism
{
    struct SyntaxDiagnosticInfo final
    {
        RefCountPtr<const DiagnosticInfo> info{};
        std::uint32_t offset = 0;
        std::uint32_t width = 0;

        explicit constexpr SyntaxDiagnosticInfo(RefCountPtr<const DiagnosticInfo> info,
                                                const std::uint32_t offset = 0,
                                                const std::uint32_t width = 0) noexcept
            : info{std::move(info)

              },
              offset{offset}, width{width}
        {
        }
    };
} // namespace prism
