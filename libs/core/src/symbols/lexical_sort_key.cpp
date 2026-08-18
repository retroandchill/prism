/**
 * @file lexical_sort_key.cpp
 * @author Francesco Corso
 * @date 8/13/2026
 * @brief
 */
module prism.core:symbols.lexical_sort_key.impl;

import :symbols.lexical_sort_key;
import :semantic.compilation;
import :syntax.tree;
import :syntax.reference;
import :diagnostics.location;

namespace prism
{

    LexicalSortKey::LexicalSortKey(const SyntaxTree &tree, const std::uint32_t position, const Compilation &compilation)
        : LexicalSortKey{CompilationInternal::get_syntax_tree_ordinal(compilation, tree), position}
    {
    }

    LexicalSortKey::LexicalSortKey(const SyntaxReference &reference, const Compilation &compilation)
        : LexicalSortKey{reference.tree(), reference.span().start, compilation}
    {
    }

    LexicalSortKey::LexicalSortKey(const SourceLocation &location, const Compilation &compilation)
        : LexicalSortKey{location.tree(), location.source_span().start, compilation}
    {
    }
} // namespace prism
