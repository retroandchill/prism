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
    class SyntaxDiagnosticInfo final : public DiagnosticInfo
    {
      public:
        template <std::formattable<char>... Args>
        explicit constexpr SyntaxDiagnosticInfo(const DiagnosticCode code, Args &&...args)
            : DiagnosticInfo{code, std::forward<Args>(args)...}
        {
        }

        template <std::formattable<char>... Args>
        constexpr SyntaxDiagnosticInfo(const std::uint32_t offset,
                                       const std::uint32_t width,
                                       const DiagnosticCode code,
                                       Args &&...args)
            : DiagnosticInfo{code, std::forward<Args>(args)...}, offset_{offset}, width_{width}
        {
        }

        constexpr SyntaxDiagnosticInfo(const std::uint32_t offset,
                                       const std::uint32_t width,
                                       const DiagnosticCode code,
                                       std::shared_ptr<DiagnosticArguments> arguments)
            : DiagnosticInfo{code, std::move(arguments)}, offset_{offset}, width_{width}
        {
        }

        constexpr SyntaxDiagnosticInfo(const DiagnosticCode code, std::shared_ptr<DiagnosticArguments> arguments)
            : DiagnosticInfo{code, std::move(arguments)}
        {
        }

        SyntaxDiagnosticInfo(CloneTag, const SyntaxDiagnosticInfo &original, DiagnosticSeverity severity)
            : DiagnosticInfo{clone_tag, original, severity}, offset_{original.offset_}, width_{original.width_}
        {
        }

        template <DiagnosticCode Code, typename... Args>
            requires CanCreateDiagnostic<Code, Args...>
        constexpr static RefCountPtr<SyntaxDiagnosticInfo> create(std::uint32_t offset,
                                                                  std::uint32_t width,
                                                                  Args &&...args)
        {
            return make_ref_counted<SyntaxDiagnosticInfo>(offset, width, Code, std::forward<Args>(args)...);
        }

        template <DiagnosticCode Code, typename... Args>
            requires CanCreateDiagnostic<Code, Args...>
        constexpr static RefCountPtr<SyntaxDiagnosticInfo> create(Args &&...args)
        {
            return make_ref_counted<SyntaxDiagnosticInfo>(Code, std::forward<Args>(args)...);
        }

        [[nodiscard]] constexpr std::uint32_t offset() const noexcept
        {
            return offset_;
        }
        [[nodiscard]] constexpr std::uint32_t width() const noexcept
        {
            return width_;
        }

        [[nodiscard]] RefCountPtr<const SyntaxDiagnosticInfo> with_offset(uint32_t offset) const;

      protected:
        [[nodiscard]] RefCountPtr<const DiagnosticInfo> clone_with_severity(DiagnosticSeverity severity) const override;

      private:
        std::uint32_t offset_ = 0;
        std::uint32_t width_ = 0;
    };
} // namespace prism
