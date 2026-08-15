/**
 * @file declaration.ixx
 * @author Francesco Corso
 * @date 8/13/2026
 * @brief
 */
export module prism.core:declarations.declaration;

import std;
import :util.noncopyable;
import :text.name;
import :semantic.semantic_lifetime;

namespace prism
{

    enum class DeclarationKind : std::uint8_t
    {
        namespace_,
        variable,
        function,
        parameter
    };

    class Declaration;

    template <typename T>
    concept HasChildType = requires { typename T::ChildType; };

    class Declaration : NonCopyable
    {
      protected:
        constexpr explicit Declaration(SemanticLifetime &lifetime, Name name) noexcept
            : name_{std::move(name)}, lifetime_{&lifetime}
        {
        }

        ~Declaration() noexcept = default;

      public:
        [[nodiscard]] virtual DeclarationKind kind() const noexcept = 0;

        [[nodiscard]] constexpr Name name() const noexcept
        {
            return name_;
        }

        template <typename Self>
        [[nodiscard]] constexpr std::shared_ptr<Self> shared_from_this(this Self &self) noexcept
        {
            return std::shared_ptr<Self>{self.lifetime_->shared_from_this(), std::addressof(self)};
        }

        template <typename Self>
        [[nodiscard]] constexpr auto children(this const Self &self)
        {
            if constexpr (HasChildType<Self>)
            {
                return static_cast<const Declaration &>(self).get_declaration_children() |
                       std::views::transform([](const auto &child) -> auto &
                                             { return static_cast<const Self::ChildType &>(child.get()); });
            }
            else
            {
                return static_cast<const Declaration &>(self).get_declaration_children();
            }
        }

      protected:
        [[nodiscard]] virtual std::span<const Ref<const Declaration>> get_declaration_children() const = 0;

        [[nodiscard]] constexpr SemanticLifetime &lifetime() const noexcept
        {
            return *lifetime_;
        }

      private:
        Name name_;
        SemanticLifetime *lifetime_;
    };
} // namespace prism
