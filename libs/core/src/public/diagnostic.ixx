/**
 * @file diagnostic.ixx
 * @author Francesco Corso
 * @date 6/26/2026
 * @brief
 */
export module prism.core.diagnostic;

import std;
import prism.core.source.source_file;
import prism.core.util;
import prism.core.lexer.token;

namespace prism
{
    export enum class Severity : std::uint8_t
    {
        error,
        warning,
        info,
        hint,
    };

    export struct EmptyStatement
    {
    };

    export constexpr EmptyStatement empty_statement{};

    export struct EmptyExpression
    {
    };

    export constexpr EmptyExpression empty_expression{};

    export struct UnexpectedToken
    {
        TokenKind actual = TokenKind::unrecognized;
        std::vector<TokenKind> expected;
    };

    export struct DuplicateSymbolDefinition
    {
        SharedString name;
    };

    export struct OtherDiagnostic
    {
        SharedString message;
    };

    export using DiagnosticInfo =
        std::variant<EmptyStatement, EmptyExpression, UnexpectedToken, DuplicateSymbolDefinition, OtherDiagnostic>;

    export struct Diagnostic
    {
        Severity severity = Severity::error;
        SourceRange range;
        DiagnosticInfo info;
    };

    export class DiagnosticSink
    {
      public:
        [[nodiscard]] constexpr const std::vector<Diagnostic> &diagnostics() const noexcept
        {
            return diagnostics_;
        }

        constexpr void report(Severity severity, SourceRange range, DiagnosticInfo info)
        {
            diagnostics_.emplace_back(severity, range, std::move(info));
        }

      private:
        std::vector<Diagnostic> diagnostics_;
    };
} // namespace prism
