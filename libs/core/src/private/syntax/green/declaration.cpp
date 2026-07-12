/**
 * @file declaration.cpp
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
module prism.core:syntax.green.declaration.impl;

import :syntax.green.declaration;
import :syntax.green.type_hint;
import :syntax.green.initializer;

namespace prism
{

    GreenDeclaration::GreenDeclaration(const SyntaxKind kind, GreenPtr<GreenTokenList> modifiers)
        : GreenNode{kind}, modifiers_{std::move(modifiers)}
    {
        if (modifiers_ != nullptr)
        {
            adjust_flags_and_width(*modifiers_);
        }
    }

    GreenVariableDeclaration::GreenVariableDeclaration(GreenPtr<GreenTokenList> modifiers,
                                                       GreenPtr<GreenToken> var_keyword,
                                                       GreenPtr<GreenToken> identifier,
                                                       GreenPtr<GreenTypeHint> type_hint,
                                                       GreenPtr<GreenInitializer> initializer,
                                                       GreenPtr<GreenToken> semicolon)
        : GreenDeclaration{SyntaxKind::variable_declaration_node, std::move(modifiers)},
          var_keyword_{std::move(var_keyword)}, identifier_{std::move(identifier)}, type_hint_{std::move(type_hint)},
          initializer_{std::move(initializer)}, semicolon_{std::move(semicolon)}
    {
        set_child_count(6);
        adjust_flags_and_width(*var_keyword_);
        adjust_flags_and_width(*identifier_);
        if (type_hint_ != nullptr)
        {
            adjust_flags_and_width(*type_hint_);
        }

        if (initializer_ != nullptr)
        {
            adjust_flags_and_width(*initializer_);
        }

        adjust_flags_and_width(*semicolon_);
    }

    Optional<const GreenNode &> GreenVariableDeclaration::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return modifiers();
            case 1:
                return *var_keyword_;
            case 2:
                return *identifier_;
            case 3:
                return type_hint_.get();
            case 4:
                return initializer_.get();
            case 5:
                return *semicolon_;
            default:
                return std::nullopt;
        }
    }
} // namespace prism
