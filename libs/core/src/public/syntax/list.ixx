/**
 * @file list.ixx
 * @author Francesco Corso
 * @date 7/21/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

#include <cassert>

export module prism.core:syntax.list;

import :syntax.list_view;
import :syntax.node;
import :syntax.green.list;

namespace prism
{
    export template <SyntaxNodeLike T>
    class SyntaxList;

    class PRISM_CORE_API SyntaxListImpl final : public SyntaxNode
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

      private:
        std::span<std::atomic<const SyntaxNode *>> create_children() const;

      protected:
        [[nodiscard]] Optional<const SyntaxNode &> get_node_slot(std::size_t index) const override;

        [[nodiscard]] Optional<const SyntaxNode &> get_cached_slot(std::size_t index) const override;

      private:
        template <SyntaxNodeLike T>
        friend class SyntaxList;

        std::span<std::atomic<const SyntaxNode *>> children_ = create_children();
    };

    template <SyntaxNodeLike T>
    class SyntaxList final : public SyntaxListView<T>
    {
      public:
        constexpr SyntaxList() = default;

        constexpr explicit SyntaxList(const SyntaxListImpl &impl) : impl_{&impl}
        {
        }

        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            return impl_ != nullptr ? impl_->green().child_count() : 0;
        }

        [[nodiscard]] constexpr const T &operator[](const std::size_t index) const
        {
            assert(impl_ != nullptr);
            return impl_->get_node_slot(index).value();
        }

      private:
        const SyntaxListImpl *impl_ = nullptr;
    };
} // namespace prism
