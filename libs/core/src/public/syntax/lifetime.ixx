/**
 * @file lifetime.ixx
 * @author Francesco Corso
 * @date 7/19/2026
 * @brief
 */
export module prism.core:syntax.lifetime;

import :syntax.green.node;
import :memory.persistent_allocator;
import boost;
import :syntax.node;

namespace prism
{
    class SyntaxLifetime final
    {
      public:
        template <std::derived_from<SyntaxNode> Red, std::derived_from<GreenNode> Green>
            requires std::constructible_from<Red, const Green &, const SyntaxNode *, std::uint32_t>
        constexpr const Red &add(const Green &green, const SyntaxNode *parent = nullptr, std::uint32_t position = 0)
        {
            std::scoped_lock lock{mutex_};
            roots_.emplace_back(green.shared_from_this());
            return allocator_.create<Red>(green, parent, position);
        }

        constexpr std::size_t num_roots() const noexcept
        {
            return roots_.size();
        }

        constexpr const GreenNode &get_root(const std::size_t index) const
        {
            return *roots_[index];
        }

      private:
        static constexpr std::size_t small_buffer_size = 4;

        boost::container::small_vector<GreenPtr<GreenNode>, small_buffer_size> roots_;
        PersistentAllocator allocator_;
        std::mutex mutex_;
    };
} // namespace prism
