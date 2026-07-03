/**
 * @file ast.ixx
 * @author Francesco Corso
 * @date 7/2/2026
 * @brief
 */
export module prism.core.ast;

export import :syntax_node;
export import :type_syntax;
export import :identifier_syntax;
export import :expression_syntax;
export import :statement_syntax;
export import :declaration_syntax;
export import :compilation_unit_syntax;

namespace prism
{
#define SYNTAX_CATEGORY(name, class_name)                                                                              \
    template <>                                                                                                        \
    struct SyntaxTraits<class_name>                                                                                    \
    {                                                                                                                  \
        static constexpr SyntaxCategory category = SyntaxCategory::name;                                               \
    };                                                                                                                 \
    template <>                                                                                                        \
    struct SyntaxCategoryLookup<SyntaxCategory::name>                                                                  \
    {                                                                                                                  \
        using Type = class_name;                                                                                       \
    };
#define SYNTAX_KIND(name, class_name)                                                                                  \
    template <>                                                                                                        \
    struct SyntaxTraits<class_name>                                                                                    \
    {                                                                                                                  \
        static constexpr SyntaxCategory category = SyntaxCategory::none;                                               \
        static constexpr SyntaxKind kind = SyntaxKind::name;                                                           \
    };                                                                                                                 \
    template <>                                                                                                        \
    struct SyntaxKindLookup<SyntaxKind::name>                                                                          \
    {                                                                                                                  \
        using Type = class_name;                                                                                       \
    };
#define DERIVED_SYNTAX_KIND(name, class_name, base)                                                                    \
    template <>                                                                                                        \
    struct SyntaxTraits<class_name>                                                                                    \
    {                                                                                                                  \
        static constexpr SyntaxCategory category = SyntaxTraits<base>::category;                                       \
        static constexpr SyntaxKind kind = SyntaxKind::name;                                                           \
    };                                                                                                                 \
    template <>                                                                                                        \
    struct SyntaxKindLookup<SyntaxKind::name>                                                                          \
    {                                                                                                                  \
        using Type = class_name;                                                                                       \
    };
#include "syntax.inl"
} // namespace prism
