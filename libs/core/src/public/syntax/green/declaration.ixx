/**
 * @file declaration.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
export module prism.core:syntax.green.declaration;

import :syntax.green.node;
import :syntax.green.token;

namespace prism
{
    class GreenInitializer;
    class GreenTypeHint;

    class GreenDeclaration : public GreenNode
    {
      protected:
        GreenDeclaration(SyntaxKind kind, GreenPtr<GreenTokenList> modifiers);

      public:
        [[nodiscard]] constexpr Optional<const GreenTokenList &> modifiers() const noexcept
        {
            return modifiers_.get();
        }

      private:
        GreenPtr<GreenTokenList> modifiers_;
    };

    class GreenVariableDeclaration final : public GreenDeclaration
    {
      public:
        GreenVariableDeclaration(GreenPtr<GreenTokenList> modifiers,
                                 GreenPtr<GreenToken> var_keyword,
                                 GreenPtr<GreenToken> identifier,
                                 GreenPtr<GreenTypeHint> type_hint,
                                 GreenPtr<GreenInitializer> initializer,
                                 GreenPtr<GreenToken> semicolon);

        [[nodiscard]] constexpr const GreenToken &var_keyword() const noexcept
        {
            return *var_keyword_;
        }

        [[nodiscard]] constexpr const GreenToken &identifier() const noexcept
        {
            return *identifier_;
        }

        [[nodiscard]] constexpr Optional<const GreenTypeHint &> type_hint() const noexcept
        {
            return type_hint_.get();
        }

        [[nodiscard]] constexpr Optional<const GreenInitializer &> initializer() const noexcept
        {
            return initializer_.get();
        }

        [[nodiscard]] constexpr const GreenToken &semicolon() const noexcept
        {
            return *semicolon_;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      private:
        GreenPtr<GreenToken> var_keyword_;
        GreenPtr<GreenToken> identifier_;
        GreenPtr<GreenTypeHint> type_hint_;
        GreenPtr<GreenInitializer> initializer_;
        GreenPtr<GreenToken> semicolon_;
    };
} // namespace prism
