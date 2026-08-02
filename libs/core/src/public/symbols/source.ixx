/**
 * @file source.ixx
 * @author Francesco Corso
 * @date 8/1/2026
 * @brief
 */
export module prism.core:symbols.source;

import :symbols.namespace_symbol;
import :symbols.assembly_symbol;

namespace prism
{
    class SourceAssemblySymbol final : public AssemblySymbol
    {
      public:
        constexpr explicit SourceAssemblySymbol(const Name name) : AssemblySymbol{name}
        {
        }
    };

    class SourceNamespaceSymbol final : public NamespaceSymbol
    {
      public:
        constexpr SourceNamespaceSymbol(const Name name, const AssemblySymbol *assembly)
            : NamespaceSymbol{name, assembly}
        {
        }

        constexpr SourceNamespaceSymbol(const Name name, const NamespaceSymbol *containing)
            : NamespaceSymbol{name, containing}
        {
        }

        [[nodiscard]] constexpr SymbolSpan<Symbol> members() const override
        {
            return members_;
        }

      private:
        constexpr void add_member(const Symbol &member)
        {
            members_.emplace_back(member);
        }

        template <std::ranges::input_range Range>
            requires std::convertible_to<std::ranges::range_reference_t<Range>, Ref<const Symbol>>
        constexpr void add_members(Range &&range)
        {
            members_.append_range(std::forward<Range>(range));
        }

        friend class DeclarationBinder;

        std::vector<Ref<const Symbol>> members_;
    };
} // namespace prism
