/**
 * @file token.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core:syntax.green.token;

import :syntax.green.node;
import :syntax.green.trivia;
import :util.exceptions;

namespace prism
{
    class GreenToken : public GreenNode
    {
      public:
        explicit GreenToken(SyntaxKind kind, GreenTriviaList leading_trivia = {}, GreenTriviaList trailing_trivia = {});

        GreenToken(SyntaxKind kind,
                   std::uint32_t width,
                   GreenTriviaList leading_trivia = {},
                   GreenTriviaList trailing_trivia = {});

        static const GreenPtr<GreenToken> &eof();
        static const GreenPtr<GreenToken> &bad_token();

        static GreenPtr<GreenToken> from(SyntaxKind kind);

        [[nodiscard]] virtual std::string_view text() const;

        [[nodiscard]] constexpr const GreenTriviaList &leading_trivia() const noexcept
        {
            return leading_trivia_;
        }

        [[nodiscard]] std::uint32_t leading_trivia_width() const override;

        [[nodiscard]] constexpr const GreenTriviaList &trailing_trivia() const noexcept
        {
            return trailing_trivia_;
        }

        [[nodiscard]] std::uint32_t trailing_trivia_width() const override;

        [[nodiscard]] Optional<const GreenNode &> get_child(std::size_t index) const final;

        [[nodiscard]] static constexpr bool instance_of(const GreenNode &node) noexcept
        {
            return prism::is_token(node.kind());
        }

        [[nodiscard]] const SyntaxNode &create_red(const SyntaxLifetime &,
                                                   const SyntaxNode *,
                                                   std::uint32_t) const final
        {
            throw UnsupportedOperationException{};
        }

        [[nodiscard]] virtual GreenPtr<GreenToken> with_leading_trivia(GreenTriviaList leading_trivia) const;

        [[nodiscard]] virtual GreenPtr<GreenToken> with_trailing_trivia(GreenTriviaList trailing_trivia) const;

        [[nodiscard]] virtual GreenPtr<GreenToken> update(GreenTriviaList leading_trivia,
                                                          GreenTriviaList trailing_trivia) const;

      protected:
        [[nodiscard]] virtual GreenPtr<GreenToken> clone_with_trivia(GreenTriviaList leading_trivia,
                                                                     GreenTriviaList trailing_trivia) const;

      private:
        GreenTriviaList leading_trivia_;
        GreenTriviaList trailing_trivia_;
    };

    template <typename T>
    concept CanGetStringView = requires(const T &t) {
        {
            t.get_string_view()
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

        constexpr GreenValueToken(Data data, GreenTriviaList leading_trivia, GreenTriviaList trailing_trivia)
            : GreenToken{T::kind, get_width(data), std::move(leading_trivia), std::move(trailing_trivia)},
              data_{std::move(data)}
        {
        }

      public:
        constexpr GreenValueToken(T value,
                                  std::string text,
                                  GreenTriviaList leading_trivia = {},
                                  GreenTriviaList trailing_trivia = {})
            requires !CanGetStringView<T>
            : GreenValueToken{make_data(std::move(value), std::move(text)),
                              std::move(leading_trivia),
                              std::move(trailing_trivia)}
        {
        }

        explicit constexpr GreenValueToken(T value,
                                           GreenTriviaList leading_trivia = {},
                                           GreenTriviaList trailing_trivia = {})
            requires CanGetStringView<T>
            : GreenValueToken{make_data(std::move(value)), std::move(leading_trivia), std::move(trailing_trivia)}
        {
        }

        [[nodiscard]] constexpr std::string_view text() const override
        {
            if constexpr (CanGetStringView<T>)
            {
                return data().value.get_string_view();
            }
            else
            {
                return data().text;
            }
        }

        [[nodiscard]] constexpr const T &value() const noexcept
        {
            return data().value;
        }

      protected:
        GreenPtr<GreenToken> clone_with_trivia(GreenTriviaList leading_trivia,
                                               GreenTriviaList trailing_trivia) const override
        {
            return GreenPtr<GreenToken>::no_ref(
                new GreenValueToken{data_, std::move(leading_trivia), std::move(trailing_trivia)});
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

    template <typename T>
        requires StandardLiteralData<std::decay_t<T>> &&
                 !CanGetStringView<std::decay_t<T>>
                 constexpr RefCountPtr<GreenValueToken<std::decay_t<T>>> make_green_value(
                     T && value,
                     std::string text,
                     GreenTriviaList leading_trivia = {},
                     GreenTriviaList trailing_trivia = {})
    {
        return make_ref_counted<GreenValueToken<std::decay_t<T>>>(std::forward<T>(value),
                                                                  std::move(text),
                                                                  std::move(leading_trivia),
                                                                  std::move(trailing_trivia));
    }

    template <typename T>
        requires StandardLiteralData<std::decay_t<T>> && CanGetStringView<std::decay_t<T>>
    constexpr RefCountPtr<GreenValueToken<std::decay_t<T>>> make_green_value(T &&value,
                                                                             GreenTriviaList leading_trivia = {},
                                                                             GreenTriviaList trailing_trivia = {})
    {
        return make_ref_counted<GreenValueToken<std::decay_t<T>>>(std::forward<T>(value),
                                                                  std::move(leading_trivia),
                                                                  std::move(trailing_trivia));
    }

    using GreenTokenList = GreenSyntaxList<GreenToken>;
} // namespace prism
