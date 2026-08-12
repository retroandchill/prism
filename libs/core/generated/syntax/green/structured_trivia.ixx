export module prism.core:syntax.green.structured_trivia;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.trivia;
import :syntax.green.separated_list;

namespace prism
{
    class GreenSkippedTokensTrivia final : public GreenStructuredTrivia
    {
      public:
        explicit GreenSkippedTokensTrivia(GreenSyntaxList<GreenToken> tokens, DiagnosticInfoList diagnostics = {});

        ~GreenSkippedTokensTrivia() override;

        [[nodiscard]] constexpr const GreenSyntaxList<GreenToken> &tokens() const noexcept
        {
            return tokens_;
        }

        void set_tokens(GreenSyntaxList<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::skipped_tokens_trivia;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenSkippedTokensTrivia> with_tokens(GreenSyntaxList<GreenToken> tokens) const;

        [[nodiscard]] GreenPtr<GreenSkippedTokensTrivia> update(GreenSyntaxList<GreenToken> tokens) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenSyntaxList<GreenToken> tokens_;
    };

    template <>
    struct GreenNodeTraits<GreenSkippedTokensTrivia>
    {
        static constexpr std::size_t slot_count = 1;

        using ChildTypes = std::tuple<GreenSyntaxList<GreenToken>>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenSkippedTokensTrivia &node)
        {
            {
                static_assert(N == 0);
                return node.tokens();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenSkippedTokensTrivia>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenSkippedTokensTrivia &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                node.set_tokens(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenSkippedTokensTrivia>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenSkippedTokensTrivia> with(const GreenSkippedTokensTrivia &node, Arg &&value)
        {
            {
                static_assert(N == 0);
                return node.with_tokens(std::forward<Arg>(value));
            }
        }
    };
} // namespace prism
