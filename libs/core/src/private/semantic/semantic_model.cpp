/**
 * @file semantic_model.cpp
 * @author Francesco Corso
 * @date 8/4/2026
 * @brief
 */
module prism.core:semantic.semantic_model.impl;

import :semantic.semantic_model;
import :syntax.tree;
import :semantic.compilation;
import :diagnostics.diagnostic;

namespace prism
{
    std::generator<Diagnostic> SemanticModel::get_diagnostics() const
    {
        co_yield std::ranges::elements_of(tree_->get_diagnostics());

        for (auto &diagnostic : compilation_->diagnostics())
        {
            co_yield diagnostic;
        }
    }

    std::generator<Diagnostic> SemanticModel::get_diagnostics(TextSpan span) const
    {
        for (auto diagnostic : get_diagnostics())
        {
            if (auto *source_location = std::get_if<SourceLocation>(&diagnostic.location());
                source_location == nullptr || &source_location->tree() != tree_ ||
                source_location->source_span().overlaps_with(span))
                continue;

            co_yield std::move(diagnostic);
        }
    }
} // namespace prism
