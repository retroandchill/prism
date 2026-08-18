/**
 * @file binder_factory.ixx
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */

export module prism.core:binder.binder_factory;

import :binder;

namespace prism
{
    class SyntaxTree;

    class BinderFactory final : NonCopyable
    {
      public:
        BinderFactory(const Compilation &compilation, const SyntaxTree &syntax_tree);

        [[nodiscard]] constexpr const SyntaxTree &syntax_tree() const noexcept
        {
            return syntax_tree_;
        }

        [[nodiscard]] const Binder &get_binder(const SyntaxNode &node) const;

      private:
        [[nodiscard]] static Optional<const SyntaxNode &> find_enclosing_designator(const SyntaxNode &node);
        [[nodiscard]] static bool introduces_new_scope(const SyntaxNode &node);
        [[nodiscard]] Lazy<const Binder &> &get_slot(const SyntaxNode &node) const;
        [[nodiscard]] const Binder &create_binder_for_designator(const SyntaxNode &node) const;

        const Compilation &compilation_;
        const SyntaxTree &syntax_tree_;

        mutable std::mutex mutex_;
        mutable std::unordered_map<const SyntaxNode *, Lazy<const Binder &>> binder_cache_{};
        mutable Lazy<const Binder &> root_binder_{};
    };
} // namespace prism
