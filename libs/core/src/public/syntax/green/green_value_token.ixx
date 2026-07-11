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
    concept StandardLiteralData = std::copyable<T> && requires(T t) {
        {
            T::kind
        } -> std::convertible_to<SyntaxKind>;
    };

    struct TrivialBase
    {
    };

    template <StandardLiteralData T>
    using TokenDataBase =
        std::conditional_t<std::is_trivially_copy_constructible_v<T>, TrivialBase, IntrusiveRefCounted>;

    template <StandardLiteralData T>
    struct GreenValueTokenData : TokenDataBase<T>
    {
        T value;
        std::string text;

        constexpr GreenValueTokenData(T value, std::string text) : value{std::move(value)}, text{std::move(text)}
        {
        }

        [[nodiscard]] constexpr std::uint32_t width() const noexcept
        {
            return static_cast<std::uint32_t>(text.length());
        }
    };

    template <StandardLiteralData T>
        requires CanGetStringView<T>
    struct GreenValueTokenData<T> : TokenDataBase<T>
    {
        T value;

        explicit constexpr GreenValueTokenData(T value) : value{std::move(value)}
        {
        }

        [[nodiscard]] constexpr std::uint32_t width() const noexcept
        {
            return value.get_string_view().length();
        }
    };

    template <StandardLiteralData T>
    class GreenValueToken final : public GreenToken
    {
        using RawData = GreenValueTokenData<T>;
        using Data = std::conditional_t<std::is_trivially_copy_constructible_v<T>, RawData, RefCountPtr<const RawData>>;

        constexpr GreenValueToken(Data data,
                                  RefCountPtr<const GreenTriviaList> leading_trivia,
                                  RefCountPtr<const GreenTriviaList> trailing_trivia)
            : GreenToken{T::kind, get_width(data), std::move(leading_trivia), std::move(trailing_trivia)},
              data_{std::move(data)}
        {
        }

      public:
        constexpr GreenValueToken(T value,
                                  std::string text,
                                  RefCountPtr<const GreenTriviaList> leading_trivia = nullptr,
                                  RefCountPtr<const GreenTriviaList> trailing_trivia = nullptr)
            requires !CanGetStringView<T>
            : GreenValueToken{make_data(std::move(value), std::move(text)),
                              std::move(leading_trivia),
                              std::move(trailing_trivia)}
        {
        }

        explicit constexpr GreenValueToken(T value,
                                           RefCountPtr<const GreenTriviaList> leading_trivia = nullptr,
                                           RefCountPtr<const GreenTriviaList> trailing_trivia = nullptr)
            requires CanGetStringView<T>
            : GreenValueToken{make_data(std::move(value)), std::move(leading_trivia), std::move(trailing_trivia)}
        {
        }

        [[nodiscard]] constexpr std::string_view text() const override
        {
            if constexpr (CanGetStringView<T>)
            {
                return data().text;
            }
            else
            {
                return data().value.get_string_view();
            }
        }

        [[nodiscard]] constexpr const T &value() const noexcept
        {
            return data().value;
        }

      protected:
        GreenPtr<GreenToken> clone_with_trivia(GreenPtr<GreenTriviaList> leading_trivia,
                                               GreenPtr<GreenTriviaList> trailing_trivia) const override
        {
            return make_ref_counted<GreenValueToken>(data_, std::move(leading_trivia), std::move(trailing_trivia));
        }

      private:
        constexpr const RawData &data() const noexcept
        {
            if constexpr (std::is_trivially_copy_constructible_v<T>)
            {
                return data_;
            }
            else
            {
                return *data_;
            }
        }

        static constexpr Data make_data(T value)
            requires CanGetStringView<T>
        {
            if constexpr (std::is_trivially_copy_constructible_v<T>)
            {
                return Data{std::move(value)};
            }
            else
            {
                return make_ref_counted<RawData>(std::move(value));
            }
        }

        static constexpr Data make_data(T value, std::string text)
            requires !CanGetStringView<T>
        {
            if constexpr (std::is_trivially_copy_constructible_v<T>)
            {
                return Data{std::move(value), std::move(text)};
            }
            else
            {
                return make_ref_counted<RawData>(std::move(value), std::move(text));
            }
        }

        static constexpr std::uint32_t get_width(const Data &data) noexcept
        {
            if constexpr (std::is_trivially_copy_constructible_v<T>)
            {
                return data.width();
            }
            else
            {
                return data->width();
            }
        }

        Data data_;
    };
} // namespace prism
