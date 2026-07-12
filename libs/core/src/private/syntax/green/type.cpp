/**
 * @file green_type.cpp
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
module prism.core:syntax.green.type.impl;

import :syntax.green.type;

namespace prism
{

    GreenIdentifierNamedType::GreenIdentifierNamedType(GreenPtr<GreenToken> identifier)
        : GreenSimpleNamedType{SyntaxKind::identifier_name_type_node}, identifier_{std::move(identifier)}
    {
        set_child_count(1);
        adjust_flags_and_width(*identifier_);
    }

    Optional<const GreenNode &> GreenIdentifierNamedType::get_child(std::size_t index) const
    {
        if (index != 0)
            return std::nullopt;

        return *identifier_;
    }
} // namespace prism
