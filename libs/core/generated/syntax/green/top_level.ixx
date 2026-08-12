export module prism.core:syntax.green.top_level;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.trivia;
import :syntax.green.separated_list;

namespace prism
{
    class GreenDeclaration;
    class GreenUsingDirective;

    class GreenCompilationUnit final : public GreenNode
    {
      public:
        GreenCompilationUnit(GreenSyntaxList<GreenUsingDirective> usings,
                             GreenSyntaxList<GreenDeclaration> members,
                             DiagnosticInfoList diagnostics = {});

        ~GreenCompilationUnit() override;

        [[nodiscard]] constexpr const GreenSyntaxList<GreenUsingDirective> &usings() const noexcept
        {
            return usings_;
        }

        void set_usings(GreenSyntaxList<GreenUsingDirective> value) noexcept;

        [[nodiscard]] constexpr const GreenSyntaxList<GreenDeclaration> &members() const noexcept
        {
            return members_;
        }

        void set_members(GreenSyntaxList<GreenDeclaration> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::compilation_unit;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenCompilationUnit> with_usings(GreenSyntaxList<GreenUsingDirective> usings) const;

        [[nodiscard]] GreenPtr<GreenCompilationUnit> with_members(GreenSyntaxList<GreenDeclaration> members) const;

        [[nodiscard]] GreenPtr<GreenCompilationUnit> update(GreenSyntaxList<GreenUsingDirective> usings,
                                                            GreenSyntaxList<GreenDeclaration> members) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenSyntaxList<GreenUsingDirective> usings_;
        GreenSyntaxList<GreenDeclaration> members_;
    };

    template <>
    struct GreenNodeTraits<GreenCompilationUnit>
    {
        static constexpr std::size_t slot_count = 2;

        using ChildTypes = std::tuple<GreenSyntaxList<GreenUsingDirective>, GreenSyntaxList<GreenDeclaration>>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenCompilationUnit &node)
        {
            if constexpr (N == 0)
            {
                return node.usings();
            }
            else
            {
                static_assert(N == 1);
                return node.members();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenCompilationUnit>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenCompilationUnit &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_usings(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                node.set_members(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenCompilationUnit>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenCompilationUnit> with(const GreenCompilationUnit &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_usings(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                return node.with_members(std::forward<Arg>(value));
            }
        }
    };
} // namespace prism
