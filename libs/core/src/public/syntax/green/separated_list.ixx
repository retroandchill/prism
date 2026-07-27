/**
 * @file separated_list.ixx
 * @author Francesco Corso
 * @date 7/13/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:syntax.green.separated_list;

import :syntax.green.token;
import boost;
import libassert;

namespace prism
{
    template <typename T, bool Owning = true>
    class GreenSeparatedList : public SyntaxListView<T>
    {
      public:
        using value_type = T;

        explicit constexpr GreenSeparatedList(GreenSyntaxList<GreenNode, Owning> children) : list_{std::move(children)}
        {
#ifndef NDEBUG
            validate(list_);
#endif
        }

        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            return (list_.size() + 1) / 2;
        }

        [[nodiscard]] constexpr std::size_t separator_count() const noexcept
        {
            return list_.size() / 2;
        }

        [[nodiscard]] constexpr const T &operator[](const std::size_t index) const noexcept
        {
            return static_cast<const T &>(*list_[index * 2]);
        }

        [[nodiscard]] constexpr const GreenToken &separator(const std::size_t index) const noexcept
        {
            return static_cast<const GreenToken &>(*list_[index * 2 + 1]);
        }

        [[nodiscard]] constexpr const GreenSyntaxList<GreenNode, Owning> &with_separators() const noexcept
        {
            return list_;
        }

        [[nodiscard]] constexpr Optional<const GreenNode &> node() const noexcept
        {
            return list_.node();
        }

        [[nodiscard]] constexpr friend bool operator==(const GreenSeparatedList &lhs,
                                                       const GreenSeparatedList &rhs) noexcept
        {
            return lhs.list_ == rhs.list_;
        }

        [[nodiscard]] std::string to_string() const
        {
            std::string result;
            auto writer = StringWriter{result};
            write_to(writer);
            return result;
        }

        void write_to(TextWriter &writer) const
        {
            list_.write_to(writer);
        }

      private:
#ifndef NDEBUG
        static void validate(const GreenSyntaxList<GreenNode, Owning> &list)
        {
            for (auto [i, item] : list | std::views::enumerate)
            {
                if (i % 2 == 0)
                {
                    DEBUG_ASSERT(!item.is_token());
                }
                else
                {
                    DEBUG_ASSERT(item.is_token());
                }
            }
        }
#endif

        GreenSyntaxList<GreenNode, Owning> list_;
    };

    template <typename T>
    class GreenSeparatedListBuilder final
    {
      public:
        const T &add_item(GreenPtr<T> item)
        {
            return static_cast<const T &>(children_.add(std::move(item)));
        }

        const T &add_separator(GreenPtr<GreenToken> item)
        {
            return static_cast<const GreenToken &>(children_.add(std::move(item)));
        }

        void reserve(const std::uint32_t capacity)
        {
            children_.reserve(capacity);
        }

        GreenPtr<GreenSeparatedList<T>> build() const &
        {

            return GreenSeparatedList<T>{children_.build()};
        }

        GreenPtr<GreenSeparatedList<T>> build() &&
        {
            return GreenSeparatedList<T>{std::move(children_).build()};
        }

      private:
        GreenListBuilder<GreenNode> children_;
    };
} // namespace prism
