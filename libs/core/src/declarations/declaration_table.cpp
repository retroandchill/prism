/**
 * @file declaration_table.cpp
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
module prism.core:declarations.declaration_table.impl;

import :declarations.declaration_table;

namespace prism
{

    const MergedNamespaceDeclaration &DeclarationTable::get_merged_root(const Compilation &compilation) const
    {
        return *merged_root_.get_or_compute([&] { return calculate_merged_root(compilation); });
    }

    RefCountPtr<const MergedNamespaceDeclaration> DeclarationTable::calculate_merged_root(
        const Compilation &compilation) const
    {
        throw NotImplementedException{};
    }
} // namespace prism
