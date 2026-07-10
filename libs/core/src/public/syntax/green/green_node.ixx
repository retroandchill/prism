/**
 * @file green_node.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
module;

#include <cassert>

export module prism.core:syntax.green.green_node;

import :syntax.kind;
import :syntax.flags;
import :memory.ref_counted_ptr;
import :diagnostics.diagnostic_info;

namespace prism
{
    class GreenNode : IntrusiveRefCounted
    {
      protected:
        constexpr GreenNode(const SyntaxKind kind, const std::uint32_t full_width)
            : kind_{kind}, full_width_{full_width}
        {
        }

      public:
        virtual ~GreenNode() = default;

        [[nodiscard]] constexpr SyntaxKind kind() const noexcept
        {
            return kind_;
        }

        [[nodiscard]] constexpr std::size_t full_width() const noexcept
        {
            return full_width_;
        }

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
            return has_flag(flags_, SyntaxFlags::missing);
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

        [[nodiscard]] const GreenNode *first_leaf() const;

        [[nodiscard]] const GreenNode *last_leaf() const;

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
        [[nodiscard]] virtual const GreenNode *get_child(std::size_t index) const = 0;

        template <std::derived_from<GreenNode> T>
        const T *get_child(const std::size_t index) const
        {
            return dynamic_cast<const T *>(get_child(index));
        }

        template <std::derived_from<GreenNode> T>
        const T *get_child_unchecked(const std::size_t index) const
        {
            return static_cast<const T *>(get_child(index));
        }

        template <std::derived_from<GreenNode> T>
        const T &get_required_child(const std::size_t index) const
        {
            auto child = get_child<T>(index);
            assert(child != nullptr);
            return *child;
        }

        [[nodiscard]] std::uint32_t get_child_offset(std::size_t index) const;

        [[nodiscard]] constexpr const std::vector<RefCountPtr<DiagnosticInfo>> &diagnostics() const noexcept
        {
            return diagnostics_;
        }

        void add_diagnostic(RefCountPtr<DiagnosticInfo> diagnostic);

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, RefCountPtr<DiagnosticInfo>>
        void add_diagnostics(Range &&range)
        {
            diagnostics_.append_range(std::forward<Range>(range));
            if (diagnostics_.size() > 1)
            {
                flags_ |= SyntaxFlags::contains_diagnostics;
            }
        }

      private:
        SyntaxKind kind_;
        SyntaxFlags flags_ = SyntaxFlags::none;
        std::uint32_t full_width_;
        std::size_t child_count_ = 0;
        std::vector<RefCountPtr<DiagnosticInfo>> diagnostics_;
    };
} // namespace prism
