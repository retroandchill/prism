/**
 * @file list.ixx
 * @author Francesco Corso
 * @date 7/21/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

#include <libassert/assert-macros.hpp>

export module prism.core:syntax.list;

import :syntax.list_view;
import :syntax.node;
import :syntax.green.list;
import libassert;
import :syntax.node_or_token;

namespace prism
{
    export template <typename T>
    class SyntaxList;

    export template <typename T>
    class SeparatedSyntaxList;

    class PRISM_CORE_API SyntaxListImpl final : public SyntaxNode, public SyntaxListView<SyntaxNodeOrToken>
    {
      public:
        constexpr SyntaxListImpl(const GreenListNode &node, const SyntaxTree &tree, const std::uint32_t position)
            : SyntaxNode{node, tree, position}
        {
        }

        constexpr SyntaxListImpl(const GreenListNode &node, const SyntaxNode &parent, const std::uint32_t position)
            : SyntaxNode{node, parent, position}
        {
        }

        [[nodiscard]] static constexpr bool instance_of(const SyntaxNode &node) noexcept
        {
            return node.kind() == SyntaxKind::list;
        }

      private:
        std::span<std::atomic<const SyntaxNode *>> create_children() const;

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;

        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        template <typename T>
        friend class SyntaxList;

        std::span<std::atomic<const SyntaxNode *>> children_ = create_children();
    };

    template <typename T>
    class SyntaxList final : public SyntaxListView<T>
    {
        constexpr explicit SyntaxList(const SyntaxNode *list) : node_{list}
        {
        }

      public:
        constexpr SyntaxList() = default;

        constexpr explicit SyntaxList(const T &node) : node_{&node}
        {
        }

        constexpr explicit SyntaxList(const SyntaxListImpl &list) : node_{&list}
        {
        }

        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            if (node_ == nullptr)
                return 0;

            if (const auto list = node_->as<SyntaxListImpl>(); list.has_value())
                return list->children_.size();

            return 1;
        }

        [[nodiscard]] constexpr const T &operator[](const std::size_t index) const
        {
            if (node_ == nullptr)
                throw std::out_of_range{"index out of range"};

            if (const auto list = node_->as<SyntaxListImpl>(); list.has_value())
            {
                if (index >= list->children_.size())
                    throw std::out_of_range{"index out of range"};

                return static_cast<const T &>(*list->children_[index]);
            }

            if (index != 0)
                throw std::out_of_range{"index out of range"};

            DEBUG_ASSERT(node_->is<T>());
            return static_cast<const T &>(*node_);
        }

      private:
        friend class SyntaxNode;

        const SyntaxNode *node_ = nullptr;
    };

    template <typename T>
    class SeparatedSyntaxList : public SyntaxListView<T>
    {
      public:
        constexpr SeparatedSyntaxList() = default;

        constexpr explicit SeparatedSyntaxList(const SyntaxNodeOrTokenList list) : list_{list}
        {
#ifndef NDEBUG
            validate(list_);
#endif
        }

        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            return (list_.size() + 1) / 2;
        }

        [[nodiscard]] constexpr std::size_t separator_count() const noexcept
        {
            return list_.size() / 2;
        }

        [[nodiscard]] constexpr const T &operator[](const std::size_t index) const noexcept
        {
            auto node = list_.node();
            if (!node.has_value())
                throw std::out_of_range{"index out of range"};

            if (!node->green().is_list())
            {
                if (index != 0)
                    throw std::out_of_range{"index out of range"};

                DEBUG_ASSERT(node->is<T>());
                return static_cast<const T &>(*node);
            }

            if (index >= node->green().child_count())
                throw std::out_of_range{"index out of range"};

            auto &target = node->get_required_node_slot(index);
            DEBUG_ASSERT(target.is<T>());
            return static_cast<const T &>(target);
        }

        [[nodiscard]] constexpr SyntaxToken separator(const std::size_t index) const noexcept
        {
            const auto node = list_.node();
            if (!node.has_value())
                throw std::out_of_range{"index out of range"};

            DEBUG_ASSERT(node->green().is_list(), "separator cannot appear in a non-list");
            if (index >= node->green().child_count())
                throw std::out_of_range{"index out of range"};

            auto &green = node->green().get_required_child(index);
            DEBUG_ASSERT(green.is_token());
            return SyntaxToken{static_cast<const GreenToken &>(green),
                               node.value_ptr(),
                               node->get_child_position(index)};
        }

        [[nodiscard]] constexpr SyntaxNodeOrTokenList with_separators() const noexcept
        {
            return list_;
        }

      private:
        friend class SyntaxNode;

#ifndef NDEBUG
        static void validate(const SyntaxNodeOrTokenList list)
        {
            for (const auto [i, item] : list | std::views::enumerate)
            {
                if (i % 2 == 0)
                {
                    DEBUG_ASSERT(!holds_alternative<SyntaxToken>(item));
                }
                else
                {
                    DEBUG_ASSERT(holds_alternative<SyntaxToken>(item));
                }
            }
        }
#endif

        SyntaxNodeOrTokenList list_;
    };

} // namespace prism
