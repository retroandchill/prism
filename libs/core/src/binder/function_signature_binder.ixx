/**
 * @file function_signature_binder.ixx
 * @author Francesco Corso
 * @date 8/18/2026
 * @brief
 */
export module prism.core:binder.function_signature_binder;

import :binder;

namespace prism
{
    class FunctionSymbol;
    class FunctionDeclarationSyntax;

    class FunctionSignatureBinder final : public Binder
    {

      public:
        FunctionSignatureBinder(const Binder &next,
                                const FunctionSymbol &symbol,
                                const FunctionDeclarationSyntax &syntax);

        [[nodiscard]] Optional<const SyntaxNode &> scope_designator() const override;

        [[nodiscard]] Optional<const Symbol &> containing_symbol() const override;

      protected:
        [[nodiscard]] LookupResult lookup_local(Name name,
                                                LookupOptions options,
                                                const LookupContext &context) const override;

      private:
        const FunctionDeclarationSyntax &syntax_;
        const FunctionSymbol &symbol_;
    };
} // namespace prism
