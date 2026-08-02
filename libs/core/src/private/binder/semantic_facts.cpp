/**
 * @file semantic_facts.cpp
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
module prism.core:binder.semantic_facts.impl;

import :binder.semantic_facts;

namespace prism
{

    const TypeSymbol &SemanticFacts::type_of(const VariableSymbol &symbol)
    {
        auto &[type] = facts_for(symbol);
        return type.get_or_compute([&] -> auto & { return compute_type(symbol); });
    }

    VariableFacts &SemanticFacts::facts_for(const VariableSymbol &symbol)
    {
        std::scoped_lock lock{variable_facts_mutex_};
        if (const auto it = variable_facts_.find(&symbol); it != variable_facts_.end())
            return *it->second;

        auto &facts = allocator_.create<VariableFacts>();
        variable_facts_.emplace(&symbol, &facts);
        return facts;
    }

    const TypeSymbol &SemanticFacts::compute_type(const VariableSymbol &symbol)
    {
        throw NotImplementedException{};
    }
} // namespace prism
