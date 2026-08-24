/**
 * @file tree.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core:syntax.tree;

import :text.source_file;
import :syntax.green.node;
import :util.noncopyable;
import :syntax.node;
import :syntax.node_or_token;
import :memory.persistent_allocator;
import :syntax.lifetime;
import :diagnostics.diagnostic;

namespace prism
{
    class SyntaxTrivia;

    export class PRISM_CORE_API SyntaxTree final : NonCopyable
    {
        struct ConstructTag
        {
        };

        static constexpr ConstructTag construct_tag;

      public:
        SyntaxTree(ConstructTag,
                   std::string path,
                   std::shared_ptr<SourceText> text,
                   GreenPtr<GreenNode> root,
                   SyntaxLifetime &lifetime);

        SyntaxTree(ConstructTag, std::shared_ptr<SourceText> text, GreenPtr<GreenNode> root, SyntaxLifetime &lifetime);

        SyntaxTree(ConstructTag, const SyntaxNode &root, SyntaxLifetime &lifetime);

        static std::shared_ptr<const SyntaxTree> parse(std::string text);
        static std::shared_ptr<const SyntaxTree> parse(std::shared_ptr<SourceText> text);

        [[nodiscard]] constexpr const std::string &path() const noexcept
        {
            return path_;
        }

        [[nodiscard]] constexpr Optional<const SourceText &> text() const noexcept
        {
            return text_.get();
        }

        [[nodiscard]] constexpr const SyntaxNode &root() const noexcept
        {
            return *root_;
        }

        [[nodiscard]] FileSourcePositionSpan get_position_span(TextSpan span) const;

        [[nodiscard]] std::generator<Diagnostic> get_diagnostics() const;

        [[nodiscard]] std::generator<Diagnostic> get_diagnostics(const SyntaxNode &node) const;

        [[nodiscard]] std::generator<Diagnostic> get_diagnostics(const SyntaxToken &token) const;

        [[nodiscard]] std::generator<Diagnostic> get_diagnostics(const SyntaxTrivia &trivia) const;

        [[nodiscard]] std::generator<Diagnostic> get_diagnostics(const SyntaxNodeOrToken &node_or_token) const;

        template <typename Self>
        [[nodiscard]] std::shared_ptr<Self> shared_from_this(this Self &self) noexcept
        {
            return std::shared_ptr<Self>{self.lifetime_->shared_from_this(), std::addressof(self)};
        }

      private:
        friend struct SyntaxTreeInternal;

        [[nodiscard]] std::generator<Diagnostic> get_diagnostics(const GreenNode &root, std::uint32_t position) const;

        std::string path_;
        std::shared_ptr<SourceText> text_{};
        SyntaxLifetime *lifetime_;
        const SyntaxNode *root_ = nullptr;
    };

    struct SyntaxTreeInternal
    {
        static constexpr SyntaxTree::ConstructTag construct_tag;
    };
} // namespace prism
