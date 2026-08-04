/**
 * @file tree.cpp
 * @author Francesco Corso
 * @date 7/20/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>

module prism.core:syntax.tree.impl;

import :syntax.tree;
import :parser.language_parser;
import :memory.buffer_pool;
import :syntax.trivia;

namespace prism
{
    struct NodeIteration
    {
        Ref<const GreenNode> node;
        std::uint32_t slot_index = 0;
        bool processed_diagnostics = false;
    };

    SyntaxTree::SyntaxTree(std::string path, std::shared_ptr<SourceText> text, GreenPtr<GreenNode> root)
        : path_{std::move(path)}, text_{std::move(text)}, root_{&root->create_red(*lifetime_)}
    {
        lifetime_->add_root(std::move(root));
        root_->tree_.store(this);
    }

    SyntaxTree::SyntaxTree(std::shared_ptr<SourceText> text, GreenPtr<GreenNode> root)
        : SyntaxTree{"", std::move(text), std::move(root)}
    {
    }

    std::unique_ptr<SyntaxTree> SyntaxTree::parse(std::string text)
    {
        return parse(std::make_shared<SourceText>(std::move(text)));
    }

    std::unique_ptr<SyntaxTree> SyntaxTree::parse(std::shared_ptr<SourceText> text)
    {
        LanguageParser parser{text->text()};
        auto root = parser.parse_compilation_unit();
        return std::make_unique<SyntaxTree>(text, std::move(root));
    }

    FileSourcePositionSpan SyntaxTree::get_position_span(TextSpan span) const
    {
        return FileSourcePositionSpan{path_, text_->position_of(span.start), text_->position_of(span.end())};
    }

    std::generator<Diagnostic> SyntaxTree::get_diagnostics() const
    {
        return get_diagnostics(*root_);
    }

    std::generator<Diagnostic> SyntaxTree::get_diagnostics(const SyntaxNode &node) const
    {
        return get_diagnostics(node.green(), node.position());
    }

    std::generator<Diagnostic> SyntaxTree::get_diagnostics(const SyntaxToken &token) const
    {
        return get_diagnostics(*token.green_, token.position_);
    }

    std::generator<Diagnostic> SyntaxTree::get_diagnostics(const SyntaxTrivia &trivia) const
    {
        return get_diagnostics(*trivia.green_, trivia.position_);
    }

    std::generator<Diagnostic> SyntaxTree::get_diagnostics(const SyntaxNodeOrToken &node_or_token) const
    {
        return std::visit(Overload{[this](const SyntaxNode &node) { return get_diagnostics(node); },
                                   [this](const SyntaxToken &token)
                                   {
                                       return get_diagnostics(token);
                                   }},
                          node_or_token);
    }

    std::generator<Diagnostic> SyntaxTree::get_diagnostics(const GreenNode &root, std::uint32_t position) const
    {
        if (!root.contains_diagnostics())
        {
            co_return;
        }

        PooledVector<NodeIteration> stack;
        stack.emplace_back(root);
        const auto full_tree_length = root_->full_span().length;

        auto process_node = [&position, &stack](const GreenNode &node)
        {
            if (node.slot_count() == 0)
            {
                position += node.width();
            }
            else
            {
                for (auto next_slot_index = stack.back().slot_index; next_slot_index < node.slot_count();
                     ++next_slot_index)
                {
                    auto child = node.get_slot(next_slot_index);
                    if (!child.has_value())
                        continue;

                    if (!child->contains_diagnostics())
                    {
                        position += child->full_width();
                        continue;
                    }

                    stack.back().slot_index = next_slot_index;
                    stack.emplace_back(*child);

                    return;
                }
            }
        };

        while (!stack.empty())
        {
            auto &[node, position, processed_diagnostics] = stack.back();
            if (!processed_diagnostics)
            {
                for (auto &info : node->diagnostics())
                {
                    auto &sdi = dynamic_cast<const SyntaxDiagnosticInfo &>(*info);
                    const auto leading_width_to_add = node->is_token() ? 0 : node->leading_trivia_width();

                    const auto span_start = std::min(position + leading_width_to_add + sdi.offset(), full_tree_length);
                    const auto span_end = std::min(span_start + sdi.width(), full_tree_length);
                    co_yield Diagnostic{info, SourceLocation{*this, TextSpan::from_bounds(span_start, span_end)}};
                }
                processed_diagnostics = true;
            }

            process_node(node);
        }
    }
} // namespace prism
