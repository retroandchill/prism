export module prism.core:syntax.green.declaration;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.separated_list;

namespace prism
{
    class GreenInitializer;
    class GreenTypeHint;

    class GreenDeclaration : public GreenNode
    {
      protected:
        explicit constexpr GreenDeclaration(const SyntaxKind kind, DiagnosticInfoList diagnostics = {})
            : GreenNode{kind, std::move(diagnostics)}
        {
        }

      public:
        ~GreenDeclaration() override;

        [[nodiscard]] virtual const GreenSyntaxList<GreenToken> &modifiers() const noexcept = 0;
    };

    class GreenVariableDeclaration final : public GreenDeclaration
    {
      public:
        GreenVariableDeclaration(SyntaxKind kind,
                                 GreenSyntaxList<GreenToken> modifiers,
                                 GreenPtr<GreenToken> var_keyword,
                                 GreenPtr<GreenToken> mut_keyword,
                                 GreenPtr<GreenToken> identifier,
                                 GreenPtr<GreenTypeHint> type,
                                 GreenPtr<GreenInitializer> initializer,
                                 GreenPtr<GreenToken> semicolon,
                                 DiagnosticInfoList diagnostics = {});
        ~GreenVariableDeclaration() override;

        [[nodiscard]] constexpr const GreenSyntaxList<GreenToken> &modifiers() const noexcept override
        {
            return modifiers_;
        }

        [[nodiscard]] constexpr const GreenToken &var_keyword() const noexcept
        {
            return *var_keyword_;
        }

        [[nodiscard]] constexpr Optional<const GreenToken &> mut_keyword() const noexcept
        {
            return mut_keyword_.get();
        }

        [[nodiscard]] constexpr const GreenToken &identifier() const noexcept
        {
            return *identifier_;
        }

        [[nodiscard]] constexpr const GreenTypeHint &type() const noexcept
        {
            return *type_;
        }

        [[nodiscard]] constexpr const GreenInitializer &initializer() const noexcept
        {
            return *initializer_;
        }

        [[nodiscard]] constexpr const GreenToken &semicolon() const noexcept
        {
            return *semicolon_;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

      private:
        GreenSyntaxList<GreenToken> modifiers_;
        GreenPtr<GreenToken> var_keyword_;
        GreenPtr<GreenToken> mut_keyword_;
        GreenPtr<GreenToken> identifier_;
        GreenPtr<GreenTypeHint> type_;
        GreenPtr<GreenInitializer> initializer_;
        GreenPtr<GreenToken> semicolon_;
    };

} // namespace prism
