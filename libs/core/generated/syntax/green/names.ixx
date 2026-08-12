export module prism.core:syntax.green.names;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.trivia;
import :syntax.green.separated_list;

namespace prism
{
    class GreenName : public GreenNode
    {
      protected:
        explicit constexpr GreenName(const SyntaxKind kind, DiagnosticInfoList diagnostics = {})
            : GreenNode{kind, std::move(diagnostics)}
        {
        }

      public:
        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::simple_name || node.kind() == SyntaxKind::qualified_name;
        }
    };

    class GreenSimpleName final : public GreenName
    {
      public:
        explicit GreenSimpleName(GreenPtr<GreenToken> identifier, DiagnosticInfoList diagnostics = {});

        ~GreenSimpleName() override;

        [[nodiscard]] constexpr const GreenToken &identifier() const noexcept
        {
            return *identifier_;
        }

        void set_identifier(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::simple_name;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenSimpleName> with_identifier(GreenPtr<GreenToken> identifier) const;

        [[nodiscard]] GreenPtr<GreenSimpleName> update(GreenPtr<GreenToken> identifier) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> identifier_;
    };

    template <>
    struct GreenNodeTraits<GreenSimpleName>
    {
        static constexpr std::size_t slot_count = 1;

        using ChildTypes = std::tuple<GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenSimpleName &node)
        {
            {
                static_assert(N == 0);
                return node.identifier();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenSimpleName>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenSimpleName &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                node.set_identifier(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenSimpleName>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenSimpleName> with(const GreenSimpleName &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                return node.with_identifier(std::forward<Arg>(value));
            }
        }
    };

    class GreenQualifiedName final : public GreenName
    {
      public:
        GreenQualifiedName(GreenPtr<GreenName> left,
                           GreenPtr<GreenToken> separator,
                           GreenPtr<GreenSimpleName> right,
                           DiagnosticInfoList diagnostics = {});

        ~GreenQualifiedName() override;

        [[nodiscard]] constexpr const GreenName &left() const noexcept
        {
            return *left_;
        }

        void set_left(GreenPtr<GreenName> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &separator() const noexcept
        {
            return *separator_;
        }

        void set_separator(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenSimpleName &right() const noexcept
        {
            return *right_;
        }

        void set_right(GreenPtr<GreenSimpleName> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::qualified_name;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenQualifiedName> with_left(GreenPtr<GreenName> left) const;

        [[nodiscard]] GreenPtr<GreenQualifiedName> with_separator(GreenPtr<GreenToken> separator) const;

        [[nodiscard]] GreenPtr<GreenQualifiedName> with_right(GreenPtr<GreenSimpleName> right) const;

        [[nodiscard]] GreenPtr<GreenQualifiedName> update(GreenPtr<GreenName> left,
                                                          GreenPtr<GreenToken> separator,
                                                          GreenPtr<GreenSimpleName> right) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenName> left_;
        GreenPtr<GreenToken> separator_;
        GreenPtr<GreenSimpleName> right_;
    };

    template <>
    struct GreenNodeTraits<GreenQualifiedName>
    {
        static constexpr std::size_t slot_count = 3;

        using ChildTypes = std::tuple<GreenName, GreenToken, GreenSimpleName>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenQualifiedName &node)
        {
            if constexpr (N == 0)
            {
                return node.left();
            }
            else if constexpr (N == 1)
            {
                return node.separator();
            }
            else
            {
                static_assert(N == 2);
                return node.right();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenQualifiedName>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenQualifiedName &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_left(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_separator(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                node.set_right(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenQualifiedName>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenQualifiedName> with(const GreenQualifiedName &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_left(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_separator(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                return node.with_right(std::forward<Arg>(value));
            }
        }
    };
} // namespace prism
