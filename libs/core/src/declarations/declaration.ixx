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
        constexpr explicit Declaration(Name name, const DeclarationKind kind, const bool is_merged) noexcept
            : name_{std::move(name)}, kind_{kind}, is_merged_{is_merged}
        {
        }

      public:
        virtual ~Declaration() noexcept = default;

        [[nodiscard]] constexpr DeclarationKind kind() const noexcept
        {
            return kind_;
        }

        [[nodiscard]] constexpr bool is_merged() const noexcept
        {
            return is_merged_;
        }

        [[nodiscard]] constexpr Name name() const noexcept
        {
            return name_;
        }

      private:
        Name name_;
        DeclarationKind kind_;
        bool is_merged_ = false;
    };
} // namespace prism
