/**
 * @file node.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:syntax.green.node;

import :syntax.kind;
import :syntax.flags;
import :memory.ref_counted_ptr;
import :diagnostics.info;
import :util.optional;
import :util.exceptions;
import libassert;
import :text.writer;

namespace prism
{
    class GreenNode;
    class SyntaxNode;
    class SyntaxLifetime;
    class GreenChildList;
    class GreenNodeView;
    class GreenToken;

    class GreenTrivia;
    template <typename T, bool Owning = true>
    class GreenSyntaxList;
    using GreenTriviaList = GreenSyntaxList<GreenTrivia>;

    template <typename T>
    using GreenPtr = RefCountPtr<const T>;

    template <typename T>
    concept GreenNodeNotNullWrapper = requires(const T &wrapper) {
        {
            wrapper.node()
        } -> std::convertible_to<const GreenNode &>;
    };

    template <typename T>
    concept GreenNodeNullableWrapper = requires(const T &wrapper) {
        {
            wrapper.node()
        } -> std::convertible_to<Optional<const GreenNode &>>;
    };

    template <typename T>
    concept GreenNodeWrapper = GreenNodeNotNullWrapper<T> || GreenNodeNullableWrapper<T>;

    template <typename T>
    concept GreenNodeLike =
        std::same_as<T, GreenNode> || (std::derived_from<T, GreenNode> && requires(const GreenNode &node) {
            {
                T::instance_of(node)
            } -> std::convertible_to<bool>;
        });

    template <std::derived_from<GreenNode>>
    struct GreenNodeTraits;

    template <typename T>
    concept ConcreteGreenNode = requires {
        {
            GreenNodeTraits<T>::slot_count
        } -> std::convertible_to<std::size_t>;
    };

    template <ConcreteGreenNode T>
    constexpr std::size_t green_slot_count = GreenNodeTraits<T>::slot_count;

    template <std::size_t N, std::derived_from<GreenNode> T>
    using GreenChildType = std::tuple_element_t<N, typename GreenNodeTraits<T>::ChildTypes>;

    template <std::size_t N, std::derived_from<GreenNode> T>
    using GreenSetterParam = std::
        conditional_t<GreenNodeWrapper<GreenChildType<N, T>>, GreenChildType<N, T>, GreenPtr<GreenChildType<N, T>>>;

    template <typename T, std::size_t N>
    concept CanGetChild = std::derived_from<T, GreenNode> && requires(const T &node) {
        {
            GreenNodeTraits<T>::template get<N>(node)
        };
    };

    template <typename T, std::size_t N, typename Arg>
    concept CanSetChild = std::derived_from<T, GreenNode> && requires(T &node, Arg &&arg) {
        {
            GreenNodeTraits<T>::template set<N>(node, std::forward<Arg>(arg))
        };
    } && requires(const T &node, Arg &&arg) {
        {
            GreenNodeTraits<T>::template with<N>(node, std::forward<Arg>(arg))
        } -> std::convertible_to<GreenPtr<T>>;
    };

    class GreenNode : public IntrusiveRefCounted
    {
      protected:
        explicit constexpr GreenNode(const SyntaxKind kind,
                                     const std::uint32_t full_width = 0,
                                     DiagnosticInfoList diagnostics = {})
            : kind_{kind}, full_width_{full_width}, diagnostics_{std::move(diagnostics)}
        {
        }

        constexpr GreenNode(const SyntaxKind kind, DiagnosticInfoList diagnostics)
            : kind_{kind}, diagnostics_{std::move(diagnostics)}
        {
        }

      public:
        virtual ~GreenNode() = default;

        [[nodiscard]] constexpr SyntaxKind kind() const noexcept
        {
            return kind_;
        }

        [[nodiscard]] constexpr bool is_list() const noexcept
        {
            return kind_ == SyntaxKind::list;
        }

        [[nodiscard]] constexpr bool is_token() const noexcept
        {
            return prism::is_token(kind_);
        }

        [[nodiscard]] constexpr bool is_trivia() const noexcept
        {
            return prism::is_trivia(kind_);
        }

        [[nodiscard]] constexpr std::uint32_t full_width() const noexcept
        {
            return full_width_;
        }

        [[nodiscard]] virtual std::uint32_t width() const;

        [[nodiscard]] constexpr SyntaxFlags flags() const noexcept
        {
            return flags_;
        }

        constexpr void set_flags(const SyntaxFlags flags) noexcept
        {
            flags_ |= flags;
        }

        constexpr void clear_flags(const SyntaxFlags flags) noexcept
        {
            flags_ &= ~flags;
        }

        [[nodiscard]] constexpr bool is_missing() const noexcept
        {
            return !has_flag(flags_, SyntaxFlags::not_missing);
        }

        [[nodiscard]] constexpr bool contains_diagnostics() const noexcept
        {
            return has_flag(flags_, SyntaxFlags::contains_diagnostics);
        }

        [[nodiscard]] virtual Optional<const GreenNode &> leading_trivia() const;

        [[nodiscard]] virtual std::uint32_t leading_trivia_width() const;

        [[nodiscard]] inline bool has_leading_trivia() const
        {
            return leading_trivia_width() > 0;
        }

        [[nodiscard]] virtual Optional<const GreenNode &> trailing_trivia() const;

        [[nodiscard]] virtual std::uint32_t trailing_trivia_width() const;

        [[nodiscard]] inline bool has_trailing_trivia() const
        {
            return trailing_trivia_width() > 0;
        }

        [[nodiscard]] Optional<const GreenNode &> first_terminal() const;

        [[nodiscard]] Optional<const GreenToken &> first_token() const;

        [[nodiscard]] Optional<const GreenNode &> last_terminal() const;

        [[nodiscard]] Optional<const GreenToken &> last_token() const;

        [[nodiscard]] constexpr std::size_t slot_count() const noexcept
        {
            return child_count_;
        }

      protected:
        constexpr void set_slot_count(const std::size_t count) noexcept
        {
            child_count_ = count;
        }

      public:
        template <GreenNodeLike T, typename Self>
            requires std::derived_from<T, Self>
        [[nodiscard]] bool is(this const Self &self)
        {
            if constexpr (std::is_same_v<T, GreenNode>)
            {
                return true;
            }
            else
            {
                return T::instance_of(self);
            }
        }

        template <GreenNodeLike T, typename Self>
            requires std::derived_from<T, Self>
        [[nodiscard]] Optional<const T &> as(this const Self &self)
        {
            if constexpr (std::is_same_v<T, GreenNode>)
            {
                return self;
            }
            else
            {
                return T::instance_of(self) ? Optional<const T &>{static_cast<const T &>(self)} : std::nullopt;
            }
        }

        [[nodiscard]] virtual Optional<const GreenNode &> get_slot(std::size_t index) const = 0;

        template <std::derived_from<GreenNode> T>
        Optional<const T &> get_slot(const std::size_t index) const
        {
            return get_slot(index).and_then([](const GreenNode &child) { return child.as<T>(); });
        }

        template <std::derived_from<GreenNode> T = GreenNode>
        const T &get_slot_unchecked(const std::size_t index) const
        {
            return static_cast<const T &>(*get_slot(index));
        }

        template <std::derived_from<GreenNode> T = GreenNode>
        const T &get_required_slot(const std::size_t index) const
        {
            auto child = get_slot<T>(index);
            DEBUG_ASSERT(child.has_value());
            return *child;
        }

        [[nodiscard]] std::uint32_t get_slot_offset(std::size_t index) const;

        [[nodiscard]] GreenChildList child_nodes_and_tokens() const;

        [[nodiscard]] GreenNodeView enumerate_nodes() const;

        [[nodiscard]] constexpr const DiagnosticInfoList &diagnostics() const noexcept
        {
            return diagnostics_;
        }

        void add_diagnostic(RefCountPtr<const DiagnosticInfo> diagnostic);

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, RefCountPtr<const DiagnosticInfo>>
        void add_diagnostics(Range &&range)
        {
            diagnostics_.append_range(std::forward<Range>(range));
            if (diagnostics_.size() > 1)
            {
                flags_ |= SyntaxFlags::contains_diagnostics;
            }
        }

        [[nodiscard]] const SyntaxNode &create_red(SyntaxLifetime &lifetime) const
        {
            return create_red(lifetime, nullptr, 0);
        }

        [[nodiscard]] virtual SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                                     const SyntaxNode *parent,
                                                     std::uint32_t position) const = 0;

        template <typename Self>
        [[nodiscard]] RefCountPtr<Self> clone(this const Self &self)
        {
            return static_pointer_cast<Self>(self.clone_internal());
        }

        [[nodiscard]] virtual RefCountPtr<GreenNode> clone_internal() const = 0;

        [[nodiscard]] std::string to_string() const;

        virtual void write_to(TextWriter &writer) const;

      protected:
        void adjust_flags_and_width(const GreenNode &node);

        template <GreenNodeWrapper T>
        void adjust_flags_and_width(const T &wrapper)
        {
            if constexpr (GreenNodeNotNullWrapper<T>)
            {
                adjust_flags_and_width(wrapper.node());
            }
            else
            {
                static_assert(GreenNodeNullableWrapper<T>);
                if (const Optional<const GreenNode &> child = wrapper.node(); child.has_value())
                {
                    adjust_flags_and_width(*child);
                }
            }
        }

      private:
        friend class Lexer;

        constexpr void set_diagnostics(DiagnosticInfoList list)
        {
            diagnostics_ = std::move(list);
        }

        SyntaxKind kind_;
        SyntaxFlags flags_ = SyntaxFlags::none;
        std::uint32_t full_width_ = 0;
        std::size_t child_count_ = 0;
        DiagnosticInfoList diagnostics_;
    };

    template <std::size_t N, CanGetChild<N> Node>
    constexpr decltype(auto) get_slot(const Node &node)
    {
        return GreenNodeTraits<Node>::template get<N>(node);
    }

    template <std::size_t N, typename Arg, CanSetChild<N, Arg> Node>
    constexpr void set_slot(const Node &node, Arg &&arg)
    {
        GreenNodeTraits<Node>::template set<N>(node, std::forward<Arg>(arg));
    }

    template <std::size_t N, typename Arg, CanSetChild<N, Arg> Node>
    constexpr GreenPtr<Node> with_slot(const Node &node, Arg &&arg)
    {
        return GreenNodeTraits<Node>::template with<N>(node, std::forward<Arg>(arg));
    }
} // namespace prism
