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
        unary_expression,
        binary_expression,
        assignment_expression,
        conditional_expression,
        call_expression,
        conversion_expression
    };

    [[nodiscard]] constexpr bool is_statement(const BoundNodeKind kind) noexcept
    {
        return std::to_underlying(kind) >= std::to_underlying(BoundNodeKind::block) &&
               std::to_underlying(kind) <= std::to_underlying(BoundNodeKind::return_statement);
    }

    [[nodiscard]] constexpr bool is_expression(const BoundNodeKind kind) noexcept
    {
        return std::to_underlying(kind) >= std::to_underlying(BoundNodeKind::bad_expression) &&
               std::to_underlying(kind) <= std::to_underlying(BoundNodeKind::conversion_expression);
    }

    template <typename T>
    using BoundSpan = std::span<const Ref<const T>>;

    template <typename T>
    using BoundPtr = RefCountPtr<const T>;

    template <typename T>
    using BoundList = ImmutableArray<BoundPtr<T>>;

    class BoundNode;

    template <typename T>
    concept BoundNodeLike =
        std::same_as<T, BoundNode> || (std::derived_from<T, BoundNode> && requires(const BoundNode &node) {
            {
                T::instance_of(node)
            } -> std::convertible_to<bool>;
        });

    class BoundNode : NonCopyable
    {
      protected:
        constexpr BoundNode(const BoundNodeKind kind, const SyntaxNode &syntax_node)
            : kind_{kind}, syntax_node_{syntax_node}
        {
        }

        ~BoundNode() = default;

      public:
        [[nodiscard]] constexpr BoundNodeKind kind() const noexcept
        {
            return kind_;
        }

        [[nodiscard]] constexpr const SyntaxNode &syntax() const noexcept
        {
            return syntax_node_;
        }

        template <BoundNodeLike T>
        [[nodiscard]] bool is() const
        {
            if constexpr (std::is_same_v<T, BoundNode>)
            {
                return true;
            }
            else
            {
                return T::instance_of(*this);
            }
        }

        template <BoundNodeLike... Ts>
        [[nodiscard]] bool is_any_of() const
        {
            return (is<Ts>() || ...);
        }

        template <BoundNodeLike T, typename Self>
            requires std::derived_from<T, Self>
        [[nodiscard]] Optional<const T &> as(this const Self &self)
        {
            if constexpr (std::is_same_v<T, BoundNode>)
            {
                return self;
            }
            else
            {
                return T::instance_of(self) ? Optional<const T &>{static_cast<const T &>(self)} : std::nullopt;
            }
        }

        template <BoundNodeLike T, typename Self>
        [[nodiscard]] const T &as_checked(this const Self &self)
        {
            auto result = self.template as<T>();
            DEBUG_ASSERT(result.has_value());
            return result.value();
        }

      private:
        BoundNodeKind kind_;
        const SyntaxNode &syntax_node_;
    };
} // namespace prism
