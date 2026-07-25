/**
 * @file info.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:diagnostics.info;

import std;
import :diagnostics.descriptor;
import :diagnostics.registry;
import :diagnostics.traits;
import :memory.ref_counted_ptr;
import :util.optional;
import boost;
import :text.name;
import :util.noncopyable;
import libassert;

namespace prism
{
    template <DiagnosticCode Code, typename... Args>
    concept CanCreateDiagnostic = std::constructible_from<typename DiagnosticTraits<Code>::Args, Args...> &&
                                  sizeof...(Args) == std::tuple_size_v<typename DiagnosticTraits<Code>::Args>;

    class DiagnosticArguments : NonCopyable
    {
      public:
        virtual ~DiagnosticArguments() = default;

        virtual std::string format(std::string_view format_string) const = 0;

        template <std::formattable<char>... Args>
        constexpr static std::shared_ptr<DiagnosticArguments> create(Args &&...args);
    };

    template <std::formattable<char>... Args>
    class DiagnosticArgumentsImpl final : public DiagnosticArguments
    {
      public:
        constexpr explicit DiagnosticArgumentsImpl(Args... args) : args_{std::move(args)...}
        {
        }

        std::string format(const std::string_view format_string) const override
        {
            return std::vformat(format_string,
                                std::apply([&](const auto &...args) { return std::make_format_args(args...); }, args_));
        }

      private:
        std::tuple<Args...> args_;
    };

    class DiagnosticInfo : public IntrusiveRefCounted
    {
      protected:
        struct CloneTag
        {
        };

        static constexpr CloneTag clone_tag{};

      public:
        template <std::formattable<char>... Args>
        constexpr explicit DiagnosticInfo(const DiagnosticCode code, Args &&...args)
            : code_{code}, default_severity_{get_severity(code)}, effective_severity_{default_severity_},
              arguments_{DiagnosticArguments::create(std::forward<Args>(args)...)}
        {
        }

        constexpr explicit DiagnosticInfo(const DiagnosticCode code, std::shared_ptr<DiagnosticArguments> arguments)
            : code_{code}, default_severity_{get_severity(code)}, effective_severity_{default_severity_},
              arguments_{std::move(arguments)}
        {
        }

        constexpr explicit DiagnosticInfo(CloneTag, const DiagnosticInfo &original, const DiagnosticSeverity severity)
            : code_{original.code_}, default_severity_{original.default_severity_}, effective_severity_{severity},
              arguments_{original.arguments_}
        {
        }

        virtual ~DiagnosticInfo() = default;

        template <DiagnosticCode Code, typename... Args>
            requires CanCreateDiagnostic<Code, Args...>
        constexpr static RefCountPtr<DiagnosticInfo> create(Args &&...args)
        {
            return make_ref_counted<DiagnosticInfo>(Code, std::forward<Args>(args)...);
        }

        [[nodiscard]] constexpr DiagnosticCode code() const noexcept
        {
            return code_;
        }

        [[nodiscard]] constexpr virtual const DiagnosticDescriptor &descriptor() const
        {
            return *diagnostics::get_descriptor(code_);
        }

        [[nodiscard]] constexpr DiagnosticSeverity severity() const noexcept
        {
            return effective_severity_;
        }

        [[nodiscard]] constexpr DiagnosticSeverity default_severity() const noexcept
        {
            return default_severity_;
        }

        [[nodiscard]] constexpr const std::shared_ptr<DiagnosticArguments> &arguments() const noexcept
        {
            return arguments_;
        }

        [[nodiscard]] std::string get_message() const;

        template <typename Self>
        [[nodiscard]] RefCountPtr<const Self> with_severity(this const Self &self, DiagnosticSeverity severity)
        {
            if (self.severity() == severity)
                return self.shared_from_this();

            auto result = self.clone_with_severity(severity);
            DEBUG_ASSERT(typeid(*this) == typeid(*result));
            DEBUG_ASSERT(severity == result->severity());
            return static_pointer_cast<const Self>(std::move(result));
        }

      protected:
        virtual RefCountPtr<const DiagnosticInfo> clone_with_severity(DiagnosticSeverity severity) const;

      private:
        constexpr static DiagnosticSeverity get_severity(const DiagnosticCode code)
        {
            return diagnostics::get_descriptor(code)
                .transform([](const DiagnosticDescriptor &descriptor) { return descriptor.default_severity; })
                .value_or(DiagnosticSeverity::error);
        }

        constexpr static std::string_view get_message(const DiagnosticCode code)
        {
            return diagnostics::get_descriptor(code)
                .transform([](const DiagnosticDescriptor &descriptor) { return descriptor.format_message; })
                .value_or("<unknown>");
        }

        DiagnosticCode code_;
        DiagnosticSeverity default_severity_;
        DiagnosticSeverity effective_severity_;
        std::shared_ptr<DiagnosticArguments> arguments_;
    };

    using DiagnosticInfoList = std::vector<RefCountPtr<const DiagnosticInfo>>;

    template <std::formattable<char>... Args>
    constexpr std::shared_ptr<DiagnosticArguments> DiagnosticArguments::create(Args &&...args)
    {
        if constexpr (sizeof...(Args) == 0)
        {
            return nullptr;
        }
        else
        {
            return std::make_unique<DiagnosticArgumentsImpl<std::decay_t<Args>...>>(std::forward<Args>(args)...);
        }
    }
} // namespace prism
