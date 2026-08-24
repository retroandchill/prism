/**
 * @file trivia.ixx
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core:syntax.trivia;

import :syntax.token;
import :syntax.list_view;
import :syntax.node;

namespace prism
{
    export class SyntaxTriviaList;
    export class StructuredTriviaSyntax;

    export class PRISM_CORE_API SyntaxTrivia final
    {
        constexpr SyntaxTrivia(const SyntaxToken &parent, const GreenNode &trivia, const std::uint32_t position)
            : token_{parent}, green_{&trivia}, position_{position}
        {
        }

      public:
        [[nodiscard]] constexpr const SyntaxToken &token() const noexcept
        {
            return token_;
        }

        [[nodiscard]] constexpr Optional<const SyntaxTree &> tree() const
        {
            return token_.tree();
        }

        [[nodiscard]] constexpr TextSpan full_span() const noexcept
        {
            return {.start = position_, .length = green_->full_width()};
        }

        [[nodiscard]] constexpr TextSpan span() const
        {
            return {.start = position_ + green_->leading_trivia_width(), .length = green_->width()};
        }

        [[nodiscard]] constexpr bool contains_diagnostics() const noexcept
        {
            return green_->contains_diagnostics();
        }

      private:
        friend struct SyntaxTriviaInternal;

        SyntaxToken token_;
        const GreenNode *green_;
        std::uint32_t position_;
    };

    class PRISM_CORE_API SyntaxTriviaList final : public SyntaxListView<SyntaxTrivia>
    {
        constexpr explicit SyntaxTriviaList(SyntaxToken parent, const GreenNode *trivia_list)
            : parent_{std::move(parent)}, green_{trivia_list}, position_{SyntaxTokenInternal::get_position(parent)}
        {
        }

        constexpr explicit SyntaxTriviaList(SyntaxToken parent,
                                            const GreenNode *trivia_list,
                                            const std::uint32_t position)
            : parent_{std::move(parent)}, green_{trivia_list}, position_{position}
        {
        }

      public:
        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            return green_.size();
        }

        [[nodiscard]] SyntaxTrivia operator[](std::size_t index) const;

      private:
        friend class SyntaxTriviaInternal;

        SyntaxToken parent_;
        GreenSyntaxList<GreenNode, false> green_;
        std::uint32_t position_;
    };

    class PRISM_CORE_API StructuredTriviaSyntax : public SyntaxNode
    {
      protected:
        constexpr StructuredTriviaSyntax(SyntaxLifetime &lifetime,
                                         const GreenNode &node,
                                         const SyntaxNode *parent,
                                         const std::uint32_t position)
            : SyntaxNode(lifetime, node, parent, position)
        {
        }

        ~StructuredTriviaSyntax() = default;

        [[nodiscard]] constexpr Optional<const SyntaxTrivia &> parent_trivia() const noexcept
        {
            return parent_trivia_;
        }

        static StructuredTriviaSyntax &create(SyntaxLifetime &lifetime, const SyntaxTrivia &trivia);

      private:
        Optional<SyntaxTrivia> parent_trivia_;
    };

    struct SyntaxTriviaInternal
    {
        [[nodiscard]] static constexpr SyntaxTrivia create(SyntaxToken parent,
                                                           const GreenNode &trivia,
                                                           const std::uint32_t position)
        {
            return SyntaxTrivia{std::move(parent), trivia, position};
        }

        [[nodiscard]] static constexpr const GreenNode &get_green(const SyntaxTrivia &trivia) noexcept
        {
            return *trivia.green_;
        }

        [[nodiscard]] static constexpr std::uint32_t get_position(const SyntaxTrivia &trivia) noexcept
        {
            return trivia.position_;
        }

        [[nodiscard]] static constexpr SyntaxTriviaList create_list(const SyntaxToken &parent,
                                                                    const GreenNode *trivia_list)
        {
            return SyntaxTriviaList{parent, trivia_list};
        }

        [[nodiscard]] constexpr static constexpr SyntaxTriviaList create_list(const SyntaxToken &parent,
                                                                              const GreenNode *trivia_list,
                                                                              const std::uint32_t position)
        {
            return SyntaxTriviaList{parent, trivia_list, position};
        }
    };
} // namespace prism
