/**
 * @file immutable_ordered_set.ixx
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
export module prism.core:collections.immutable_ordered_set;

import :collections.immutable_hash_set;
import :memory.buffer_pool;

namespace prism
{
    template <typename T>
    class ImmutableOrderedSet final
    {
      public:
        using value_type = T;

        constexpr ImmutableOrderedSet() noexcept = default;

      private:
        constexpr ImmutableOrderedSet(ImmutableArray<T> values, ImmutableHashSet<T> set) noexcept
            : values_{values}, set_{set}
        {
        }

      public:
        [[nodiscard]] std::size_t size() const noexcept
        {
            return values_.size();
        }

        [[nodiscard]] constexpr bool empty() const noexcept
        {
            return values_.empty();
        }

        [[nodiscard]] constexpr bool contains(const T &value) const noexcept
        {
            return set_.contains(value);
        }

        [[nodiscard]] constexpr ImmutableOrderedSet add(const T &value)
        {
            auto next_set = set_.add(value);
            if (set_.same_storage(next_set))
                return *this;

            return ImmutableOrderedSet{values_.add(value), std::move(next_set)};
        }

        [[nodiscard]] constexpr ImmutableOrderedSet add(T &&value)
        {
            auto next_set = set_.add(value);
            if (set_.same_storage(next_set))
                return *this;

            return ImmutableOrderedSet{values_.add(std::move(value)), std::move(next_set)};
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_value_t<Range>, T>
        [[nodiscard]] constexpr ImmutableOrderedSet add_range(Range &&range)
        {
            bool changed = false;
            PooledVector<T> values;
            PooledSet<T> explored;

            for (auto &&value : range)
            {
                if (!changed)
                {
                    if (set_.contains(value))
                        continue;

                    values.append_range(values_);
                    explored.insert_range(values_);
                    changed = true;
                }
                else if (explored.contains(value))
                {
                    continue;
                }

                values.append(value);
                explored.insert(std::forward<decltype(value)>(value));
            }

            if (!changed)
                return *this;

            return ImmutableOrderedSet{make_immutable_array(values | std::views::as_rvalue),
                                       make_immutable_hash_set(explored | std::views::as_rvalue)};
        }

        [[nodiscard]] constexpr ImmutableOrderedSet remove(const T &value)
        {
            auto new_set = set_.remove(value);
            if (set_.same_storage(new_set))
                return *this;

            return ImmutableOrderedSet{remove_ordered_value(value), std::move(new_set)};
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_value_t<Range>, T>
        [[nodiscard]] constexpr ImmutableOrderedSet remove_range(Range &&range)
        {
            bool changed = false;
            PooledVector<T> values;

            for (auto &&value : range)
            {
                if (!changed)
                {
                    if (!set_.contains(value))
                        continue;

                    values.append_range(values_);
                    changed = true;
                }

                std::ranges::remove(values, value);
            }

            if (!changed)
                return *this;

            return ImmutableOrderedSet{make_immutable_array(values | std::views::as_rvalue),
                                       make_immutable_hash_set(set_)};
        }

        [[nodiscard]] constexpr auto begin() const noexcept
        {
            return values_.begin();
        }

        [[nodiscard]] constexpr auto end() const noexcept
        {
            return values_.end();
        }

      private:
        [[nodiscard]] constexpr ImmutableArray<T> remove_ordered_value(const T &value) const
        {
            for (std::size_t index = 0; index < values_.size(); ++index)
            {
                if (set_.key_eq()(values_[index], value))
                    return values_.remove_at(index);
            }

            return values_;
        }

        ImmutableArray<T> values_{};
        ImmutableHashSet<T> set_{};
    };
} // namespace prism
