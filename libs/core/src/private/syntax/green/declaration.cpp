module prism.core:syntax.green.declaration.impl;

import :syntax.green.declaration;
import :syntax.green.clauses;

namespace prism
{
    GreenDeclaration::~GreenDeclaration() = default;

    GreenVariableDeclaration::GreenVariableDeclaration(const SyntaxKind kind,
                                                       GreenSyntaxList<GreenToken> modifiers,
                                                       GreenPtr<GreenToken> var_keyword,
                                                       GreenPtr<GreenToken> mut_keyword,
                                                       GreenPtr<GreenToken> identifier,
                                                       GreenPtr<GreenTypeHint> type,
                                                       GreenPtr<GreenInitializer> initializer,
                                                       GreenPtr<GreenToken> semicolon,
                                                       DiagnosticInfoList diagnostics)
        : GreenDeclaration{kind, std::move(diagnostics)}, modifiers_{std::move(modifiers)},
          var_keyword_{std::move(var_keyword)}, mut_keyword_{std::move(mut_keyword)},
          identifier_{std::move(identifier)}, type_{std::move(type)}, initializer_{std::move(initializer)},
          semicolon_{std::move(semicolon)}
    {
        set_child_count(7);
        adjust_flags_and_width(modifiers_);
        adjust_flags_and_width(*var_keyword_);
        if (mut_keyword_ != nullptr)
            adjust_flags_and_width(*mut_keyword_);
        adjust_flags_and_width(*identifier_);
        adjust_flags_and_width(*type_);
        adjust_flags_and_width(*initializer_);
        adjust_flags_and_width(*semicolon_);
    }
    GreenVariableDeclaration::~GreenVariableDeclaration() = default;

    Optional<const GreenNode &> GreenVariableDeclaration::get_child(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return modifiers_.node();
            case 1:
                return *var_keyword_;
            case 2:
                return mut_keyword_.get();
            case 3:
                return *identifier_;
            case 4:
                return *type_;
            case 5:
                return *initializer_;
            case 6:
                return *semicolon_;
            default:
                return std::nullopt;
        }
    }

} // namespace prism
