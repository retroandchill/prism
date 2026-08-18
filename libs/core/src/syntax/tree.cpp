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

    SyntaxTree::SyntaxTree(ConstructTag,
                           std::string path,
                           std::shared_ptr<SourceText> text,
                           GreenPtr<GreenNode> root,
                           SyntaxLifetime &lifetime)
        : path_{std::move(path)}, text_{std::move(text)}, lifetime_{&lifetime}, root_{&root->create_red(*lifetime_)}
    {
        lifetime_->add_root(std::move(root));
        SyntaxNodeInternal::set_tree(*root_, *this);
    }

    SyntaxTree::SyntaxTree(ConstructTag,
                           std::shared_ptr<SourceText> text,
                           GreenPtr<GreenNode> root,
                           SyntaxLifetime &lifetime)
        : SyntaxTree{construct_tag, "", std::move(text), std::move(root), lifetime}
    {
    }

    SyntaxTree::SyntaxTree(ConstructTag, const SyntaxNode &root, SyntaxLifetime &lifetime)
        : lifetime_{&lifetime}, root_{&root}
    {
    }

    std::shared_ptr<const SyntaxTree> SyntaxTree::parse(std::string text)
    {
        return parse(std::make_shared<SourceText>(std::move(text)));
    }

    std::shared_ptr<const SyntaxTree> SyntaxTree::parse(std::shared_ptr<SourceText> text)
    {
        LanguageParser parser{text->text()};
        auto root = parser.parse_compilation_unit();
        auto lifetime = std::make_shared<SyntaxLifetime>();
        auto &tree = lifetime->allocate_tree(construct_tag, text, std::move(root));
        return std::shared_ptr<SyntaxTree>{std::move(lifetime), &tree};
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
        return get_diagnostics(SyntaxNodeInternal::get_green(node), node.position());
    }

    std::generator<Diagnostic> SyntaxTree::get_diagnostics(const SyntaxToken &token) const
    {
        return get_diagnostics(SyntaxTokenInternal::get_green(token), SyntaxTokenInternal::get_position(token));
    }

    std::generator<Diagnostic> SyntaxTree::get_diagnostics(const SyntaxTrivia &trivia) const
    {
        return get_diagnostics(SyntaxTriviaInternal::get_green(trivia), SyntaxTriviaInternal::get_position(trivia));
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
