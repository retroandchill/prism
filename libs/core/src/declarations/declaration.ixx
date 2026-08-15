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
    };

    class Declaration;

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

      protected:
        [[nodiscard]] constexpr SemanticLifetime &lifetime() const noexcept
        {
            return *lifetime_;
        }

      private:
        Name name_;
        SemanticLifetime *lifetime_;
    };
} // namespace prism
