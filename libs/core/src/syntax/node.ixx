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
import :diagnostics.location;
import :syntax.lifetime;
import :util.lazy;

namespace prism
{
    class SyntaxTree;
    class SyntaxLifetime;
    class ChildSyntaxList;
    class SyntaxToken;
    class SyntaxTokenList;
    class SyntaxTriviaList;
    export class SyntaxNode;
    template <typename T>
    class SyntaxList;
    template <typename T>
    class SeparatedSyntaxList;
    class SyntaxNodeOrTokenList;

    template <typename T>
    concept SyntaxNodeLike =
        std::same_as<T, SyntaxNode> || (std::derived_from<T, SyntaxNode> && requires(const SyntaxNode &node) {
            {
                T::instance_of(node)
            } -> std::convertible_to<bool>;
        });

    class PRISM_CORE_API SyntaxNode : NonCopyable
    {
      protected:
        constexpr SyntaxNode(SyntaxLifetime &lifetime,
                             const GreenNode &node,
                             const SyntaxNode *parent,
                             const std::uint32_t position)
            : green_{&node}, parent_{parent}, lifetime_{&lifetime}, position_{position}
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

        [[nodiscard]] Location location() const;

      protected:
        [[nodiscard]] constexpr SyntaxLifetime &lifetime() const noexcept
        {
            return *lifetime_;
        }

        [[nodiscard]] virtual Optional<const SyntaxNode &> get_node_slot(std::size_t index) const = 0;

        [[nodiscard]] const SyntaxNode &get_required_node_slot(std::size_t index) const;

        template <std::derived_from<SyntaxNode> T>
        Optional<const T &> get_red(Lazy<const T *> &slot) const
        {
            return slot.get_or_compute(
                [this]
                {
                    return green_->get_slot(0)
                        .transform(
                            [this](const GreenNode &green) {
                                return static_cast<const T *>(
                                    std::addressof(green.create_red(lifetime(), this, position_)));
                            })
                        .value_or(nullptr);
                });
        }

        template <std::derived_from<SyntaxNode> T>
        Optional<const T &> get_red(Lazy<const T *> &slot, const std::size_t index) const
        {
            return slot.get_or_compute(
                [this, index]
                {
                    return green_->get_slot(index)
                        .transform(
                            [this, index](const GreenNode &green) {
                                return static_cast<const T *>(
                                    std::addressof(green.create_red(lifetime(), this, get_slot_position(index))));
                            })
                        .value_or(nullptr);
                });
        }

        template <std::derived_from<SyntaxNode> T>
        SyntaxList<T> make_syntax_list(const Optional<const SyntaxNode &> red) const
        {
            return SyntaxList<T>{red.value_ptr()};
        }

        template <std::derived_from<SyntaxNode> T>
        SeparatedSyntaxList<T> make_separated_syntax_list(const Optional<const SyntaxNode &> red) const
        {
            // ReSharper disable once CppClassIsIncomplete
            return SeparatedSyntaxList<T>{SyntaxNodeOrTokenList{red.value_ptr()}};
        }

        [[nodiscard]] virtual Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const = 0;

        [[nodiscard]] virtual std::uint32_t get_slot_position(std::size_t index) const;

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
                return T::instance_of(*this);
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
                return T::instance_of(*this) ? Optional<const T &>{static_cast<const T &>(*this)} : std::nullopt;
            }
        }

        template <typename Self>
        [[nodiscard]] std::shared_ptr<Self> shared_from_this(this Self &self) noexcept
        {
            return std::shared_ptr<Self>{lifetime_->shared_from_this(), std::addressof(self)};
        }

      protected:
        [[nodiscard]] constexpr const GreenNode &green() const noexcept
        {
            return *green_;
        }

      private:
        friend class SyntaxNodeOrTokenList;
        friend class ChildSyntaxList;
        friend class SyntaxTree;
        template <typename T>
        friend class SeparatedSyntaxList;

        static const SyntaxTree &compute_tree(const SyntaxNode *node);

        const GreenNode *green_;
        const SyntaxNode *parent_ = nullptr;
        SyntaxLifetime *lifetime_;
        mutable Lazy<const SyntaxTree &> tree_;
        std::uint32_t position_;
    };
} // namespace prism
