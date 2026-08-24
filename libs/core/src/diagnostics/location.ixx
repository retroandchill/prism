/**
 * @file location.ixx
 * @author Francesco Corso
 * @date 8/3/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core:diagnostics.location;

import :text.text_span;
import :text.source_file;

namespace prism
{
    export class SyntaxToken;
    export class SyntaxNode;
    export class SyntaxTree;
    export class SyntaxReference;

    export class PRISM_CORE_API SourceLocation
    {
      public:
        SourceLocation(const SyntaxTree &syntax_tree, TextSpan span) noexcept;

        explicit SourceLocation(const SyntaxReference &reference) noexcept;

        explicit SourceLocation(const SyntaxNode &node) noexcept;

        explicit SourceLocation(const SyntaxToken &token) noexcept;

        [[nodiscard]] constexpr TextSpan source_span() const noexcept
        {
            return span_;
        }

        [[nodiscard]] constexpr const SyntaxTree &tree() const noexcept
        {
            return *syntax_tree_;
        }

        [[nodiscard]] FileSourcePositionSpan get_position_span() const;

        [[nodiscard]] constexpr friend bool operator==(const SourceLocation &lhs, const SourceLocation &rhs) noexcept
        {
            return lhs.syntax_tree_ == rhs.syntax_tree_ && lhs.span_ == rhs.span_;
        }

      private:
        std::shared_ptr<const SyntaxTree> syntax_tree_;
        TextSpan span_{};
    };

    export struct NoLocation
    {
        constexpr friend bool operator==(NoLocation, NoLocation) noexcept
        {
            return true;
        }
    };

    export constexpr NoLocation no_location{};

    export using Location = std::variant<NoLocation, SourceLocation>;
} // namespace prism

template <>
struct std::hash<prism::SourceLocation>
{
    constexpr std::size_t operator()(const prism::SourceLocation &loc) const noexcept
    {
        return prism::hash_combine(&loc.tree(), loc.source_span());
    }
};

template <>
struct std::hash<prism::NoLocation>
{
    constexpr std::size_t operator()(prism::NoLocation) const noexcept
    {
        // Abitrary number because all structs are equal
        return 0x16487756;
    }
};
