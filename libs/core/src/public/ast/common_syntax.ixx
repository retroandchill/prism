/**
 * @file common_syntax.ixx
 * @author Francesco Corso
 * @date 6/26/2026
 * @brief
 */
export module prism.core.ast.common_syntax;

import std;
import prism.core.lexer.token;
import prism.core.util;
import prism.core.source.source_file;
import prism.core.strings.name;

namespace prism
{
    export enum class SyntaxFlags : std::uint8_t
    {
        none = 0,
        missing = 1 << 0,
    };

    template <>
    constexpr bool is_flag_enum<SyntaxFlags> = true;

    export template <typename T>
    struct SyntaxNode
    {
        SourceRange range;
        SyntaxFlags flags = SyntaxFlags::none;
        T data;

        template <VariantAlternative<T> Alternative>
        [[nodiscard]] constexpr bool is() const
            requires VariantLike<T>
        {
            return std::holds_alternative<Alternative>(data);
        }

        template <VariantAlternative<T> Alternative, typename Self>
        constexpr decltype(auto) get(this Self &&self)
            requires VariantLike<T>
        {
            return std::get<Alternative>(std::forward<Self>(self).data);
        }

        template <VariantAlternative<T> Alternative>
        constexpr auto try_get() &
            requires VariantLike<T>
        {
            return std::get_if<Alternative>(data);
        }

        template <VariantAlternative<T> Alternative>
        constexpr auto try_get() const &
            requires VariantLike<T>
        {
            return std::get_if<Alternative>(data);
        }

        template <typename Self, VariantVisitorFor<ForwardLikeType<T, Self>> Visitor>
        constexpr decltype(auto) visit(this Self &&self, Visitor &&visitor)
            requires VariantLike<T>
        {
            return std::visit(std::forward<Visitor>(visitor), std::forward<Self>(self).data);
        }

        template <typename Self, typename R, VariantVisitorReturnSame<R, ForwardLikeType<T, Self>> Visitor>
        constexpr decltype(auto) visit(this Self &&self, Visitor &&visitor)
            requires VariantLike<T>
        {
            return std::visit(std::forward<Visitor>(visitor), std::forward<Self>(self).data);
        }
    };

    export struct Modifiers
    {
        bool is_extern = false;
    };

    export struct EmptySyntax
    {
    };

    export constexpr EmptySyntax empty_syntax{};

    export struct ErrorSyntax
    {
    };

    export constexpr ErrorSyntax error_syntax{};

    export struct IdentifierSyntax
    {
        Name name;
        SyntaxFlags flags{};
        SourceRange range{};
    };
} // namespace prism
