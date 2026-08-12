/**
 * @file view.ixx
 * @author Francesco Corso
 * @date 7/26/2026
 * @brief
 */
export module prism.core:syntax.green.view;

import :syntax.green.node;
import :memory.buffer_pool;
import :syntax.green.child_list;

namespace prism
{
    class GreenNodeIterator
    {
      public:
        using value_type = GreenNode;
        using difference_type = std::ptrdiff_t;

        constexpr GreenNodeIterator() = default;

        explicit GreenNodeIterator(const GreenNode &node);

        constexpr const GreenNode &operator*() const
        {
            return *current_;
        }

        constexpr const GreenNode *operator->() const
        {
            return current_;
        }

        GreenNodeIterator &operator++();

        inline void operator++(int)
        {
            ++*this;
        }

        constexpr friend bool operator==(const GreenNodeIterator &lhs, std::default_sentinel_t) noexcept
        {
            return lhs.current_ == nullptr;
        }

      private:
        const GreenNode *current_ = nullptr;
        PooledVector<std::pair<GreenChildIterator, GreenChildIterator>> stack_;
    };

    class GreenNodeView
    {
      public:
        explicit constexpr GreenNodeView(const GreenNode &node) : node_{&node}
        {
        }

        inline GreenNodeIterator begin() const
        {
            return GreenNodeIterator{*node_};
        }

        static constexpr std::default_sentinel_t end() noexcept
        {
            return std::default_sentinel;
        }

      private:
        const GreenNode *node_;
    };
} // namespace prism
