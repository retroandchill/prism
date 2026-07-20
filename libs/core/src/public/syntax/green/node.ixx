/**
 * @file node.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
module;

#include <cassert>

export module prism.core:syntax.green.node;

import :syntax.kind;
import :syntax.flags;
import :memory.ref_counted_ptr;
import :diagnostics.diagnostic_info;
import :util.optional;
import :util.exceptions;

namespace prism
{
    class GreenNode;
    class SyntaxNode;
    class SyntaxLifetime;

    template <typename T>
    concept GreenNodeWrapper = requires(const T &wrapper) {
        {
            wrapper.node()
        } -> std::convertible_to<const GreenNode &>;
    };

    template <typename T>
    concept GreenNodeLike =
        std::same_as<T, GreenNode> || (std::derived_from<T, GreenNode> && requires(const GreenNode &node) {
            {
                T:: instanceof (node)
            } -> std::convertible_to<bool>;
        });

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

        [[nodiscard]] virtual constexpr bool is_token() const noexcept
        {
            return false;
        }

        [[nodiscard]] virtual constexpr bool is_trivia() const noexcept
        {
            return false;
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

        [[nodiscard]] virtual std::uint32_t leading_trivia_width() const;

        [[nodiscard]] inline bool has_leading_trivia() const
        {
            return leading_trivia_width() > 0;
        }

        [[nodiscard]] virtual std::uint32_t trailing_trivia_width() const;

        [[nodiscard]] inline bool has_trailing_trivia() const
        {
            return trailing_trivia_width() > 0;
        }

        [[nodiscard]] Optional<const GreenNode &> first_leaf() const;

        [[nodiscard]] Optional<const GreenNode &> last_leaf() const;

        [[nodiscard]] constexpr std::size_t child_count() const noexcept
        {
            return child_count_;
        }

      protected:
        constexpr void set_child_count(const std::size_t count) noexcept
        {
            child_count_ = count;
        }

      public:
        template <GreenNodeLike T>
        [[nodiscard]] bool is() const
        {
            if constexpr (std::is_same_v<T, GreenNode>)
            {
                return true;
            }
            else
            {
                return T:: instanceof (*this);
            }
        }

        template <GreenNodeLike T>
        [[nodiscard]] Optional<const T &> as() const
        {
            if constexpr (std::is_same_v<T, GreenNode>)
            {
                return *this;
            }
            else
            {
                return T:: instanceof (*this) ? Optional<const T &>{static_cast<const T &>(*this)} : std::nullopt;
            }
        }

        [[nodiscard]] virtual Optional<const GreenNode &> get_child(std::size_t index) const = 0;

        template <std::derived_from<GreenNode> T>
        Optional<const T &> get_child(const std::size_t index) const
        {
            return get_child(index).and_then([](const GreenNode &child) { return child.as<T>(); });
        }

        template <std::derived_from<GreenNode> T>
        const T &get_child_unchecked(const std::size_t index) const
        {
            return static_cast<const T &>(*get_child(index));
        }

        template <std::derived_from<GreenNode> T>
        const T &get_required_child(const std::size_t index) const
        {
            auto child = get_child<T>(index);
            assert(child.has_value());
            return *child;
        }

        [[nodiscard]] std::uint32_t get_child_offset(std::size_t index) const;

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

        [[nodiscard]] RefCountPtr<const SyntaxNode> create_red() const;

        [[nodiscard]] virtual RefCountPtr<const SyntaxNode> create_red(const SyntaxNode *parent,
                                                                       std::uint32_t position) const
        {
            throw UnsupportedOperationException{};
        }

      protected:
        void adjust_flags_and_width(const GreenNode &node);

        template <GreenNodeWrapper T>
        void adjust_flags_and_width(const T &wrapper)
        {
            adjust_flags_and_width(wrapper.node());
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
} // namespace prism
