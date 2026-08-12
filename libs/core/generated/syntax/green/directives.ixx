export module prism.core:syntax.green.directives;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.trivia;
import :syntax.green.separated_list;

namespace prism
{
    class GreenName;

    class GreenUsingDirective final : public GreenNode
    {
      public:
        GreenUsingDirective(GreenPtr<GreenToken> using_keyword,
                            GreenPtr<GreenName> name,
                            GreenPtr<GreenToken> semicolon,
                            DiagnosticInfoList diagnostics = {});

        ~GreenUsingDirective() override;

        [[nodiscard]] constexpr const GreenToken &using_keyword() const noexcept
        {
            return *using_keyword_;
        }

        void set_using_keyword(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenName &name() const noexcept
        {
            return *name_;
        }

        void set_name(GreenPtr<GreenName> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &semicolon() const noexcept
        {
            return *semicolon_;
        }

        void set_semicolon(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::using_directive;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenUsingDirective> with_using_keyword(GreenPtr<GreenToken> using_keyword) const;

        [[nodiscard]] GreenPtr<GreenUsingDirective> with_name(GreenPtr<GreenName> name) const;

        [[nodiscard]] GreenPtr<GreenUsingDirective> with_semicolon(GreenPtr<GreenToken> semicolon) const;

        [[nodiscard]] GreenPtr<GreenUsingDirective> update(GreenPtr<GreenToken> using_keyword,
                                                           GreenPtr<GreenName> name,
                                                           GreenPtr<GreenToken> semicolon) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> using_keyword_;
        GreenPtr<GreenName> name_;
        GreenPtr<GreenToken> semicolon_;
    };

    template <>
    struct GreenNodeTraits<GreenUsingDirective>
    {
        static constexpr std::size_t slot_count = 3;

        using ChildTypes = std::tuple<GreenToken, GreenName, GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenUsingDirective &node)
        {
            if constexpr (N == 0)
            {
                return node.using_keyword();
            }
            else if constexpr (N == 1)
            {
                return node.name();
            }
            else
            {
                static_assert(N == 2);
                return node.semicolon();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenUsingDirective>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenUsingDirective &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_using_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_name(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                node.set_semicolon(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenUsingDirective>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenUsingDirective> with(const GreenUsingDirective &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_using_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_name(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                return node.with_semicolon(std::forward<Arg>(value));
            }
        }
    };
} // namespace prism
