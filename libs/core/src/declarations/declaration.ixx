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

    class Declaration : public IntrusiveRefCounted
    {
      protected:
        constexpr explicit Declaration(Name name) noexcept : name_{std::move(name)}
        {
        }

      public:
        virtual ~Declaration() noexcept = default;

        [[nodiscard]] virtual DeclarationKind kind() const noexcept = 0;

        [[nodiscard]] constexpr Name name() const noexcept
        {
            return name_;
        }

      private:
        Name name_;
    };
} // namespace prism
