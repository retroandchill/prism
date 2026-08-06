/**
 * @file signature_binder.ixx
 * @author Francesco Corso
 * @date 8/6/2026
 * @brief
 */

export module prism.core:binder.signature_binder;

import :binder.declaration_merger;

namespace prism
{
    class TypeSyntax;
    class TypeSymbol;
    class Compilation;

    class SignatureBinder final : NonCopyable
    {
      public:
        constexpr SignatureBinder(const Compilation &compilation, DiagnosticBag &diagnostics)
            : compilation_{compilation}, diagnostics_{diagnostics}
        {
        }

        void bind(std::span<PartiallyBoundSymbol> partially_bound);

      private:
        void bind_variable_type(SourceVariableSymbol &variable) const;
        void bind_function_return_type(SourceFunctionSymbol &function) const;
        void bind_function_parameter_type(SourceParameterSymbol &parameter) const;

        const Compilation &compilation_;
        DiagnosticBag &diagnostics_;
    };
} // namespace prism
