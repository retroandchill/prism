/**
 * @file symbol.cpp
 * @author Francesco Corso
 * @date 7/1/2026
 * @brief
 */
module prism.core.semantic.symbol;

namespace prism
{
    std::span<const Ref<const Symbol>> Scope::get(const Name name) const noexcept
    {
        const auto it = symbols_.find(name);
        if (it == symbols_.end())
            return {};
        return it->second.symbols();
    }

    void Scope::add(const Symbol &symbol)
    {
        symbols_[symbol.name].add(symbol);
    }
} // namespace prism
