/**
 * @file bound_node.ixx
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
export module prism.core:semantic.bound.bound_node;

import :syntax.node;
import :util.ref;

namespace prism
{
    enum class BoundNodeKind : std::uint8_t
    {
        error,

        block,
        variable_declaration,
        expression_statement,
        return_statement,

        literal_expression,
        variable_reference_expression,
        unary_expression,
        binary_expression,
        assignment_expression,
        conditional_expression,
        call_expression,
        conversion_expression
    };

    template <typename T>
    using BoundSpan = std::span<const Ref<const T>>;

    class BoundNode : NonCopyable
    {
      protected:
        constexpr BoundNode(const BoundNodeKind kind, const SyntaxNode &syntax_node)
            : kind_{kind}, syntax_node_{syntax_node}
        {
        }

        ~BoundNode() noexcept = default;

      public:
        [[nodiscard]] constexpr BoundNodeKind kind() const noexcept
        {
            return kind_;
        }

        [[nodiscard]] constexpr const SyntaxNode &syntax() const noexcept
        {
            return syntax_node_;
        }

      private:
        BoundNodeKind kind_;
        const SyntaxNode &syntax_node_;
    };
} // namespace prism
