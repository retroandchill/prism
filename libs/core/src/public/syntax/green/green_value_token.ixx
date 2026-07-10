/**
 * @file green_value_token.ixx
 * @author Francesco Corso
 * @date 7/10/2026
 * @brief
 */
export module prism.core:syntax.green.green_value_token;

import std;
import :syntax.green.green_token;

namespace prism
{
    template <typename T>
    concept CanGetStringView = requires(const T &t) {
        {
            t.get_string_view(t)
        } -> std::convertible_to<std::string_view>;
    };

    template <typename T>
    concept StandardLiteralData = requires(T t) {
        {
            T::kind
        } -> std::convertible_to<SyntaxKind>;
    };

    template <StandardLiteralData T>
    struct GreenValueTokenData
    {
        T value;
        std::string text;
    };

    template <StandardLiteralData T>
        requires CanGetStringView<T>
    struct GreenValueTokenData<T>
    {
        T value;
    };

    template <StandardLiteralData T>
    class GreenValueToken final : public GreenToken
    {
      public:
        constexpr GreenValueToken(T value,
                                  std::string text,
                                  RefCountPtr<const GreenTriviaList> leading_trivia = nullptr,
                                  RefCountPtr<const GreenTriviaList> trailing_trivia = nullptr)
            requires CanGetStringView<T>
            : GreenToken{T::kind,
                         static_cast<std::uint32_t>(text.length()),
                         std::move(leading_trivia),
                         std::move(trailing_trivia)},
              data_{std::move(value), std::move(text)}
        {
        }

        constexpr GreenValueToken(T value,
                                  RefCountPtr<const GreenTriviaList> leading_trivia = nullptr,
                                  RefCountPtr<const GreenTriviaList> trailing_trivia = nullptr)
            requires !CanGetStringView<T>
            : GreenToken{T::kind,
                         static_cast<std::uint32_t>(value.get_string_view().length()),
                         std::move(leading_trivia),
                         std::move(trailing_trivia)},
        data_{std::move(value)}
        {
        }

        [[nodiscard]] constexpr std::string_view text() const override
        {
            if constexpr (CanGetStringView<T>)
            {
                return data_.text;
            }
            else
            {
                return data_.value.get_string_view();
            }
        }

        [[nodiscard]] constexpr const T &value() const noexcept
        {
            return data_.value;
        }

      private:
        GreenValueTokenData<T> data_;
    };
} // namespace prism
