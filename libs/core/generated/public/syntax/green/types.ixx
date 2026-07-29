export module prism.core:syntax.green.types;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.trivia;
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
        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::predefined_type || node.kind() == SyntaxKind::identifier_named_type;
        }
    };

    class GreenPredefinedType final : public GreenType
    {
      public:
        explicit GreenPredefinedType(GreenPtr<GreenToken> keyword, DiagnosticInfoList diagnostics = {});

        ~GreenPredefinedType() override;

        [[nodiscard]] constexpr const GreenToken &keyword() const noexcept
        {
            return *keyword_;
        }

        void set_keyword(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::predefined_type;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenPredefinedType> with_keyword(GreenPtr<GreenToken> keyword) const;

        [[nodiscard]] GreenPtr<GreenPredefinedType> update(GreenPtr<GreenToken> keyword) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> keyword_;
    };

    template <>
    struct GreenNodeTraits<GreenPredefinedType>
    {
        static constexpr std::size_t slot_count = 1;

        using ChildTypes = std::tuple<GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenPredefinedType &node)
        {
            {
                static_assert(N == 0);
                return node.keyword();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenPredefinedType>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenPredefinedType &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                node.set_keyword(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenPredefinedType>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenPredefinedType> with(const GreenPredefinedType &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                return node.with_keyword(std::forward<Arg>(value));
            }
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
        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
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
        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::identifier_named_type;
        }
    };

    class GreenIdentifierNamedType final : public GreenSimpleNamedType
    {
      public:
        explicit GreenIdentifierNamedType(GreenPtr<GreenToken> identifier, DiagnosticInfoList diagnostics = {});

        ~GreenIdentifierNamedType() override;

        [[nodiscard]] constexpr const GreenToken &identifier() const noexcept
        {
            return *identifier_;
        }

        void set_identifier(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::identifier_named_type;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenIdentifierNamedType> with_identifier(GreenPtr<GreenToken> identifier) const;

        [[nodiscard]] GreenPtr<GreenIdentifierNamedType> update(GreenPtr<GreenToken> identifier) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> identifier_;
    };

    template <>
    struct GreenNodeTraits<GreenIdentifierNamedType>
    {
        static constexpr std::size_t slot_count = 1;

        using ChildTypes = std::tuple<GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenIdentifierNamedType &node)
        {
            {
                static_assert(N == 0);
                return node.identifier();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenIdentifierNamedType>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenIdentifierNamedType &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                node.set_identifier(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenIdentifierNamedType>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenIdentifierNamedType> with(const GreenIdentifierNamedType &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                return node.with_identifier(std::forward<Arg>(value));
            }
        }
    };
} // namespace prism
