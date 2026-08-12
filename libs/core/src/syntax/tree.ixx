/**
 * @file tree.ixx
 * @author Francesco Corso
 * @date 7/12/2026
 * @brief
 */
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

    export class SyntaxTree final : NonCopyable
    {
      public:
        SyntaxTree(std::string path, std::shared_ptr<SourceText> text, GreenPtr<GreenNode> root);

        SyntaxTree(std::shared_ptr<SourceText> text, GreenPtr<GreenNode> root);

        static std::unique_ptr<SyntaxTree> parse(std::string text);
        static std::unique_ptr<SyntaxTree> parse(std::shared_ptr<SourceText> text);

        [[nodiscard]] constexpr const std::string &path() const noexcept
        {
            return path_;
        }

        [[nodiscard]] constexpr const SourceText &text() const noexcept
        {
            return *text_;
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

      private:
        [[nodiscard]] std::generator<Diagnostic> get_diagnostics(const GreenNode &root, std::uint32_t position) const;

        friend SyntaxNode;

        std::string path_;
        std::shared_ptr<SourceText> text_;
        std::unique_ptr<SyntaxLifetime> lifetime_ = std::make_unique<SyntaxLifetime>();
        const SyntaxNode *root_ = nullptr;
    };
} // namespace prism
