/**
 * @file syntax_node.ixx
 * @author Francesco Corso
 * @date 7/2/2026
 * @brief
 */
module;

#include "ast/macros.hpp"

export module prism.core.ast:syntax_node;

import std;
import prism.core.source.source_file;
import prism.core.util;

namespace prism
{
    export enum class SyntaxCategory : std::uint8_t
    {
        none,
#define SYNTAX_CATEGORY(name, str) name,
#include "syntax.inl"
    };

    export enum class SyntaxKind : std::uint16_t
    {
#define SYNTAX_KIND(name, str) name,
#include "syntax.inl"
    };

    export enum class SyntaxFlags : std::uint8_t
    {
        none = 0,
        missing = 1 << 0,
        unknown = 1 << 1,
    };

    export struct Modifiers
    {
        bool is_extern = false;
    };

    template <>
    constexpr bool is_flag_enum<SyntaxFlags> = true;

    export class SyntaxNode;

    template <typename>
    struct SyntaxTraits;

    template <SyntaxCategory>
    struct SyntaxCategoryLookup;

    template <SyntaxKind>
    struct SyntaxKindLookup;

    template <typename T>
    concept SyntaxNodeLike = requires {
        {
            SyntaxTraits<T>::category
        } -> std::convertible_to<SyntaxCategory>;
    };

    template <typename T>
    concept ConcreteSyntaxNode = SyntaxNodeLike<T> && requires {
        {
            SyntaxTraits<T>::kind
        } -> std::convertible_to<SyntaxKind>;
    };

    template <SyntaxCategory Category>
        requires(Category != SyntaxCategory::none)
    using SyntaxCategoryType = SyntaxCategoryLookup<Category>::Type;

    template <SyntaxKind Kind>
    using SyntaxNodeType = SyntaxKindLookup<Kind>::Type;

    class SyntaxNode
    {
      protected:
        constexpr SyntaxNode(SyntaxCategory category,
                             SyntaxKind kind,
                             const SourceRange range,
                             const SyntaxFlags flags = SyntaxFlags::none)
            : category_{category}, flags_{flags}, kind_{kind}, range_{range}
        {
        }

        AST_NODE_BOILERPLATE(SyntaxNode)

        // ReSharper disable once CppRedundantAccessSpecifier
      public:
        VALUE_PROPERTY(SyntaxCategory, category)
        VALUE_PROPERTY(SyntaxFlags, flags)
        VALUE_PROPERTY(SyntaxKind, kind)
        VALUE_PROPERTY(SourceRange, range)

        template <SyntaxNodeLike T>
        [[nodiscard]] constexpr bool is() const noexcept
        {
            if constexpr (ConcreteSyntaxNode<T>)
            {
                return kind_ == SyntaxTraits<T>::kind;
            }
            else
            {
                return category_ == SyntaxTraits<T>::category;
            }
        }

        template <SyntaxNodeLike T, typename Self>
            requires std::derived_from<T, std::decay_t<Self>>
        [[nodiscard]] constexpr decltype(auto) cast(this Self &&self)
        {
            if constexpr (std::same_as<std::decay_t<Self>, T>)
            {
                return std::forward<Self>(self);
            }
            else
            {
                if (!self.template is<T>())
                {
                    throw std::bad_cast{};
                }

                return static_cast<ForwardLikeType<Self, T>>(self);
            }
        }

        template <SyntaxNodeLike T, typename Self>
            requires std::derived_from<T, std::decay_t<Self>> && std::is_lvalue_reference_v<Self>
        [[nodiscard]] constexpr auto *try_cast(this Self &&self)
        {
            if constexpr (std::same_as<std::decay_t<Self>, T>)
            {
                return std::addressof(self);
            }
            else
            {
                if (!self.template is<T>())
                {
                    return nullptr;
                }

                return static_cast<std::remove_reference_t<ForwardLikeType<T, Self>> *>(std::addressof(self));
            }
        }

      private:
        template <SyntaxNodeLike T, typename Self>
            requires std::derived_from<T, std::decay_t<Self>>
        [[nodiscard]] constexpr decltype(auto) cast_unchecked(this Self &&self) noexcept
        {
            if constexpr (std::same_as<std::decay_t<Self>, T>)
            {
                return std::forward<Self>(self);
            }
            else
            {
                return static_cast<ForwardLikeType<T, Self>>(self);
            }
        }
    };

    export template <SyntaxNodeLike T>
    constexpr T *get_if(SyntaxNode *node) noexcept
    {
        return node->try_cast<T>(node);
    }

    export template <SyntaxNodeLike T>
    constexpr const T *get_if(const SyntaxNode *node) noexcept
    {
        return node->try_cast<T>(node);
    }
} // namespace prism
