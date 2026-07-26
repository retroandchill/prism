export module prism.core:syntax.green.top_level;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.trivia;
import :syntax.green.separated_list;

namespace prism
{
    class GreenDeclaration;

    class GreenCompilationUnit final : public GreenNode
    {
      public:
        explicit GreenCompilationUnit(GreenSyntaxList<GreenDeclaration> members, DiagnosticInfoList diagnostics = {});

        ~GreenCompilationUnit() override;

        [[nodiscard]] constexpr GreenSyntaxList<GreenDeclaration> members() const noexcept
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

        [[nodiscard]] GreenPtr<GreenCompilationUnit> with_members(GreenSyntaxList<GreenDeclaration> members) const;

        [[nodiscard]] GreenPtr<GreenCompilationUnit> update(GreenSyntaxList<GreenDeclaration> members) const;

      protected:
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenSyntaxList<GreenDeclaration> members_;
    };
} // namespace prism
