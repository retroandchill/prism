/**
 * @file binding_helpers.ixx
 * @author Francesco Corso
 * @date 8/6/2026
 * @brief
 */

export module prism.core:binder.binding_helpers;

namespace prism
{
    class Compilation;
    class TypeSyntax;
    class TypeSymbol;

    const TypeSymbol &resolve_type(const TypeSyntax &syntax, const Compilation &compilation);
} // namespace prism
