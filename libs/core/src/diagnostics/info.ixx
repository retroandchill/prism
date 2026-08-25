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
import :diagnostics.location;
import libassert;
import :text.writer;
import :collections.immutable_array;

namespace prism
{
    template <DiagnosticCode Code, typename... Args>
    concept CanCreateDiagnostic = std::constructible_from<typename DiagnosticTraits<Code>::Args, Args...> &&
                                  sizeof...(Args) == std::tuple_size_v<typename DiagnosticTraits<Code>::Args> &&
                                  (std::formattable<Args, char> && ...);

    class DiagnosticArguments : NonCopyable
    {
      public:
        virtual ~DiagnosticArguments() = default;

        [[nodiscard]] virtual std::string format(std::string_view format_string) const = 0;

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

        [[nodiscard]] std::string format(const std::string_view format_string) const override
        {
            return std::vformat(format_string,
                                std::apply([&](const auto &...args) { return std::make_format_args(args...); }, args_));
        }

      private:
        std::tuple<Args...> args_;
    };

    class DiagnosticInfo : public std::enable_shared_from_this<DiagnosticInfo>
    {
      protected:
        struct CloneTag
        {
        };

        static constexpr CloneTag clone_tag{};

      public:
        template <std::formattable<char>... Args>
        constexpr explicit DiagnosticInfo(const DiagnosticDescriptor &descriptor, Args &&...args)
            : descriptor_{descriptor}, default_severity_{descriptor.default_severity()},
              effective_severity_{default_severity_},
              arguments_{DiagnosticArguments::create(std::forward<Args>(args)...)}
        {
        }

        constexpr explicit DiagnosticInfo(const DiagnosticDescriptor &descriptor,
                                          std::shared_ptr<DiagnosticArguments> arguments)
            : descriptor_{descriptor}, default_severity_{descriptor.default_severity()},
              effective_severity_{default_severity_}, arguments_{std::move(arguments)}
        {
        }

        constexpr explicit DiagnosticInfo(CloneTag, const DiagnosticInfo &original, const DiagnosticSeverity severity)
            : descriptor_{original.descriptor_}, default_severity_{original.default_severity_},
              effective_severity_{severity}, arguments_{original.arguments_}
        {
        }

        virtual ~DiagnosticInfo() = default;

        template <DiagnosticCode Code, typename... Args>
            requires CanCreateDiagnostic<Code, Args...>
        constexpr static std::shared_ptr<DiagnosticInfo> create(Args &&...args)
        {
            return std::make_shared<DiagnosticInfo>(diagnostics::get_descriptor(Code).value(),
                                                    std::forward<Args>(args)...);
        }

        [[nodiscard]] constexpr std::string_view id() const noexcept
        {
            return descriptor().id();
        }

        [[nodiscard]] constexpr std::string_view category() const noexcept
        {
            return descriptor().category();
        }

        [[nodiscard]] constexpr const DiagnosticDescriptor &descriptor() const
        {
            return descriptor_;
        }

        [[nodiscard]] constexpr DiagnosticSeverity severity() const noexcept
        {
            return effective_severity_;
        }

        [[nodiscard]] constexpr DiagnosticSeverity default_severity() const noexcept
        {
            return default_severity_;
        }

        [[nodiscard]] constexpr bool is_enabled_by_default() const
        {
            return descriptor_.enabled_by_default();
        }

        [[nodiscard]] bool is_warning_as_error() const noexcept;

        [[nodiscard]] constexpr const std::shared_ptr<DiagnosticArguments> &arguments() const noexcept
        {
            return arguments_;
        }

        [[nodiscard]] std::string get_message() const;

        template <std::derived_from<TextWriter> Writer>
        void write_message(Writer &writer) const
        {
            if (severity() == DiagnosticSeverity::error || is_warning_as_error())
                writer.write("error ");
            else
                writer.write("warning ");

            writer.write(id());
            writer.write(": ");
            writer.write(get_message());
        }

        [[nodiscard]] std::string to_string() const;

        [[nodiscard]] virtual const ImmutableArray<Location> &additional_locations() const;

        [[nodiscard]] std::span<const std::string_view> custom_tags() const;

        template <typename Self>
        [[nodiscard]] std::shared_ptr<const Self> with_severity(this const Self &self, DiagnosticSeverity severity)
        {
            if (self.severity() == severity)
                return self.shared_from_this();

            auto result = self.clone_with_severity(severity);
            DEBUG_ASSERT(typeid(*this) == typeid(*result));
            DEBUG_ASSERT(severity == result->severity());
            return std::static_pointer_cast<const Self>(std::move(result));
        }

        [[nodiscard]] virtual std::shared_ptr<const DiagnosticInfo> clone_with_severity(
            DiagnosticSeverity severity) const;

      private:
        constexpr static DiagnosticSeverity get_severity(const DiagnosticCode code)
        {
            return diagnostics::get_descriptor(code)
                .transform([](const DiagnosticDescriptor &descriptor) { return descriptor.default_severity(); })
                .value_or(DiagnosticSeverity::error);
        }

        constexpr static std::string_view get_message(const DiagnosticCode code)
        {
            return diagnostics::get_descriptor(code)
                .transform([](const DiagnosticDescriptor &descriptor) { return descriptor.format_message(); })
                .value_or("<unknown>");
        }

        const DiagnosticDescriptor &descriptor_;
        DiagnosticSeverity default_severity_{};
        DiagnosticSeverity effective_severity_{};
        std::shared_ptr<DiagnosticArguments> arguments_;
    };

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
