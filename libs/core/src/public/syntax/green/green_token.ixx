/**
 * @file green_token.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core:syntax.green.green_token;

import :syntax.green.green_node;
import :syntax.green.green_trivia_list;

namespace prism
{
    class GreenToken : public GreenNode
    {
      public:
        explicit GreenToken(SyntaxKind kind,
                            RefCountPtr<const GreenTriviaList> leading_trivia = nullptr,
                            RefCountPtr<const GreenTriviaList> trailing_trivia = nullptr);

        GreenToken(SyntaxKind kind,
                   std::uint32_t width,
                   RefCountPtr<const GreenTriviaList> leading_trivia = nullptr,
                   RefCountPtr<const GreenTriviaList> trailing_trivia = nullptr);

        [[nodiscard]] virtual std::string_view text() const;

        [[nodiscard]] constexpr const GreenTriviaList *leading_trivia() const noexcept
        {
            return leading_trivia_.get();
        }

        [[nodiscard]] std::uint32_t leading_trivia_width() const override;

        [[nodiscard]] constexpr const GreenTriviaList *trailing_trivia() const noexcept
        {
            return trailing_trivia_.get();
        }

        [[nodiscard]] std::uint32_t trailing_trivia_width() const override;

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const final;

        [[nodiscard]] virtual RefCountPtr<GreenToken> with_leading_trivia(
            RefCountPtr<const GreenTriviaList> leading_trivia) const;

        [[nodiscard]] virtual RefCountPtr<GreenToken> with_trailing_trivia(
            RefCountPtr<const GreenTriviaList> trailing_trivia) const;

        [[nodiscard]] virtual RefCountPtr<GreenToken> with_leading_and_trailing_trivia(
            RefCountPtr<const GreenTriviaList> leading_trivia,
            RefCountPtr<const GreenTriviaList> trailing_trivia) const;

      protected:
        [[nodiscard]] virtual RefCountPtr<GreenToken> clone_with_trivia(
            RefCountPtr<const GreenTriviaList> leading_trivia,
            RefCountPtr<const GreenTriviaList> trailing_trivia) const;

      private:
        RefCountPtr<const GreenTriviaList> leading_trivia_;
        RefCountPtr<const GreenTriviaList> trailing_trivia_;
    };
} // namespace prism
