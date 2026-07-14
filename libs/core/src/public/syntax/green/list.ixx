/**
 * @file list.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
export module prism.core:syntax.green.list;

import :syntax.green.node;
import boost;
import :syntax.list_view;

namespace prism
{
    constexpr std::uint32_t small_list_size = 8;

    using GreenSyntaxVector = boost::container::small_vector<GreenPtr<GreenNode>, small_list_size>;

    class GreenListNode final : public GreenNode
    {
      public:
        constexpr GreenListNode() : GreenNode{SyntaxKind::list}
        {
        }

        explicit GreenListNode(GreenSyntaxVector children);

        static const GreenPtr<GreenListNode> &empty();

        [[nodiscard]] constexpr Optional<const GreenNode &> get_child(const std::size_t index) const override
        {
            return children_[index].get();
        }

      private:
        friend class GreenListNodeBuilder;

        GreenSyntaxVector children_;
    };

    class GreenListNodeBuilder
    {
      public:
        constexpr GreenListNodeBuilder() = default;

        constexpr const GreenNode &add(GreenPtr<GreenNode> child)
        {
            return *children_.emplace_back(std::move(child));
        }

        constexpr void reserve(const std::uint32_t capacity)
        {
            children_.reserve(capacity);
        }

        GreenPtr<GreenListNode> build() const &
        {
            if (children_.empty())
                return GreenListNode::empty();

            return make_ref_counted<const GreenListNode>(children_);
        }

        GreenPtr<GreenListNode> build() &&
        {
            if (children_.empty())
                return GreenListNode::empty();

            return make_ref_counted<GreenListNode>(std::move(children_));
        }

      private:
        GreenSyntaxVector children_;
    };

    template <typename T, bool Owning = true>
    class GreenSyntaxList : public SyntaxListView<T>
    {
        using Ptr = std::conditional_t<Owning, GreenPtr<GreenListNode>, const GreenListNode *>;

      public:
        using value_type = T;

        constexpr GreenSyntaxList()
            requires(!Owning)
            : children_{GreenListNode::empty().get()}
        {
        }

        constexpr GreenSyntaxList()
            requires(Owning)
            : children_{GreenListNode::empty()}
        {
        }

        explicit(false) constexpr GreenSyntaxList(const GreenSyntaxList<T> &other)
            requires(!Owning)
            : children_{other.children_.get()}
        {
        }

        explicit constexpr GreenSyntaxList(const GreenListNode &children)
            requires(!Owning)
            : children_{&children}
        {
        }

        explicit constexpr GreenSyntaxList(GreenPtr<GreenListNode> children)
            requires(Owning)
            : children_{std::move(children)}
        {
        }

        constexpr GreenSyntaxList(const GreenSyntaxList &) = default;
        constexpr GreenSyntaxList(GreenSyntaxList &&) noexcept = default;

        ~GreenSyntaxList() = default;

        constexpr GreenSyntaxList &operator=(const GreenSyntaxList &) = default;
        constexpr GreenSyntaxList &operator=(GreenSyntaxList &&) noexcept = default;

        [[nodiscard]] constexpr bool empty() const noexcept
        {
            return size() == 0;
        }

        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            return children_->child_count();
        }

        [[nodiscard]] const T &operator[](const std::size_t index) const
        {
            return static_cast<const T &>(*children_->get_child(index));
        }

        [[nodiscard]] constexpr const GreenListNode &node() const noexcept
        {
            return *children_;
        }

        constexpr friend bool operator==(const GreenSyntaxList &lhs, const GreenSyntaxList &rhs) noexcept
        {
            return lhs.children_ == rhs.children_;
        }

      private:
        template <typename U, bool OtherOwning>
        friend class GreenSyntaxList;

        Ptr children_;
    };

    template <typename T>
    class GreenListBuilder
    {
      public:
        const T &add(GreenPtr<T> child)
        {
            return static_cast<const T &>(inner_.add(std::move(child)));
        }

        void reserve(std::uint32_t capacity)
        {
            inner_.reserve(capacity);
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
