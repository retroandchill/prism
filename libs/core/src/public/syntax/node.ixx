/**
 * @file node.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core:syntax.node;

import :syntax.green.node;
import :util.noncopyable;
import :text.text_span;

namespace prism
{
    class SyntaxTree;
    class SyntaxLifetime;
    class ChildSyntaxList;
    class SyntaxToken;
    class SyntaxTriviaList;
    export class SyntaxNode;

    template <typename T>
    concept SyntaxNodeLike =
        std::same_as<T, SyntaxNode> || (std::derived_from<T, SyntaxNode> && requires(const SyntaxNode &node) {
            {
                T:: instanceof (node)
            } -> std::convertible_to<bool>;
        });

    class PRISM_CORE_API SyntaxNode : NonCopyable
    {
      protected:
        constexpr SyntaxNode(const GreenNode &node, const SyntaxTree &tree, const std::uint32_t position)
            : green_{&node}, tree_{&tree}, position_{position}
        {
        }

        constexpr SyntaxNode(const GreenNode &node, const SyntaxNode &parent, const std::uint32_t position)
            : green_{&node}, parent_{&parent}, position_{position}
        {
        }

        constexpr ~SyntaxNode() = default;

      public:
        [[nodiscard]] constexpr SyntaxKind kind() const noexcept
        {
            return green_->kind();
        }

        [[nodiscard]] constexpr std::uint32_t position() const noexcept
        {
            return position_;
        }

        [[nodiscard]] constexpr std::uint32_t end_position() const noexcept
        {
            return position_ + green_->full_width();
        }

        [[nodiscard]] constexpr TextSpan full_span() const noexcept
        {
            return {.start = position_, .length = green_->full_width()};
        }

        [[nodiscard]] constexpr TextSpan span() const
        {
            return {.start = position_ + green_->leading_trivia_width(), .length = green_->width()};
        }

        [[nodiscard]] constexpr bool is_missing() const noexcept
        {
            return green_->is_missing();
        }

        [[nodiscard]] constexpr bool contains_diagnostics() const noexcept
        {
            return green_->contains_diagnostics();
        }

        [[nodiscard]] constexpr bool has_leading_trivia() const noexcept
        {
            return green_->has_leading_trivia();
        }

        [[nodiscard]] SyntaxTriviaList leading_trivia() const noexcept;

        [[nodiscard]] constexpr bool has_trailing_trivia() const noexcept
        {
            return green_->has_trailing_trivia();
        }

        [[nodiscard]] SyntaxTriviaList trailing_trivia() const noexcept;

        [[nodiscard]] constexpr Optional<const SyntaxNode &> parent() const noexcept
        {
            return parent_;
        }

        [[nodiscard]] const SyntaxTree &tree() const;

      protected:
        [[nodiscard]] SyntaxLifetime &lifetime() const;

        [[nodiscard]] virtual Optional<const SyntaxNode &> get_node_slot(std::size_t index) const = 0;

        template <std::derived_from<SyntaxNode> T>
        Optional<const T &> get_red(std::atomic<const T *> &slot) const
        {
            auto *result = slot.load(std::memory_order_acquire);

            if (result == nullptr)
            {
                if (const auto green = green_->get_child(0); green.has_value())
                {
                    slot.compare_exchange_strong(
                        result,
                        static_cast<const T *>(std::addressof(green->create_red(lifetime(), this, position_))),
                        std::memory_order_acq_rel,
                        std::memory_order_relaxed);
                }
            }

            return result;
        }

        template <std::derived_from<SyntaxNode> T>
        Optional<const T &> get_red(std::atomic<const T *> &slot, const std::size_t index) const
        {
            auto *result = slot.load(std::memory_order_acquire);

            if (result == nullptr)
            {
                if (const auto green = green_->get_child(index); green.has_value())
                {
                    slot.compare_exchange_strong(result,
                                                 static_cast<const T *>(std::addressof(
                                                     green->create_red(lifetime(), this, get_child_position(index)))),
                                                 std::memory_order_acq_rel,
                                                 std::memory_order_relaxed);
                }
            }

            return result;
        }

        [[nodiscard]] virtual Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const = 0;

        [[nodiscard]] virtual std::uint32_t get_child_position(std::size_t index) const;

      public:
        [[nodiscard]] ChildSyntaxList child_nodes_and_tokens() const;

        [[nodiscard]] std::generator<const SyntaxNode &> child_nodes() const;

        [[nodiscard]] std::generator<SyntaxToken> child_tokens() const;

        [[nodiscard]] SyntaxToken first_token() const;

        [[nodiscard]] SyntaxToken last_token() const;

        template <SyntaxNodeLike T>
        [[nodiscard]] bool is() const
        {
            if constexpr (std::is_same_v<T, SyntaxNode>)
            {
                return true;
            }
            else
            {
                return T:: instanceof (*this);
            }
        }

        template <SyntaxNodeLike T>
        [[nodiscard]] Optional<const T &> as() const
        {
            if constexpr (std::is_same_v<T, SyntaxNode>)
            {
                return *this;
            }
            else
            {
                return T:: instanceof (*this) ? Optional<const T &>{static_cast<const T &>(*this)} : std::nullopt;
            }
        }

      protected:
        [[nodiscard]] constexpr const GreenNode &green() const noexcept
        {
            return *green_;
        }

      private:
        friend class ChildSyntaxList;

        static const SyntaxTree *compute_tree(const SyntaxNode *node);

        const GreenNode *green_;
        const SyntaxNode *parent_ = nullptr;
        mutable std::atomic<const SyntaxTree *> tree_ = nullptr;
        std::uint32_t position_;
    };
} // namespace prism
