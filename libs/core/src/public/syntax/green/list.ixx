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

    using GreenSyntaxVector = boost::container::small_vector<RefCountPtr<const GreenNode>, small_list_size>;

    class GreenListNode final : public GreenNode
    {
      public:
        constexpr GreenListNode() : GreenNode{SyntaxKind::list}
        {
        }

        explicit GreenListNode(GreenSyntaxVector children);

        static const RefCountPtr<const GreenListNode> &empty();

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

        constexpr const GreenNode &add(RefCountPtr<const GreenNode> child)
        {
            return *children_.emplace_back(std::move(child));
        }

        constexpr void reserve(const std::uint32_t capacity)
        {
            children_.reserve(capacity);
        }

        RefCountPtr<const GreenListNode> build() const &
        {
            if (children_.empty())
                return GreenListNode::empty();

            return make_ref_counted<const GreenListNode>(children_);
        }

        RefCountPtr<const GreenListNode> build() &&
        {
            if (children_.empty())
                return GreenListNode::empty();

            return make_ref_counted<GreenListNode>(std::move(children_));
        }

      private:
        GreenSyntaxVector children_;
    };

    template <typename T>
    class GreenSyntaxList : public SyntaxListView<T>
    {
      public:
        using value_type = T;

        constexpr GreenSyntaxList() : children_{GreenListNode::empty()}
        {
        }

        explicit constexpr GreenSyntaxList(RefCountPtr<const GreenListNode> children) : children_{std::move(children)}
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
        RefCountPtr<const GreenListNode> children_;
    };

    template <typename T>
    class GreenListBuilder
    {
      public:
        const T &add(RefCountPtr<const T> child)
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
