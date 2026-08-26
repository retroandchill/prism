module prism.core:syntax.green.declarations.impl;

import :syntax.lifetime;
import :syntax.green.declarations;
import :syntax.declarations;
import :syntax.green.clauses;
import :syntax.green.directives;
import :syntax.green.names;
import :syntax.green.statements;

namespace prism
{
    GreenIncompleteDeclaration::GreenIncompleteDeclaration(GreenSyntaxList<GreenToken> modifiers,
                                                           DiagnosticInfoList diagnostics)
        : GreenDeclaration{SyntaxKind::incomplete_declaration, std::move(diagnostics)}, modifiers_{std::move(modifiers)}
    {
        set_slot_count(1);
        adjust_flags_and_width(modifiers_);
    }

    GreenIncompleteDeclaration::~GreenIncompleteDeclaration() = default;

    void GreenIncompleteDeclaration::set_modifiers(GreenSyntaxList<GreenToken> value) noexcept
    {
        modifiers_ = std::move(value);
    }

    Optional<const GreenNode &> GreenIncompleteDeclaration::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return modifiers_.node();
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenIncompleteDeclaration::create_red(SyntaxLifetime &lifetime,
                                                                     const SyntaxNode *parent,
                                                                     std::uint32_t position) const
    {
        return lifetime.add<IncompleteDeclarationSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenDeclaration> GreenIncompleteDeclaration::with_modifiers_core(
        GreenSyntaxList<GreenToken> modifiers) const
    {
        return update(std::move(modifiers));
    }

    GreenPtr<GreenIncompleteDeclaration> GreenIncompleteDeclaration::update(GreenSyntaxList<GreenToken> modifiers) const
    {
        if (modifiers == modifiers_)
            return shared_from_this();

        return make_ref_counted<const GreenIncompleteDeclaration>(std::move(modifiers));
    }

    RefCountPtr<GreenNode> GreenIncompleteDeclaration::clone_internal() const
    {
        return make_ref_counted<GreenIncompleteDeclaration>(modifiers_);
    }

    GreenBlockNamespaceDeclaration::GreenBlockNamespaceDeclaration(GreenSyntaxList<GreenToken> modifiers,
                                                                   GreenPtr<GreenToken> namespace_token,
                                                                   GreenPtr<GreenName> name,
                                                                   GreenPtr<GreenToken> open_brace,
                                                                   GreenSyntaxList<GreenUsingDirective> usings,
                                                                   GreenSyntaxList<GreenDeclaration> members,
                                                                   GreenPtr<GreenToken> close_brace,
                                                                   DiagnosticInfoList diagnostics)
        : GreenNamespaceDeclaration{SyntaxKind::block_namespace_declaration, std::move(diagnostics)},
          modifiers_{std::move(modifiers)}, namespace_token_{std::move(namespace_token)}, name_{std::move(name)},
          open_brace_{std::move(open_brace)}, usings_{std::move(usings)}, members_{std::move(members)},
          close_brace_{std::move(close_brace)}
    {
        set_slot_count(7);
        adjust_flags_and_width(modifiers_);
        adjust_flags_and_width(*namespace_token_);
        adjust_flags_and_width(*name_);
        adjust_flags_and_width(*open_brace_);
        adjust_flags_and_width(usings_);
        adjust_flags_and_width(members_);
        adjust_flags_and_width(*close_brace_);
    }

    GreenBlockNamespaceDeclaration::~GreenBlockNamespaceDeclaration() = default;

    void GreenBlockNamespaceDeclaration::set_modifiers(GreenSyntaxList<GreenToken> value) noexcept
    {
        modifiers_ = std::move(value);
    }

    void GreenBlockNamespaceDeclaration::set_namespace_token(GreenPtr<GreenToken> value) noexcept
    {
        namespace_token_ = std::move(value);
    }

    void GreenBlockNamespaceDeclaration::set_name(GreenPtr<GreenName> value) noexcept
    {
        name_ = std::move(value);
    }

    void GreenBlockNamespaceDeclaration::set_open_brace(GreenPtr<GreenToken> value) noexcept
    {
        open_brace_ = std::move(value);
    }

    void GreenBlockNamespaceDeclaration::set_usings(GreenSyntaxList<GreenUsingDirective> value) noexcept
    {
        usings_ = std::move(value);
    }

    void GreenBlockNamespaceDeclaration::set_members(GreenSyntaxList<GreenDeclaration> value) noexcept
    {
        members_ = std::move(value);
    }

    void GreenBlockNamespaceDeclaration::set_close_brace(GreenPtr<GreenToken> value) noexcept
    {
        close_brace_ = std::move(value);
    }

    Optional<const GreenNode &> GreenBlockNamespaceDeclaration::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return modifiers_.node();
            case 1:
                return *namespace_token_;
            case 2:
                return *name_;
            case 3:
                return *open_brace_;
            case 4:
                return usings_.node();
            case 5:
                return members_.node();
            case 6:
                return *close_brace_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenBlockNamespaceDeclaration::create_red(SyntaxLifetime &lifetime,
                                                                         const SyntaxNode *parent,
                                                                         std::uint32_t position) const
    {
        return lifetime.add<BlockNamespaceDeclarationSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenDeclaration> GreenBlockNamespaceDeclaration::with_modifiers_core(
        GreenSyntaxList<GreenToken> modifiers) const
    {
        return update(std::move(modifiers), namespace_token_, name_, open_brace_, usings_, members_, close_brace_);
    }

    [[nodiscard]] GreenPtr<GreenNamespaceDeclaration> GreenBlockNamespaceDeclaration::with_namespace_token_core(
        GreenPtr<GreenToken> namespace_token) const
    {
        return update(modifiers_, std::move(namespace_token), name_, open_brace_, usings_, members_, close_brace_);
    }

    [[nodiscard]] GreenPtr<GreenNamespaceDeclaration> GreenBlockNamespaceDeclaration::with_name_core(
        GreenPtr<GreenName> name) const
    {
        return update(modifiers_, namespace_token_, std::move(name), open_brace_, usings_, members_, close_brace_);
    }

    [[nodiscard]] GreenPtr<GreenBlockNamespaceDeclaration> GreenBlockNamespaceDeclaration::with_open_brace(
        GreenPtr<GreenToken> open_brace) const
    {
        return update(modifiers_, namespace_token_, name_, std::move(open_brace), usings_, members_, close_brace_);
    }

    [[nodiscard]] GreenPtr<GreenNamespaceDeclaration> GreenBlockNamespaceDeclaration::with_usings_core(
        GreenSyntaxList<GreenUsingDirective> usings) const
    {
        return update(modifiers_, namespace_token_, name_, open_brace_, std::move(usings), members_, close_brace_);
    }

    [[nodiscard]] GreenPtr<GreenNamespaceDeclaration> GreenBlockNamespaceDeclaration::with_members_core(
        GreenSyntaxList<GreenDeclaration> members) const
    {
        return update(modifiers_, namespace_token_, name_, open_brace_, usings_, std::move(members), close_brace_);
    }

    [[nodiscard]] GreenPtr<GreenBlockNamespaceDeclaration> GreenBlockNamespaceDeclaration::with_close_brace(
        GreenPtr<GreenToken> close_brace) const
    {
        return update(modifiers_, namespace_token_, name_, open_brace_, usings_, members_, std::move(close_brace));
    }

    GreenPtr<GreenBlockNamespaceDeclaration> GreenBlockNamespaceDeclaration::update(
        GreenSyntaxList<GreenToken> modifiers,
        GreenPtr<GreenToken> namespace_token,
        GreenPtr<GreenName> name,
        GreenPtr<GreenToken> open_brace,
        GreenSyntaxList<GreenUsingDirective> usings,
        GreenSyntaxList<GreenDeclaration> members,
        GreenPtr<GreenToken> close_brace) const
    {
        if (modifiers == modifiers_ && namespace_token == namespace_token_ && name == name_ &&
            open_brace == open_brace_ && usings == usings_ && members == members_ && close_brace == close_brace_)
            return shared_from_this();

        return make_ref_counted<const GreenBlockNamespaceDeclaration>(std::move(modifiers),
                                                                      std::move(namespace_token),
                                                                      std::move(name),
                                                                      std::move(open_brace),
                                                                      std::move(usings),
                                                                      std::move(members),
                                                                      std::move(close_brace));
    }

    RefCountPtr<GreenNode> GreenBlockNamespaceDeclaration::clone_internal() const
    {
        return make_ref_counted<GreenBlockNamespaceDeclaration>(modifiers_,
                                                                namespace_token_,
                                                                name_,
                                                                open_brace_,
                                                                usings_,
                                                                members_,
                                                                close_brace_);
    }

    GreenFileScopedNamespaceDeclaration::GreenFileScopedNamespaceDeclaration(
        GreenSyntaxList<GreenToken> modifiers,
        GreenPtr<GreenToken> namespace_token,
        GreenPtr<GreenName> name,
        GreenPtr<GreenToken> semicolon,
        GreenSyntaxList<GreenUsingDirective> usings,
        GreenSyntaxList<GreenDeclaration> members,
        DiagnosticInfoList diagnostics)
        : GreenNamespaceDeclaration{SyntaxKind::file_scoped_namespace_declaration, std::move(diagnostics)},
          modifiers_{std::move(modifiers)}, namespace_token_{std::move(namespace_token)}, name_{std::move(name)},
          semicolon_{std::move(semicolon)}, usings_{std::move(usings)}, members_{std::move(members)}
    {
        set_slot_count(6);
        adjust_flags_and_width(modifiers_);
        adjust_flags_and_width(*namespace_token_);
        adjust_flags_and_width(*name_);
        adjust_flags_and_width(*semicolon_);
        adjust_flags_and_width(usings_);
        adjust_flags_and_width(members_);
    }

    GreenFileScopedNamespaceDeclaration::~GreenFileScopedNamespaceDeclaration() = default;

    void GreenFileScopedNamespaceDeclaration::set_modifiers(GreenSyntaxList<GreenToken> value) noexcept
    {
        modifiers_ = std::move(value);
    }

    void GreenFileScopedNamespaceDeclaration::set_namespace_token(GreenPtr<GreenToken> value) noexcept
    {
        namespace_token_ = std::move(value);
    }

    void GreenFileScopedNamespaceDeclaration::set_name(GreenPtr<GreenName> value) noexcept
    {
        name_ = std::move(value);
    }

    void GreenFileScopedNamespaceDeclaration::set_semicolon(GreenPtr<GreenToken> value) noexcept
    {
        semicolon_ = std::move(value);
    }

    void GreenFileScopedNamespaceDeclaration::set_usings(GreenSyntaxList<GreenUsingDirective> value) noexcept
    {
        usings_ = std::move(value);
    }

    void GreenFileScopedNamespaceDeclaration::set_members(GreenSyntaxList<GreenDeclaration> value) noexcept
    {
        members_ = std::move(value);
    }

    Optional<const GreenNode &> GreenFileScopedNamespaceDeclaration::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return modifiers_.node();
            case 1:
                return *namespace_token_;
            case 2:
                return *name_;
            case 3:
                return *semicolon_;
            case 4:
                return usings_.node();
            case 5:
                return members_.node();
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenFileScopedNamespaceDeclaration::create_red(SyntaxLifetime &lifetime,
                                                                              const SyntaxNode *parent,
                                                                              std::uint32_t position) const
    {
        return lifetime.add<FileScopedNamespaceDeclarationSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenDeclaration> GreenFileScopedNamespaceDeclaration::with_modifiers_core(
        GreenSyntaxList<GreenToken> modifiers) const
    {
        return update(std::move(modifiers), namespace_token_, name_, semicolon_, usings_, members_);
    }

    [[nodiscard]] GreenPtr<GreenNamespaceDeclaration> GreenFileScopedNamespaceDeclaration::with_namespace_token_core(
        GreenPtr<GreenToken> namespace_token) const
    {
        return update(modifiers_, std::move(namespace_token), name_, semicolon_, usings_, members_);
    }

    [[nodiscard]] GreenPtr<GreenNamespaceDeclaration> GreenFileScopedNamespaceDeclaration::with_name_core(
        GreenPtr<GreenName> name) const
    {
        return update(modifiers_, namespace_token_, std::move(name), semicolon_, usings_, members_);
    }

    [[nodiscard]] GreenPtr<GreenFileScopedNamespaceDeclaration> GreenFileScopedNamespaceDeclaration::with_semicolon(
        GreenPtr<GreenToken> semicolon) const
    {
        return update(modifiers_, namespace_token_, name_, std::move(semicolon), usings_, members_);
    }

    [[nodiscard]] GreenPtr<GreenNamespaceDeclaration> GreenFileScopedNamespaceDeclaration::with_usings_core(
        GreenSyntaxList<GreenUsingDirective> usings) const
    {
        return update(modifiers_, namespace_token_, name_, semicolon_, std::move(usings), members_);
    }

    [[nodiscard]] GreenPtr<GreenNamespaceDeclaration> GreenFileScopedNamespaceDeclaration::with_members_core(
        GreenSyntaxList<GreenDeclaration> members) const
    {
        return update(modifiers_, namespace_token_, name_, semicolon_, usings_, std::move(members));
    }

    GreenPtr<GreenFileScopedNamespaceDeclaration> GreenFileScopedNamespaceDeclaration::update(
        GreenSyntaxList<GreenToken> modifiers,
        GreenPtr<GreenToken> namespace_token,
        GreenPtr<GreenName> name,
        GreenPtr<GreenToken> semicolon,
        GreenSyntaxList<GreenUsingDirective> usings,
        GreenSyntaxList<GreenDeclaration> members) const
    {
        if (modifiers == modifiers_ && namespace_token == namespace_token_ && name == name_ &&
            semicolon == semicolon_ && usings == usings_ && members == members_)
            return shared_from_this();

        return make_ref_counted<const GreenFileScopedNamespaceDeclaration>(std::move(modifiers),
                                                                           std::move(namespace_token),
                                                                           std::move(name),
                                                                           std::move(semicolon),
                                                                           std::move(usings),
                                                                           std::move(members));
    }

    RefCountPtr<GreenNode> GreenFileScopedNamespaceDeclaration::clone_internal() const
    {
        return make_ref_counted<GreenFileScopedNamespaceDeclaration>(modifiers_,
                                                                     namespace_token_,
                                                                     name_,
                                                                     semicolon_,
                                                                     usings_,
                                                                     members_);
    }

    GreenVariableDeclaration::GreenVariableDeclaration(GreenSyntaxList<GreenToken> modifiers,
                                                       GreenPtr<GreenToken> var_keyword,
                                                       GreenPtr<GreenToken> identifier,
                                                       GreenPtr<GreenTypeSpecifier> type,
                                                       GreenPtr<GreenInitializer> initializer,
                                                       GreenPtr<GreenToken> semicolon,
                                                       DiagnosticInfoList diagnostics)
        : GreenDeclaration{SyntaxKind::variable_declaration, std::move(diagnostics)}, modifiers_{std::move(modifiers)},
          var_keyword_{std::move(var_keyword)}, identifier_{std::move(identifier)}, type_{std::move(type)},
          initializer_{std::move(initializer)}, semicolon_{std::move(semicolon)}
    {
        set_slot_count(6);
        adjust_flags_and_width(modifiers_);
        adjust_flags_and_width(*var_keyword_);
        adjust_flags_and_width(*identifier_);
        if (type_ != nullptr)
            adjust_flags_and_width(*type_);
        if (initializer_ != nullptr)
            adjust_flags_and_width(*initializer_);
        adjust_flags_and_width(*semicolon_);
    }

    GreenVariableDeclaration::~GreenVariableDeclaration() = default;

    void GreenVariableDeclaration::set_modifiers(GreenSyntaxList<GreenToken> value) noexcept
    {
        modifiers_ = std::move(value);
    }

    void GreenVariableDeclaration::set_var_keyword(GreenPtr<GreenToken> value) noexcept
    {
        var_keyword_ = std::move(value);
    }

    void GreenVariableDeclaration::set_identifier(GreenPtr<GreenToken> value) noexcept
    {
        identifier_ = std::move(value);
    }

    void GreenVariableDeclaration::set_type(GreenPtr<GreenTypeSpecifier> value) noexcept
    {
        type_ = std::move(value);
    }

    void GreenVariableDeclaration::set_initializer(GreenPtr<GreenInitializer> value) noexcept
    {
        initializer_ = std::move(value);
    }

    void GreenVariableDeclaration::set_semicolon(GreenPtr<GreenToken> value) noexcept
    {
        semicolon_ = std::move(value);
    }

    Optional<const GreenNode &> GreenVariableDeclaration::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return modifiers_.node();
            case 1:
                return *var_keyword_;
            case 2:
                return *identifier_;
            case 3:
                return type_.get();
            case 4:
                return initializer_.get();
            case 5:
                return *semicolon_;
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenVariableDeclaration::create_red(SyntaxLifetime &lifetime,
                                                                   const SyntaxNode *parent,
                                                                   std::uint32_t position) const
    {
        return lifetime.add<VariableDeclarationSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenDeclaration> GreenVariableDeclaration::with_modifiers_core(
        GreenSyntaxList<GreenToken> modifiers) const
    {
        return update(std::move(modifiers), var_keyword_, identifier_, type_, initializer_, semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenVariableDeclaration> GreenVariableDeclaration::with_var_keyword(
        GreenPtr<GreenToken> var_keyword) const
    {
        return update(modifiers_, std::move(var_keyword), identifier_, type_, initializer_, semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenVariableDeclaration> GreenVariableDeclaration::with_identifier(
        GreenPtr<GreenToken> identifier) const
    {
        return update(modifiers_, var_keyword_, std::move(identifier), type_, initializer_, semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenVariableDeclaration> GreenVariableDeclaration::with_type(
        GreenPtr<GreenTypeSpecifier> type) const
    {
        return update(modifiers_, var_keyword_, identifier_, std::move(type), initializer_, semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenVariableDeclaration> GreenVariableDeclaration::with_initializer(
        GreenPtr<GreenInitializer> initializer) const
    {
        return update(modifiers_, var_keyword_, identifier_, type_, std::move(initializer), semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenVariableDeclaration> GreenVariableDeclaration::with_semicolon(
        GreenPtr<GreenToken> semicolon) const
    {
        return update(modifiers_, var_keyword_, identifier_, type_, initializer_, std::move(semicolon));
    }

    GreenPtr<GreenVariableDeclaration> GreenVariableDeclaration::update(GreenSyntaxList<GreenToken> modifiers,
                                                                        GreenPtr<GreenToken> var_keyword,
                                                                        GreenPtr<GreenToken> identifier,
                                                                        GreenPtr<GreenTypeSpecifier> type,
                                                                        GreenPtr<GreenInitializer> initializer,
                                                                        GreenPtr<GreenToken> semicolon) const
    {
        if (modifiers == modifiers_ && var_keyword == var_keyword_ && identifier == identifier_ && type == type_ &&
            initializer == initializer_ && semicolon == semicolon_)
            return shared_from_this();

        return make_ref_counted<const GreenVariableDeclaration>(std::move(modifiers),
                                                                std::move(var_keyword),
                                                                std::move(identifier),
                                                                std::move(type),
                                                                std::move(initializer),
                                                                std::move(semicolon));
    }

    RefCountPtr<GreenNode> GreenVariableDeclaration::clone_internal() const
    {
        return make_ref_counted<GreenVariableDeclaration>(modifiers_,
                                                          var_keyword_,
                                                          identifier_,
                                                          type_,
                                                          initializer_,
                                                          semicolon_);
    }

    GreenFunctionDeclaration::GreenFunctionDeclaration(GreenSyntaxList<GreenToken> modifiers,
                                                       GreenPtr<GreenToken> func_keyword,
                                                       GreenPtr<GreenToken> identifier,
                                                       GreenPtr<GreenParameterList> parameters,
                                                       GreenPtr<GreenTypeSpecifier> return_type,
                                                       GreenPtr<GreenBlock> body,
                                                       GreenPtr<GreenExpressionBody> expression_body,
                                                       GreenPtr<GreenToken> semicolon,
                                                       DiagnosticInfoList diagnostics)
        : GreenDeclaration{SyntaxKind::function_declaration, std::move(diagnostics)}, modifiers_{std::move(modifiers)},
          func_keyword_{std::move(func_keyword)}, identifier_{std::move(identifier)},
          parameters_{std::move(parameters)}, return_type_{std::move(return_type)}, body_{std::move(body)},
          expression_body_{std::move(expression_body)}, semicolon_{std::move(semicolon)}
    {
        set_slot_count(8);
        adjust_flags_and_width(modifiers_);
        adjust_flags_and_width(*func_keyword_);
        adjust_flags_and_width(*identifier_);
        adjust_flags_and_width(*parameters_);
        if (return_type_ != nullptr)
            adjust_flags_and_width(*return_type_);
        if (body_ != nullptr)
            adjust_flags_and_width(*body_);
        if (expression_body_ != nullptr)
            adjust_flags_and_width(*expression_body_);
        if (semicolon_ != nullptr)
            adjust_flags_and_width(*semicolon_);
    }

    GreenFunctionDeclaration::~GreenFunctionDeclaration() = default;

    void GreenFunctionDeclaration::set_modifiers(GreenSyntaxList<GreenToken> value) noexcept
    {
        modifiers_ = std::move(value);
    }

    void GreenFunctionDeclaration::set_func_keyword(GreenPtr<GreenToken> value) noexcept
    {
        func_keyword_ = std::move(value);
    }

    void GreenFunctionDeclaration::set_identifier(GreenPtr<GreenToken> value) noexcept
    {
        identifier_ = std::move(value);
    }

    void GreenFunctionDeclaration::set_parameters(GreenPtr<GreenParameterList> value) noexcept
    {
        parameters_ = std::move(value);
    }

    void GreenFunctionDeclaration::set_return_type(GreenPtr<GreenTypeSpecifier> value) noexcept
    {
        return_type_ = std::move(value);
    }

    void GreenFunctionDeclaration::set_body(GreenPtr<GreenBlock> value) noexcept
    {
        body_ = std::move(value);
    }

    void GreenFunctionDeclaration::set_expression_body(GreenPtr<GreenExpressionBody> value) noexcept
    {
        expression_body_ = std::move(value);
    }

    void GreenFunctionDeclaration::set_semicolon(GreenPtr<GreenToken> value) noexcept
    {
        semicolon_ = std::move(value);
    }

    Optional<const GreenNode &> GreenFunctionDeclaration::get_slot(std::size_t index) const
    {
        switch (index)
        {
            case 0:
                return modifiers_.node();
            case 1:
                return *func_keyword_;
            case 2:
                return *identifier_;
            case 3:
                return *parameters_;
            case 4:
                return return_type_.get();
            case 5:
                return body_.get();
            case 6:
                return expression_body_.get();
            case 7:
                return semicolon_.get();
            default:
                return std::nullopt;
        }
    }

    [[nodiscard]] SyntaxNode &GreenFunctionDeclaration::create_red(SyntaxLifetime &lifetime,
                                                                   const SyntaxNode *parent,
                                                                   std::uint32_t position) const
    {
        return lifetime.add<FunctionDeclarationSyntax>(*this, parent, position);
    }

    [[nodiscard]] GreenPtr<GreenDeclaration> GreenFunctionDeclaration::with_modifiers_core(
        GreenSyntaxList<GreenToken> modifiers) const
    {
        return update(std::move(modifiers),
                      func_keyword_,
                      identifier_,
                      parameters_,
                      return_type_,
                      body_,
                      expression_body_,
                      semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenFunctionDeclaration> GreenFunctionDeclaration::with_func_keyword(
        GreenPtr<GreenToken> func_keyword) const
    {
        return update(modifiers_,
                      std::move(func_keyword),
                      identifier_,
                      parameters_,
                      return_type_,
                      body_,
                      expression_body_,
                      semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenFunctionDeclaration> GreenFunctionDeclaration::with_identifier(
        GreenPtr<GreenToken> identifier) const
    {
        return update(modifiers_,
                      func_keyword_,
                      std::move(identifier),
                      parameters_,
                      return_type_,
                      body_,
                      expression_body_,
                      semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenFunctionDeclaration> GreenFunctionDeclaration::with_parameters(
        GreenPtr<GreenParameterList> parameters) const
    {
        return update(modifiers_,
                      func_keyword_,
                      identifier_,
                      std::move(parameters),
                      return_type_,
                      body_,
                      expression_body_,
                      semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenFunctionDeclaration> GreenFunctionDeclaration::with_return_type(
        GreenPtr<GreenTypeSpecifier> return_type) const
    {
        return update(modifiers_,
                      func_keyword_,
                      identifier_,
                      parameters_,
                      std::move(return_type),
                      body_,
                      expression_body_,
                      semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenFunctionDeclaration> GreenFunctionDeclaration::with_body(
        GreenPtr<GreenBlock> body) const
    {
        return update(modifiers_,
                      func_keyword_,
                      identifier_,
                      parameters_,
                      return_type_,
                      std::move(body),
                      expression_body_,
                      semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenFunctionDeclaration> GreenFunctionDeclaration::with_expression_body(
        GreenPtr<GreenExpressionBody> expression_body) const
    {
        return update(modifiers_,
                      func_keyword_,
                      identifier_,
                      parameters_,
                      return_type_,
                      body_,
                      std::move(expression_body),
                      semicolon_);
    }

    [[nodiscard]] GreenPtr<GreenFunctionDeclaration> GreenFunctionDeclaration::with_semicolon(
        GreenPtr<GreenToken> semicolon) const
    {
        return update(modifiers_,
                      func_keyword_,
                      identifier_,
                      parameters_,
                      return_type_,
                      body_,
                      expression_body_,
                      std::move(semicolon));
    }

    GreenPtr<GreenFunctionDeclaration> GreenFunctionDeclaration::update(GreenSyntaxList<GreenToken> modifiers,
                                                                        GreenPtr<GreenToken> func_keyword,
                                                                        GreenPtr<GreenToken> identifier,
                                                                        GreenPtr<GreenParameterList> parameters,
                                                                        GreenPtr<GreenTypeSpecifier> return_type,
                                                                        GreenPtr<GreenBlock> body,
                                                                        GreenPtr<GreenExpressionBody> expression_body,
                                                                        GreenPtr<GreenToken> semicolon) const
    {
        if (modifiers == modifiers_ && func_keyword == func_keyword_ && identifier == identifier_ &&
            parameters == parameters_ && return_type == return_type_ && body == body_ &&
            expression_body == expression_body_ && semicolon == semicolon_)
            return shared_from_this();

        return make_ref_counted<const GreenFunctionDeclaration>(std::move(modifiers),
                                                                std::move(func_keyword),
                                                                std::move(identifier),
                                                                std::move(parameters),
                                                                std::move(return_type),
                                                                std::move(body),
                                                                std::move(expression_body),
                                                                std::move(semicolon));
    }

    RefCountPtr<GreenNode> GreenFunctionDeclaration::clone_internal() const
    {
        return make_ref_counted<GreenFunctionDeclaration>(modifiers_,
                                                          func_keyword_,
                                                          identifier_,
                                                          parameters_,
                                                          return_type_,
                                                          body_,
                                                          expression_body_,
                                                          semicolon_);
    }
} // namespace prism
