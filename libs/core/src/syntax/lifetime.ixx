/**
 * @file lifetime.ixx
 * @author Francesco Corso
 * @date 7/19/2026
 * @brief
 */
export module prism.core:syntax.lifetime;

import :syntax.green.node;
import :memory.persistent_allocator;
import :util.lazy;
import boost;

namespace prism
{
    class SyntaxTree;

    class SyntaxLifetime final : public std::enable_shared_from_this<SyntaxLifetime>
    {
      public:
        template <typename... Args>
            requires std::constructible_from<SyntaxTree, Args..., SyntaxLifetime &>
        constexpr SyntaxTree &allocate_tree(Args &&...args)
        {
            std::scoped_lock lock{mutex_};
            return allocator_.create<SyntaxTree>(std::forward<Args>(args)..., *this);
        }

        template <std::derived_from<SyntaxNode> Red, std::derived_from<GreenNode> Green>
            requires std::constructible_from<Red, SyntaxLifetime &, const Green &, const SyntaxNode *, std::uint32_t>
        constexpr Red &add(const Green &green, const SyntaxNode *parent = nullptr, std::uint32_t position = 0)
        {
            std::scoped_lock lock{mutex_};
            return allocator_.create<Red>(*this, green, parent, position);
        }

        void add_root(GreenPtr<GreenNode> root) noexcept;

        template <std::derived_from<SyntaxNode> Red = SyntaxNode>
        constexpr std::span<Lazy<const Red *>> allocate_child_slots(const std::size_t count)
        {
            std::scoped_lock lock{mutex_};
            return allocator_.create_array<Lazy<const Red *>>(count);
        }

        [[nodiscard]] constexpr std::size_t num_roots() const noexcept
        {
            return roots_.size();
        }

        [[nodiscard]] constexpr const GreenNode &get_root(const std::size_t index) const
        {
            return *roots_[index];
        }

        void reset() noexcept;

      private:
        static constexpr std::size_t small_buffer_size = 4;

        boost::container::small_vector<GreenPtr<GreenNode>, small_buffer_size> roots_;
        PersistentAllocator allocator_;
        std::recursive_mutex mutex_;
    };
} // namespace prism
