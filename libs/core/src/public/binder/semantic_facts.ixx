/**
 * @file semantic_facts.ixx
 * @author Francesco Corso
 * @date 8/2/2026
 * @brief
 */
export module prism.core:binder.semantic_facts;

import :symbols.variable_symbol;
import :util.lazy;
import boost;
import :memory.persistent_allocator;

namespace prism
{
    struct VariableFacts
    {
        Lazy<const TypeSymbol &> type;
    };

    class SemanticFacts final
    {
      public:
        const TypeSymbol &type_of(const VariableSymbol &symbol);

      private:
        VariableFacts &facts_for(const VariableSymbol &symbol);

        const TypeSymbol &compute_type(const VariableSymbol &symbol);

        PersistentAllocator allocator_;
        std::mutex variable_facts_mutex_;
        std::unordered_map<const VariableSymbol *, VariableFacts *> variable_facts_;
    };
} // namespace prism
