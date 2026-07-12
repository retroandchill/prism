/**
 * @file type_hint.cpp
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
module prism.core:syntax.green.type_hint.impl;

import :syntax.green.type_hint;
import :syntax.green.type;

namespace prism
{

    GreenTypeHint::GreenTypeHint(GreenPtr<GreenToken> colon, GreenPtr<GreenType> type)
        : GreenNode{SyntaxKind::initializer_node}, colon_{std::move(colon)}, type_{std::move(type)}
    {
        set_child_count(2);
        adjust_flags_and_width(*colon_);
        adjust_flags_and_width(*type_);
    }
    Optional<const GreenNode &> GreenTypeHint::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *colon_;
            case 1:
                return *type_;
            default:
                return std::nullopt;
        }
    }
} // namespace prism
