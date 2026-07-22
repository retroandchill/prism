/**
 * @file child_list.ixx
 * @author Francesco Corso
 * @date 7/21/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

#include <libassert/assert-macros.hpp>

export module prism.core:syntax.child_list;

import :syntax.node;
import :syntax.list_view;
import :syntax.token;
import :syntax.node_or_token;

namespace prism
{
    export class PRISM_CORE_API ChildSyntaxList : public SyntaxListView<SyntaxNodeOrToken>
    {
      public:
        constexpr ChildSyntaxList() = default;
        constexpr explicit ChildSyntaxList(const SyntaxNode &node) : node_{&node}, count_{count_nodes(node.green())}
        {
        }

        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            return node_ != nullptr ? node_->green().child_count() : 0;
        }

        [[nodiscard]] SyntaxNodeOrToken operator[](std::size_t index) const;

      private:
        struct SlotData
        {
            std::size_t slot_index = 0;
            std::size_t preceding_occupant_slot_count = 0;
            std::size_t position_at_slot_index = 0;
        };

        static std::size_t count_nodes(const GreenNode &node) noexcept;

        static SyntaxNodeOrToken get_item(const SyntaxNode &node, std::size_t index);

        static SyntaxNodeOrToken get_item(const SyntaxNode &node, std::size_t index, SlotData &slot_data);

        static std::size_t occupancy(const GreenNode &node);

        const SyntaxNode *node_ = nullptr;
        std::size_t count_ = 0;
    };
} // namespace prism
