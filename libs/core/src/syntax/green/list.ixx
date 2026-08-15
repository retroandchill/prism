/**
 * @file list.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:syntax.green.list;

import :syntax.green.node;
import boost;
import :syntax.list_view;
import :text.string_writer;
import :collections.immutable_small_array;

namespace prism
{
    constexpr std::uint32_t small_list_size = 8;

    using GreenSyntaxVector = boost::container::small_vector<GreenPtr<GreenNode>, small_list_size>;
    using GreenSyntaxArray = ImmutableSmallArray<GreenPtr<GreenNode>, small_list_size>;

    class GreenListNode final : public GreenNode
    {
      public:
        explicit GreenListNode(GreenSyntaxArray children);

        [[nodiscard]] constexpr Optional<const GreenNode &> get_slot(const std::size_t index) const override
        {
            return children_[index].get();
        }

        constexpr void set_slot(const std::size_t index, GreenPtr<GreenNode> slot)
        {
            ASSUME(index < children_.size());
            children_ = children_.set(index, std::move(slot));
        }

        [[nodiscard]] GreenPtr<GreenListNode> with_slot(size_t index, GreenPtr<GreenNode> slot) const;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return node.kind() == SyntaxKind::list;
        }

        [[nodiscard]] SyntaxNode &create_red(SyntaxLifetime &lifetime,
                                             const SyntaxNode *parent,
                                             std::uint32_t position) const override;

        [[nodiscard]] RefCountPtr<GreenNode> clone_internal() const override;

      private:
        friend class GreenListNodeBuilder;

        GreenSyntaxArray children_;
    };

    class GreenListNodeBuilder
    {
      public:
        constexpr GreenListNodeBuilder() = default;

        constexpr const GreenNode &add(GreenPtr<GreenNode> child)
        {
            return *children_.emplace_back(std::move(child));
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, GreenPtr<GreenNode>>
        constexpr void add_range(Range &&range)
        {
            children_.insert(children_.end(), std::ranges::begin(range), std::ranges::end(range));
        }

        constexpr void reserve(const std::uint32_t capacity)
        {
            children_.reserve(capacity);
        }

        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            return children_.size();
        }

        GreenPtr<GreenNode> build() const &
        {
            if (children_.empty())
                return nullptr;

            if (children_.size() == 1)
                return children_[0];

            return make_ref_counted<const GreenListNode>(make_immutable_small_array<small_list_size>(children_));
        }

        GreenPtr<GreenNode> build() &&
        {
            if (children_.empty())
                return nullptr;

            if (children_.size() == 1)
                return std::move(children_[0]);

            return make_ref_counted<GreenListNode>(
                make_immutable_small_array<small_list_size>(children_ | std::views::as_rvalue));
        }

      private:
        GreenSyntaxVector children_;
    };

    template <typename T, bool Owning>
    class GreenSyntaxList : public SyntaxListView<T>
    {
        using Ptr = std::conditional_t<Owning, GreenPtr<GreenNode>, const GreenNode *>;

      public:
        using value_type = T;

        constexpr GreenSyntaxList() = default;

        explicit(false) constexpr GreenSyntaxList(const GreenSyntaxList<T> &other)
            requires(!Owning)
            : children_{other.children_.get()}
        {
        }

        explicit constexpr GreenSyntaxList(const GreenNode *children)
            requires(!Owning)
            : children_{children}
        {
        }

        explicit constexpr GreenSyntaxList(const GreenNode &children)
            requires(!Owning)
            : children_{&children}
        {
        }

        explicit constexpr GreenSyntaxList(GreenPtr<GreenNode> children)
            requires(Owning && !std::is_same_v<T, GreenNode>)
            : children_{std::move(children)}
        {
        }

        explicit(false) constexpr GreenSyntaxList(GreenPtr<T> children)
            requires(Owning)
            : children_{std::move(children)}
        {
        }

        constexpr GreenSyntaxList(const GreenSyntaxList &) = default;
        constexpr GreenSyntaxList(GreenSyntaxList &&) noexcept = default;

        ~GreenSyntaxList() = default;

        constexpr GreenSyntaxList &operator=(const GreenSyntaxList &) = default;
        constexpr GreenSyntaxList &operator=(GreenSyntaxList &&) noexcept = default;

        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            if (children_ == nullptr)
                return 0;

            return children_->template is<GreenListNode>() ? children_->slot_count() : 1;
        }

        [[nodiscard]] const T &operator[](const std::size_t index) const
        {
            if (children_ == nullptr)
                throw std::out_of_range{"Index out of range"};

            auto list = children_->template as<GreenListNode>();
            if (!list.has_value())
            {
                if (index != 0)
                    throw std::out_of_range{"Index out of range"};

                return static_cast<const T &>(*children_);
            }

            auto child = list->get_slot(index);
            if (!child.has_value())
                throw std::out_of_range{"Index out of range"};

            return static_cast<const T &>(*child);
        }

        [[nodiscard]] constexpr Optional<const GreenNode &> node() const noexcept
        {
            if constexpr (Owning)
            {
                return children_.get();
            }
            else
            {
                return children_;
            }
        }

        [[nodiscard]] constexpr GreenPtr<const GreenNode> node() && noexcept
            requires Owning
        {
            return std::move(children_);
        }

        constexpr friend bool operator==(const GreenSyntaxList &lhs, const GreenSyntaxList &rhs) noexcept
        {
            return lhs.children_ == rhs.children_;
        }

        [[nodiscard]] std::string to_string() const
        {
            std::string result;
            auto writer = StringWriter{result};
            write_to(writer);
            return result;
        }

        constexpr void write_to(TextWriter &writer) const
        {
            if (children_ != nullptr)
            {
                children_->write_to(writer);
            }
        }

      private:
        template <typename U, bool OtherOwning>
        friend class GreenSyntaxList;

        Ptr children_ = nullptr;
    };

    template <typename T>
    GreenSyntaxList(GreenPtr<T>) -> GreenSyntaxList<T>;

    template <typename T>
    GreenSyntaxList(const T *) -> GreenSyntaxList<T, false>;

    template <typename T>
    class GreenListBuilder
    {
      public:
        const T &add(GreenPtr<T> child)
        {
            return static_cast<const T &>(inner_.add(std::move(child)));
        }

        void reserve(const std::uint32_t capacity)
        {
            inner_.reserve(capacity);
        }

        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            return inner_.size();
        }

        GreenSyntaxList<T> build() const &
        {
            return GreenSyntaxList<T>{inner_.build()};
        }

        GreenSyntaxList<T> build() &&
        {
            return GreenSyntaxList<T>{std::move(inner_).build()};
        }

      private:
        GreenListNodeBuilder inner_;
    };
} // namespace prism
