export module prism.core:syntax.green.types;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.separated_list;

namespace prism
{
    class GreenType : public GreenNode
    {
      protected:
        explicit constexpr GreenType(const SyntaxKind kind, DiagnosticInfoList diagnostics = {})
            : GreenNode{kind, std::move(diagnostics)}
        {
        }

      public:
        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::identifier_named_type;
        }
    };

    class GreenNamedType : public GreenType
    {
      protected:
        explicit constexpr GreenNamedType(const SyntaxKind kind, DiagnosticInfoList diagnostics = {})
            : GreenType{kind, std::move(diagnostics)}
        {
        }

      public:
        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::identifier_named_type;
        }
    };

    class GreenSimpleNamedType : public GreenNamedType
    {
      protected:
        explicit constexpr GreenSimpleNamedType(const SyntaxKind kind, DiagnosticInfoList diagnostics = {})
            : GreenNamedType{kind, std::move(diagnostics)}
        {
        }

      public:
        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::identifier_named_type;
        }
    };

    class GreenIdentifierNamedType final : public GreenSimpleNamedType
    {
      public:
        explicit GreenIdentifierNamedType(RefCountPtr<const GreenToken> identifier,
                                          DiagnosticInfoList diagnostics = {});

        ~GreenIdentifierNamedType() override;

        [[nodiscard]] constexpr const GreenToken &identifier() const noexcept
        {
            return *identifier_;
        }

        [[nodiscard]] static constexpr bool instanceof (const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::identifier_named_type;
        }

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const override;

        [[nodiscard]] RefCountPtr<const GreenIdentifierNamedType> with_identifier(
            RefCountPtr<const GreenToken> identifier) const;

        [[nodiscard]] RefCountPtr<const GreenIdentifierNamedType> update(
            RefCountPtr<const GreenToken> identifier) const;

      private:
        RefCountPtr<const GreenToken> identifier_;
    };
} // namespace prism
