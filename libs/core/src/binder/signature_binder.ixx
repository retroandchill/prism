/**
 * @file signature_binder.ixx
 * @author Francesco Corso
 * @date 8/6/2026
 * @brief
 */

export module prism.core:binder.signature_binder;

import :util.noncopyable;
import :diagnostics.diagnostic_bag;
import :util.ref;

namespace prism
{
    class SourceParameterSymbol;
    class SourceFunctionSymbol;
    class SourceVariableSymbol;
    class TypeSyntax;
    class TypeSymbol;
    class Compilation;

    using PartiallyBoundSymbol =
        std::variant<Ref<SourceVariableSymbol>, Ref<SourceFunctionSymbol>, Ref<SourceParameterSymbol>>;

    class SignatureBinder final : NonCopyable
    {
      public:
        constexpr SignatureBinder(Compilation &compilation, DiagnosticBag &diagnostics)
            : compilation_{compilation}, diagnostics_{diagnostics}
        {
        }

        void bind(std::span<PartiallyBoundSymbol> partially_bound) const;

      private:
        void bind_variable_type(SourceVariableSymbol &variable) const;
        void bind_function_return_type(SourceFunctionSymbol &function) const;
        void bind_function_parameter_type(SourceParameterSymbol &parameter) const;

        Compilation &compilation_;
        DiagnosticBag &diagnostics_;
    };
} // namespace prism
