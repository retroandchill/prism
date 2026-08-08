/**
 * @file signature_binder.cpp
 * @author Francesco Corso
 * @date 8/6/2026
 * @brief
 */
module prism.core:binder.signature_binder.impl;

import :binder.signature_binder;
import :symbols.source;
import :syntax.declarations;
import :syntax.clauses;
import :binder.binding_helpers;
import :symbols.error;
import :semantic.compilation;

namespace prism
{

    void SignatureBinder::bind(std::span<PartiallyBoundSymbol> partially_bound)
    {
        for (auto &symbol : partially_bound)
        {
            std::visit(Overload{[this](SourceVariableSymbol &s) { bind_variable_type(s); },
                                [this](SourceFunctionSymbol &s) { bind_function_return_type(s); },
                                [this](SourceParameterSymbol &s)
                                {
                                    bind_function_parameter_type(s);
                                }},
                       symbol);
        }
    }

    void SignatureBinder::bind_variable_type(SourceVariableSymbol &variable) const
    {
        if (const auto type = variable.syntax_.type(); type.has_value())
        {
            variable.set_type(resolve_type(type->type(), compilation_, diagnostics_));
            return;
        }

        diagnostics_.add(
            Diagnostic{DiagnosticInfo::create<DiagnosticCode::expected_type_specifier>(), variable.syntax_.location()});
        variable.set_type(unnamed_error_type);
    }

    void SignatureBinder::bind_function_return_type(SourceFunctionSymbol &function) const
    {
        if (const auto return_type = function.syntax_.return_type(); return_type.has_value())
        {
            function.set_return_type(resolve_type(return_type->type(), compilation_, diagnostics_));
        }
        else
        {
            function.set_return_type(compilation_.get_special_type(SpecialType::void_));
        }
    }

    void SignatureBinder::bind_function_parameter_type(SourceParameterSymbol &parameter) const
    {
        if (const auto type = parameter.syntax_.type_specifier(); type.has_value())
        {
            parameter.set_type(resolve_type(type->type(), compilation_, diagnostics_));
            return;
        }

        diagnostics_.add(Diagnostic{DiagnosticInfo::create<DiagnosticCode::expected_type_specifier>(),
                                    parameter.syntax_.location()});
        parameter.set_type(unnamed_error_type);
    }
} // namespace prism
