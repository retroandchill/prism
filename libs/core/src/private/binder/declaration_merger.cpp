/**
 * @file declaration_merger.cpp
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module prism.core:binder.declaration_merger.impl;

import :binder.declaration_merger;
import :symbols.symbol_lifetime;
import :symbols.source;
import :util.overload;
import :syntax.declarations;

namespace prism
{

    const AssemblySymbol &DeclarationMerger::merge(const std::span<const DeclarationRecord> records)
    {
        assembly_ = &lifetime_.create<SourceAssemblySymbol>(assembly_name_);
        global_namespace_ = &lifetime_.create<SourceNamespaceSymbol>(KnownName::global, assembly_);

        assembly_->global_namespace_ = global_namespace_;

        for (const auto &record : records)
            merge_declaration(record, *global_namespace_);

        return *assembly_;
    }

    void DeclarationMerger::merge_declaration(const DeclarationRecord &record,
                                              SourceNamespaceSymbol &containing_namespace)
    {
        std::visit(
            Overload{
                [&](const NamespaceRecord &r) { merge_namespace(r, containing_namespace); },
                [&](const VariableRecord &r) { merge_variable(r, containing_namespace); },
                [&](const FunctionRecord &r) { merge_function(r, containing_namespace); },
            },
            record);
    }

    void DeclarationMerger::merge_namespace(const NamespaceRecord &record, SourceNamespaceSymbol &containing_namespace)
    {
        SourceNamespaceSymbol *namespace_symbol = &containing_namespace;

        for (const auto name : record.names)
            namespace_symbol = &get_or_create_namespace(*namespace_symbol, name);
        namespace_symbol->add_syntax_reference(*record.syntax);

        for (const auto &declaration : record.declarations)
            merge_declaration(declaration, *namespace_symbol);
    }

    void DeclarationMerger::merge_variable(const VariableRecord &record,
                                           SourceNamespaceSymbol &containing_namespace) const
    {
        const auto &symbol = lifetime_.create<SourceVariableSymbol>(record.name, &containing_namespace, *record.syntax);
        containing_namespace.add_member(symbol);
    }

    void DeclarationMerger::merge_function(const FunctionRecord &record,
                                           SourceNamespaceSymbol &containing_namespace) const
    {
        auto &symbol = lifetime_.create<SourceFunctionSymbol>(record.name, &containing_namespace, *record.syntax);
        symbol.add_parameters(
            record.parameters |
            std::views::transform(
                [&](const ParameterRecord &parameter) -> auto &
                { return lifetime_.create<SourceParameterSymbol>(parameter.name, &symbol, *parameter.syntax); }));
        containing_namespace.add_member(symbol);
    }

    SourceNamespaceSymbol &DeclarationMerger::get_or_create_namespace(SourceNamespaceSymbol &containing_namespace,
                                                                      Name name)
    {
        auto &namespaces = namespaces_[&containing_namespace];

        if (const auto it = namespaces.find(name); it != namespaces.end())
            return *it->second;

        auto &namespace_symbol = lifetime_.create<SourceNamespaceSymbol>(name, &containing_namespace);

        namespaces.emplace(name, &namespace_symbol);
        containing_namespace.add_member(namespace_symbol);

        return namespace_symbol;
    }
} // namespace prism
