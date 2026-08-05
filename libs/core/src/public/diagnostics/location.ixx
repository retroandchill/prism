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
import :syntax.node_or_token;
import :text.source_file;

namespace prism
{
    class SyntaxTree;

    export class PRISM_CORE_API SourceLocation
    {
      public:
        constexpr SourceLocation(const SyntaxTree &syntax_tree, const TextSpan span) noexcept
            : syntax_tree_{&syntax_tree}, span_{span}
        {
        }

        explicit SourceLocation(const SyntaxNode &node) noexcept;

        explicit SourceLocation(const SyntaxToken &token) noexcept;

        explicit SourceLocation(const SyntaxNodeOrToken &token) noexcept;

        [[nodiscard]] constexpr TextSpan source_span() const noexcept
        {
            return span_;
        }

        [[nodiscard]] constexpr const SyntaxTree &tree() const noexcept
        {
            return *syntax_tree_;
        }

        [[nodiscard]] FileSourcePositionSpan get_position_span() const;

        [[nodiscard]] constexpr friend bool operator==(const SourceLocation &lhs,
                                                       const SourceLocation &rhs) noexcept = default;

      private:
        const SyntaxTree *syntax_tree_ = nullptr;
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
