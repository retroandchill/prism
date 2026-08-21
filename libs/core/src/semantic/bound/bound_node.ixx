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

        bad_expression,
        literal,
        variable_access,
        parameter_access,
        overload_set,
        unary_expression,
        binary_expression,
        assignment_expression,
        conditional_expression,
        call_expression,
        conversion_expression
    };

    template <typename T>
    using BoundSpan = std::span<const Ref<const T>>;

    template <typename T>
    using BoundPtr = RefCountPtr<const T>;

    template <typename T>
    using BoundList = ImmutableArray<BoundPtr<T>>;

    class BoundNode : public IntrusiveRefCounted
    {
      protected:
        constexpr BoundNode(const BoundNodeKind kind, const SyntaxNode &syntax_node)
            : kind_{kind}, syntax_node_{syntax_node}
        {
        }

      public:
        virtual ~BoundNode() = default;

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
