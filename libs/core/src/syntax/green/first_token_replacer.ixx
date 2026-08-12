/**
 * @file first_token_replacer.ixx
 * @author Francesco Corso
 * @date 7/25/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

export module prism.core:syntax.green.first_token_replacer;

import :syntax.green.node;
import :syntax.green.token;
import :syntax.green.visit;

namespace prism
{
    template <ConcreteGreenNode T, std::size_t N>
    constexpr bool replace_first_token_core(GreenPtr<T> &node, GreenPtr<GreenToken> &&token)
    {
        using ParamType = std::remove_cvref_t<decltype(get_slot<N>(*node))>;
        if constexpr (is_optional_specialization<ParamType>)
        {
            const auto value = get_slot<N>(*node);
            if (!value.has_value())
            {
                return false;
            }

            auto copy = value->shared_from_this();
            if (!replace_first_token_core(copy, std::move(token)))
            {
                return false;
            }

            node = with_slot<N>(*node, std::move(copy));
            return true;
        }
        else if constexpr (GreenNodeWrapper<ParamType>)
        {
            const auto &value = get_slot<N>(*node);
            const auto underlying = value.node();
            if (!underlying.has_value())
            {
                return false;
            }

            auto copy = underlying->shared_from_this();
            if (!replace_first_token_core(copy, std::move(token)))
            {
                return false;
            }

            node = with_slot<N>(*node, GreenSetterParam<N, T>{std::move(copy)});
            return true;
        }
        else
        {
            auto copy = get_slot<N>(*node).shared_from_this();
            if (!replace_first_token_core(copy, std::move(token)))
            {
                return false;
            }

            node = with_slot<N>(*node, std::move(copy));
            return true;
        }
    }

    template <ConcreteGreenNode T, std::size_t... Is>
    constexpr bool replace_first_token_core(GreenPtr<T> &node, GreenPtr<GreenToken> &&token, std::index_sequence<Is...>)
    {
        return (replace_first_token_core<T, Is>(node, std::move(token)) || ...);
    }

    template <std::derived_from<GreenNode> T>
    constexpr bool replace_first_token_core(GreenPtr<T> &node, GreenPtr<GreenToken> &&token)
    {
        if constexpr (std::derived_from<T, GreenToken>)
        {
            node = std::move(token);
            return true;
        }
        else if constexpr (std::same_as<T, GreenTrivia>)
        {
            throw InvalidStateException{"Trivia cannot carry tokens"};
        }
        else if constexpr (std::same_as<T, GreenListNode>)
        {
            for (auto i : std::views::iota(0uz, node->slot_count()) | std::views::reverse)
            {
                if (auto copy = node->get_slot_unchecked(i).shared_from_this();
                    replace_first_token_core(copy, std::move(token)))
                {
                    node = node->with_slot(i, std::move(copy));
                    return true;
                }
            }

            return false;
        }
        else if constexpr (ConcreteGreenNode<T>)
        {
            return replace_first_token_core(node, std::move(token), std::make_index_sequence<green_slot_count<T>>{});
        }
        else
        {
            return visit(*node,
                         [&]<std::derived_from<T> Derived>(const Derived &n)
                         {
                             auto copy = n.shared_from_this();
                             if (!replace_first_token_core(copy, std::move(token)))
                             {
                                 return false;
                             }

                             node = std::move(copy);
                             return true;
                         });
        }
    }

    template <ConcreteGreenNode T, std::size_t N>
    constexpr bool replace_first_token_core(T &node, GreenPtr<GreenToken> &&token)
    {
        using ParamType = std::remove_cvref_t<decltype(get_slot<N>(node))>;
        if constexpr (is_optional_specialization<ParamType>)
        {
            const auto value = get_slot<N>(node);
            if (!value.has_value())
            {
                return false;
            }

            auto copy = value->shared_from_this();
            if (!replace_first_token_core(copy, std::move(token)))
            {
                return false;
            }

            set_slot<N>(*node, std::move(copy));
            return true;
        }
        else if constexpr (GreenNodeWrapper<ParamType>)
        {
            const auto &value = get_slot<N>(*node);
            const auto underlying = value.node();
            if (!underlying.has_value())
            {
                return false;
            }

            auto copy = underlying->shared_from_this();
            if (!replace_first_token_core(copy, std::move(token)))
            {
                return false;
            }

            set_slot<N>(node, ParamType{std::move(copy)});
            return true;
        }
        else
        {
            auto copy = get_slot<N>(*node).shared_from_this();
            if (!replace_first_token_core(copy, std::move(token)))
            {
                return false;
            }

            set_slot<N>(node, std::move(copy));
            return true;
        }
    }

    template <ConcreteGreenNode T, std::size_t... Is>
    constexpr bool replace_first_token_core(T &node, GreenPtr<GreenToken> &&token, std::index_sequence<Is...>)
    {
        static constexpr std::size_t N = sizeof...(Is);
        return (replace_first_token_core<T, Is>(node, std::move(token)) || ...);
    }

    template <std::derived_from<GreenNode> T>
    constexpr bool replace_first_token_core(T &node, GreenPtr<GreenToken> &&token)
    {
        if constexpr (std::derived_from<T, GreenToken>)
        {
            throw InvalidStateException{"Cannot mutably replace a token with itself"};
        }
        else if constexpr (std::same_as<T, GreenTrivia>)
        {
            throw InvalidStateException{"Trivia cannot carry tokens"};
        }
        else if constexpr (std::same_as<T, GreenListNode>)
        {
            for (auto i : std::views::iota(0uz, node->slot_count()) | std::views::reverse)
            {
                if (auto copy = node->get_slot_unchecked(i).shared_from_this();
                    replace_first_token_core(copy, std::move(token)))
                {
                    node.set_slot(i, std::move(copy));
                    return true;
                }
            }

            return false;
        }
        else if constexpr (ConcreteGreenNode<T>)
        {
            return replace_first_token_core(node, std::move(token), std::make_index_sequence<green_slot_count<T>>{});
        }
        else
        {
            return visit(node,
                         [&]<std::derived_from<T> Derived>(const Derived &n)
                         {
                             auto copy = n.shared_from_this();
                             return replace_first_token_core(copy, std::move(token));
                         });
        }
    }

    template <std::derived_from<GreenNode> T>
        requires(!std::same_as<T, GreenToken> && !std::same_as<T, GreenTrivia>)
    constexpr void replace_first_token(T &node, GreenPtr<GreenToken> token)
    {
        replace_first_token_core(node, std::move(token));
    }

    template <std::derived_from<GreenNode> T>
        requires(!std::same_as<T, GreenToken> && !std::same_as<T, GreenTrivia>)
    constexpr GreenPtr<T> replace_first_token(const T &node, GreenPtr<GreenToken> token)
    {
        auto result = node.shared_from_this();
        replace_first_token_core(result, std::move(token));
        return result;
    }
} // namespace prism
