export module prism.core:syntax.green.clauses;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.trivia;
import :syntax.green.separated_list;

namespace prism
{
    class GreenArgument;
    class GreenExpression;
    class GreenParameter;
    class GreenType;

    class GreenInitializer final : public GreenNode
    {
      public:
        GreenInitializer(GreenPtr<GreenToken> equal_sign,
                         GreenPtr<GreenExpression> value,
                         DiagnosticInfoList diagnostics = {});

        ~GreenInitializer() override;

        [[nodiscard]] constexpr const GreenToken &equal_sign() const noexcept
        {
            return *equal_sign_;
        }

        void set_equal_sign(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenExpression &value() const noexcept
        {
            return *value_;
        }

        void set_value(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::initializer;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenInitializer> with_equal_sign(GreenPtr<GreenToken> equal_sign) const;

        [[nodiscard]] GreenPtr<GreenInitializer> with_value(GreenPtr<GreenExpression> value) const;

        [[nodiscard]] GreenPtr<GreenInitializer> update(GreenPtr<GreenToken> equal_sign,
                                                        GreenPtr<GreenExpression> value) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> equal_sign_;
        GreenPtr<GreenExpression> value_;
    };

    template <>
    struct GreenNodeTraits<GreenInitializer>
    {
        static constexpr std::size_t slot_count = 2;

        using ChildTypes = std::tuple<GreenToken, GreenExpression>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenInitializer &node)
        {
            if constexpr (N == 0)
            {
                return node.equal_sign();
            }
            else
            {
                static_assert(N == 1);
                return node.value();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenInitializer>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenInitializer &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_equal_sign(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                node.set_value(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenInitializer>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenInitializer> with(const GreenInitializer &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_equal_sign(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                return node.with_value(std::forward<Arg>(value));
            }
        }
    };

    class GreenTypeSpecifier final : public GreenNode
    {
      public:
        GreenTypeSpecifier(GreenPtr<GreenToken> colon, GreenPtr<GreenType> type, DiagnosticInfoList diagnostics = {});

        ~GreenTypeSpecifier() override;

        [[nodiscard]] constexpr const GreenToken &colon() const noexcept
        {
            return *colon_;
        }

        void set_colon(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenType &type() const noexcept
        {
            return *type_;
        }

        void set_type(GreenPtr<GreenType> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::type_specifier;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenTypeSpecifier> with_colon(GreenPtr<GreenToken> colon) const;

        [[nodiscard]] GreenPtr<GreenTypeSpecifier> with_type(GreenPtr<GreenType> type) const;

        [[nodiscard]] GreenPtr<GreenTypeSpecifier> update(GreenPtr<GreenToken> colon, GreenPtr<GreenType> type) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> colon_;
        GreenPtr<GreenType> type_;
    };

    template <>
    struct GreenNodeTraits<GreenTypeSpecifier>
    {
        static constexpr std::size_t slot_count = 2;

        using ChildTypes = std::tuple<GreenToken, GreenType>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenTypeSpecifier &node)
        {
            if constexpr (N == 0)
            {
                return node.colon();
            }
            else
            {
                static_assert(N == 1);
                return node.type();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenTypeSpecifier>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenTypeSpecifier &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_colon(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                node.set_type(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenTypeSpecifier>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenTypeSpecifier> with(const GreenTypeSpecifier &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_colon(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                return node.with_type(std::forward<Arg>(value));
            }
        }
    };

    class GreenNamedParameter final : public GreenNode
    {
      public:
        GreenNamedParameter(GreenPtr<GreenToken> name, GreenPtr<GreenToken> colon, DiagnosticInfoList diagnostics = {});

        ~GreenNamedParameter() override;

        [[nodiscard]] constexpr const GreenToken &name() const noexcept
        {
            return *name_;
        }

        void set_name(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &colon() const noexcept
        {
            return *colon_;
        }

        void set_colon(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::named_parameter;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenNamedParameter> with_name(GreenPtr<GreenToken> name) const;

        [[nodiscard]] GreenPtr<GreenNamedParameter> with_colon(GreenPtr<GreenToken> colon) const;

        [[nodiscard]] GreenPtr<GreenNamedParameter> update(GreenPtr<GreenToken> name, GreenPtr<GreenToken> colon) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> name_;
        GreenPtr<GreenToken> colon_;
    };

    template <>
    struct GreenNodeTraits<GreenNamedParameter>
    {
        static constexpr std::size_t slot_count = 2;

        using ChildTypes = std::tuple<GreenToken, GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenNamedParameter &node)
        {
            if constexpr (N == 0)
            {
                return node.name();
            }
            else
            {
                static_assert(N == 1);
                return node.colon();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenNamedParameter>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenNamedParameter &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_name(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                node.set_colon(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenNamedParameter>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenNamedParameter> with(const GreenNamedParameter &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_name(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                return node.with_colon(std::forward<Arg>(value));
            }
        }
    };

    class GreenArgumentList final : public GreenNode
    {
      public:
        GreenArgumentList(GreenPtr<GreenToken> open_paren,
                          GreenSeparatedList<GreenArgument> arguments,
                          GreenPtr<GreenToken> close_paren,
                          DiagnosticInfoList diagnostics = {});

        ~GreenArgumentList() override;

        [[nodiscard]] constexpr const GreenToken &open_paren() const noexcept
        {
            return *open_paren_;
        }

        void set_open_paren(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenSeparatedList<GreenArgument> &arguments() const noexcept
        {
            return arguments_;
        }

        void set_arguments(GreenSeparatedList<GreenArgument> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &close_paren() const noexcept
        {
            return *close_paren_;
        }

        void set_close_paren(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::argument_list;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenArgumentList> with_open_paren(GreenPtr<GreenToken> open_paren) const;

        [[nodiscard]] GreenPtr<GreenArgumentList> with_arguments(GreenSeparatedList<GreenArgument> arguments) const;

        [[nodiscard]] GreenPtr<GreenArgumentList> with_close_paren(GreenPtr<GreenToken> close_paren) const;

        [[nodiscard]] GreenPtr<GreenArgumentList> update(GreenPtr<GreenToken> open_paren,
                                                         GreenSeparatedList<GreenArgument> arguments,
                                                         GreenPtr<GreenToken> close_paren) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> open_paren_;
        GreenSeparatedList<GreenArgument> arguments_;
        GreenPtr<GreenToken> close_paren_;
    };

    template <>
    struct GreenNodeTraits<GreenArgumentList>
    {
        static constexpr std::size_t slot_count = 3;

        using ChildTypes = std::tuple<GreenToken, GreenSeparatedList<GreenArgument>, GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenArgumentList &node)
        {
            if constexpr (N == 0)
            {
                return node.open_paren();
            }
            else if constexpr (N == 1)
            {
                return node.arguments();
            }
            else
            {
                static_assert(N == 2);
                return node.close_paren();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenArgumentList>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenArgumentList &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_open_paren(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_arguments(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                node.set_close_paren(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenArgumentList>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenArgumentList> with(const GreenArgumentList &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_open_paren(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_arguments(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                return node.with_close_paren(std::forward<Arg>(value));
            }
        }
    };

    class GreenArgument final : public GreenNode
    {
      public:
        GreenArgument(GreenPtr<GreenNamedParameter> name,
                      GreenPtr<GreenExpression> value,
                      DiagnosticInfoList diagnostics = {});

        ~GreenArgument() override;

        [[nodiscard]] constexpr Optional<const GreenNamedParameter &> name() const noexcept
        {
            return name_.get();
        }

        void set_name(GreenPtr<GreenNamedParameter> value) noexcept;

        [[nodiscard]] constexpr const GreenExpression &value() const noexcept
        {
            return *value_;
        }

        void set_value(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::argument;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenArgument> with_name(GreenPtr<GreenNamedParameter> name) const;

        [[nodiscard]] GreenPtr<GreenArgument> with_value(GreenPtr<GreenExpression> value) const;

        [[nodiscard]] GreenPtr<GreenArgument> update(GreenPtr<GreenNamedParameter> name,
                                                     GreenPtr<GreenExpression> value) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenNamedParameter> name_;
        GreenPtr<GreenExpression> value_;
    };

    template <>
    struct GreenNodeTraits<GreenArgument>
    {
        static constexpr std::size_t slot_count = 2;

        using ChildTypes = std::tuple<GreenNamedParameter, GreenExpression>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenArgument &node)
        {
            if constexpr (N == 0)
            {
                return node.name();
            }
            else
            {
                static_assert(N == 1);
                return node.value();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenArgument>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenArgument &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_name(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                node.set_value(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenArgument>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenArgument> with(const GreenArgument &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_name(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                return node.with_value(std::forward<Arg>(value));
            }
        }
    };

    class GreenParameterList final : public GreenNode
    {
      public:
        GreenParameterList(GreenPtr<GreenToken> open_paren,
                           GreenSeparatedList<GreenParameter> parameters,
                           GreenPtr<GreenToken> close_paren,
                           DiagnosticInfoList diagnostics = {});

        ~GreenParameterList() override;

        [[nodiscard]] constexpr const GreenToken &open_paren() const noexcept
        {
            return *open_paren_;
        }

        void set_open_paren(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenSeparatedList<GreenParameter> &parameters() const noexcept
        {
            return parameters_;
        }

        void set_parameters(GreenSeparatedList<GreenParameter> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &close_paren() const noexcept
        {
            return *close_paren_;
        }

        void set_close_paren(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::parameter_list;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenParameterList> with_open_paren(GreenPtr<GreenToken> open_paren) const;

        [[nodiscard]] GreenPtr<GreenParameterList> with_parameters(GreenSeparatedList<GreenParameter> parameters) const;

        [[nodiscard]] GreenPtr<GreenParameterList> with_close_paren(GreenPtr<GreenToken> close_paren) const;

        [[nodiscard]] GreenPtr<GreenParameterList> update(GreenPtr<GreenToken> open_paren,
                                                          GreenSeparatedList<GreenParameter> parameters,
                                                          GreenPtr<GreenToken> close_paren) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> open_paren_;
        GreenSeparatedList<GreenParameter> parameters_;
        GreenPtr<GreenToken> close_paren_;
    };

    template <>
    struct GreenNodeTraits<GreenParameterList>
    {
        static constexpr std::size_t slot_count = 3;

        using ChildTypes = std::tuple<GreenToken, GreenSeparatedList<GreenParameter>, GreenToken>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenParameterList &node)
        {
            if constexpr (N == 0)
            {
                return node.open_paren();
            }
            else if constexpr (N == 1)
            {
                return node.parameters();
            }
            else
            {
                static_assert(N == 2);
                return node.close_paren();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenParameterList>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenParameterList &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_open_paren(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_parameters(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                node.set_close_paren(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenParameterList>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenParameterList> with(const GreenParameterList &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_open_paren(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_parameters(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 2);
                return node.with_close_paren(std::forward<Arg>(value));
            }
        }
    };

    class GreenParameter final : public GreenNode
    {
      public:
        GreenParameter(GreenPtr<GreenToken> mutable_keyword,
                       GreenPtr<GreenToken> name,
                       GreenPtr<GreenTypeSpecifier> type_specifier,
                       GreenPtr<GreenInitializer> default_value,
                       DiagnosticInfoList diagnostics = {});

        ~GreenParameter() override;

        [[nodiscard]] constexpr Optional<const GreenToken &> mutable_keyword() const noexcept
        {
            return mutable_keyword_.get();
        }

        void set_mutable_keyword(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenToken &name() const noexcept
        {
            return *name_;
        }

        void set_name(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr Optional<const GreenTypeSpecifier &> type_specifier() const noexcept
        {
            return type_specifier_.get();
        }

        void set_type_specifier(GreenPtr<GreenTypeSpecifier> value) noexcept;

        [[nodiscard]] constexpr Optional<const GreenInitializer &> default_value() const noexcept
        {
            return default_value_.get();
        }

        void set_default_value(GreenPtr<GreenInitializer> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::parameter;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenParameter> with_mutable_keyword(GreenPtr<GreenToken> mutable_keyword) const;

        [[nodiscard]] GreenPtr<GreenParameter> with_name(GreenPtr<GreenToken> name) const;

        [[nodiscard]] GreenPtr<GreenParameter> with_type_specifier(GreenPtr<GreenTypeSpecifier> type_specifier) const;

        [[nodiscard]] GreenPtr<GreenParameter> with_default_value(GreenPtr<GreenInitializer> default_value) const;

        [[nodiscard]] GreenPtr<GreenParameter> update(GreenPtr<GreenToken> mutable_keyword,
                                                      GreenPtr<GreenToken> name,
                                                      GreenPtr<GreenTypeSpecifier> type_specifier,
                                                      GreenPtr<GreenInitializer> default_value) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> mutable_keyword_;
        GreenPtr<GreenToken> name_;
        GreenPtr<GreenTypeSpecifier> type_specifier_;
        GreenPtr<GreenInitializer> default_value_;
    };

    template <>
    struct GreenNodeTraits<GreenParameter>
    {
        static constexpr std::size_t slot_count = 4;

        using ChildTypes = std::tuple<GreenToken, GreenToken, GreenTypeSpecifier, GreenInitializer>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenParameter &node)
        {
            if constexpr (N == 0)
            {
                return node.mutable_keyword();
            }
            else if constexpr (N == 1)
            {
                return node.name();
            }
            else if constexpr (N == 2)
            {
                return node.type_specifier();
            }
            else
            {
                static_assert(N == 3);
                return node.default_value();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenParameter>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenParameter &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_mutable_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                node.set_name(std::forward<Arg>(value));
            }
            else if constexpr (N == 2)
            {
                node.set_type_specifier(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 3);
                node.set_default_value(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenParameter>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenParameter> with(const GreenParameter &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_mutable_keyword(std::forward<Arg>(value));
            }
            else if constexpr (N == 1)
            {
                return node.with_name(std::forward<Arg>(value));
            }
            else if constexpr (N == 2)
            {
                return node.with_type_specifier(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 3);
                return node.with_default_value(std::forward<Arg>(value));
            }
        }
    };

    class GreenExpressionBody final : public GreenNode
    {
      public:
        GreenExpressionBody(GreenPtr<GreenToken> arrow,
                            GreenPtr<GreenExpression> expression,
                            DiagnosticInfoList diagnostics = {});

        ~GreenExpressionBody() override;

        [[nodiscard]] constexpr const GreenToken &arrow() const noexcept
        {
            return *arrow_;
        }

        void set_arrow(GreenPtr<GreenToken> value) noexcept;

        [[nodiscard]] constexpr const GreenExpression &expression() const noexcept
        {
            return *expression_;
        }

        void set_expression(GreenPtr<GreenExpression> value) noexcept;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::expression_body;
        }

        [[nodiscard]] Optional<const GreenNode &> get_slot(std::size_t index) const override;

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] GreenPtr<GreenExpressionBody> with_arrow(GreenPtr<GreenToken> arrow) const;

        [[nodiscard]] GreenPtr<GreenExpressionBody> with_expression(GreenPtr<GreenExpression> expression) const;

        [[nodiscard]] GreenPtr<GreenExpressionBody> update(GreenPtr<GreenToken> arrow,
                                                           GreenPtr<GreenExpression> expression) const;
        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        GreenPtr<GreenToken> arrow_;
        GreenPtr<GreenExpression> expression_;
    };

    template <>
    struct GreenNodeTraits<GreenExpressionBody>
    {
        static constexpr std::size_t slot_count = 2;

        using ChildTypes = std::tuple<GreenToken, GreenExpression>;

        template <std::size_t N>
            requires(N < slot_count)
        static constexpr decltype(auto) get(const GreenExpressionBody &node)
        {
            if constexpr (N == 0)
            {
                return node.arrow();
            }
            else
            {
                static_assert(N == 1);
                return node.expression();
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenExpressionBody>> Arg>
            requires(N < slot_count)
        static constexpr void set(GreenExpressionBody &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                node.set_arrow(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                node.set_expression(std::forward<Arg>(value));
            }
        }

        template <std::size_t N, std::convertible_to<GreenSetterParam<N, GreenExpressionBody>> Arg>
            requires(N < slot_count)
        static constexpr GreenPtr<GreenExpressionBody> with(const GreenExpressionBody &node, Arg &&value)
        {
            if constexpr (N == 0)
            {
                return node.with_arrow(std::forward<Arg>(value));
            }
            else
            {
                static_assert(N == 1);
                return node.with_expression(std::forward<Arg>(value));
            }
        }
    };
} // namespace prism
