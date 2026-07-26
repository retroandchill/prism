/**
 * @file child_list.ixx
 * @author Francesco Corso
 * @date 7/26/2026
 * @brief
 */
export module prism.core:syntax.green.child_list;

import :syntax.green.node;

namespace prism
{
    class GreenChildIterator
    {
      public:
        using value_type = GreenNode;
        using difference_type = std::ptrdiff_t;

        constexpr GreenChildIterator() = default;

        constexpr explicit GreenChildIterator(const GreenNode &node) : node_{&node}
        {
            move_to_next();
        }

        constexpr const GreenNode &operator*() const
        {
            return *current_child_;
        }

        constexpr const GreenNode *operator->() const
        {
            return current_child_;
        }

        GreenChildIterator &operator++();

        inline GreenChildIterator operator++(int)
        {
            const auto tmp = *this;
            ++*this;
            return tmp;
        }

        GreenChildIterator &operator--();

        inline GreenChildIterator operator--(int)
        {
            const auto tmp = *this;
            --*this;
            return tmp;
        }

        constexpr friend bool operator==(const GreenChildIterator &lhs, const GreenChildIterator &rhs) noexcept
        {
            return lhs.node_ == rhs.node_ && lhs.slot_index == rhs.slot_index && lhs.list_ == rhs.list_ &&
                   lhs.list_index_ == rhs.list_index_;
        }

      private:
        friend class GreenChildList;

        void move_to_next();

        const GreenNode *node_ = nullptr;
        std::uint32_t slot_index = 0;
        std::uint32_t list_index_ = 0;
        const GreenNode *list_ = nullptr;
        const GreenNode *current_child_ = nullptr;
    };

    class GreenChildList final
    {
      public:
        constexpr explicit GreenChildList(const GreenNode &node) : node_{&node}
        {
        }

        [[nodiscard]] std::size_t size() const noexcept;

        constexpr GreenChildIterator begin() const
        {
            return GreenChildIterator{*node_};
        }

        constexpr GreenChildIterator end() const
        {
            GreenChildIterator it;
            it.node_ = node_;
            it.slot_index = node_ != nullptr ? node_->slot_count() : 0;
            return it;
        }

      private:
        std::uint32_t count_nodes() const;

        const GreenNode *node_;
        mutable Optional<std::uint32_t> size_;
    };
} // namespace prism
