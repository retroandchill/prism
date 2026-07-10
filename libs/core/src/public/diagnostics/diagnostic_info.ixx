/**
 * @file diagnostic_info.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core:diagnostics.diagnostic_info;

import std;
import :diagnostics.diagnostic_descriptor;
import :memory.ref_counted_ptr;

namespace prism
{
    class DiagnosticInfo final : IntrusiveRefCounted
    {
      public:
        template <typename... Args>
        explicit constexpr DiagnosticInfo(const DiagnosticDescriptor &descriptor, Args &&...args)
            : descriptor_{&descriptor},
              format_message_{[args = std::forward<Args>(args)...](const std::string_view message)
                              {
                                  return std::vformat(message, std::make_format_args(args...));
                              }}
        {
        }

        [[nodiscard]] constexpr const DiagnosticDescriptor &descriptor() const noexcept
        {
            return *descriptor_;
        }

        [[nodiscard]] std::string_view message() const
        {
            std::call_once(message_once_, [this] { message_ = format_message_(descriptor_->message_format()); });

            return *message_;
        }

      private:
        const DiagnosticDescriptor *descriptor_;
        std::function<std::string(std::string_view)> format_message_;

        mutable std::once_flag message_once_;
        mutable std::optional<std::string> message_;
    };
} // namespace prism
