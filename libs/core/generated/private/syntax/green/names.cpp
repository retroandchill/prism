module prism.core:syntax.green.names.impl;

import :syntax.lifetime;
import :syntax.green.names;
import :syntax.names;

namespace prism
{
    GreenSimpleName::GreenSimpleName(GreenPtr<GreenToken> identifier, DiagnosticInfoList diagnostics)
        : GreenName{SyntaxKind::simple_name, std::move(diagnostics)}, identifier_{std::move(identifier)}
    {
        set_slot_count(1);
        adjust_flags_and_width(*identifier_);
    }

    GreenSimpleName::~GreenSimpleName() = default;

    void GreenSimpleName::set_identifier(GreenPtr<GreenToken> value) noexcept
    {
        identifier_ = std::move(value);
    }

    Optional<const GreenNode &> GreenSimpleName::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *identifier_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenSimpleName::create_red(SyntaxLifetime &lifetime,
                                                          const SyntaxNode *parent,
                                                          std::uint32_t position) const
    {
        return lifetime.add<SimpleNameSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenSimpleName> GreenSimpleName::with_identifier(GreenPtr<GreenToken> identifier) const
    {
        return update(std::move(identifier));
    }

    GreenPtr<GreenSimpleName> GreenSimpleName::update(GreenPtr<GreenToken> identifier) const
    {
        if (identifier == identifier_)
            return shared_from_this();

        return make_ref_counted<const GreenSimpleName>(std::move(identifier));
    }

    RefCountPtr<GreenNode> GreenSimpleName::clone_internal() const
    {
        return make_ref_counted<GreenSimpleName>(identifier_);
    }

    GreenQualifiedName::GreenQualifiedName(GreenPtr<GreenName> left,
                                           GreenPtr<GreenToken> separator,
                                           GreenPtr<GreenSimpleName> right,
                                           DiagnosticInfoList diagnostics)
        : GreenName{SyntaxKind::qualified_name, std::move(diagnostics)}, left_{std::move(left)},
          separator_{std::move(separator)}, right_{std::move(right)}
    {
        set_slot_count(3);
        adjust_flags_and_width(*left_);
        adjust_flags_and_width(*separator_);
        adjust_flags_and_width(*right_);
    }

    GreenQualifiedName::~GreenQualifiedName() = default;

    void GreenQualifiedName::set_left(GreenPtr<GreenName> value) noexcept
    {
        left_ = std::move(value);
    }

    void GreenQualifiedName::set_separator(GreenPtr<GreenToken> value) noexcept
    {
        separator_ = std::move(value);
    }

    void GreenQualifiedName::set_right(GreenPtr<GreenSimpleName> value) noexcept
    {
        right_ = std::move(value);
    }

    Optional<const GreenNode &> GreenQualifiedName::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return *left_;
            case 1:
                return *separator_;
            case 2:
                return *right_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenQualifiedName::create_red(SyntaxLifetime &lifetime,
                                                             const SyntaxNode *parent,
                                                             std::uint32_t position) const
    {
        return lifetime.add<QualifiedNameSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenQualifiedName> GreenQualifiedName::with_left(GreenPtr<GreenName> left) const
    {
        return update(std::move(left), separator_, right_);
    }

    [[nodiscard]] GreenPtr<GreenQualifiedName> GreenQualifiedName::with_separator(GreenPtr<GreenToken> separator) const
    {
        return update(left_, std::move(separator), right_);
    }

    [[nodiscard]] GreenPtr<GreenQualifiedName> GreenQualifiedName::with_right(GreenPtr<GreenSimpleName> right) const
    {
        return update(left_, separator_, std::move(right));
    }

    GreenPtr<GreenQualifiedName> GreenQualifiedName::update(GreenPtr<GreenName> left,
                                                            GreenPtr<GreenToken> separator,
                                                            GreenPtr<GreenSimpleName> right) const
    {
        if (left == left_ && separator == separator_ && right == right_)
            return shared_from_this();

        return make_ref_counted<const GreenQualifiedName>(std::move(left), std::move(separator), std::move(right));
    }

    RefCountPtr<GreenNode> GreenQualifiedName::clone_internal() const
    {
        return make_ref_counted<GreenQualifiedName>(left_, separator_, right_);
    }
} // namespace prism
