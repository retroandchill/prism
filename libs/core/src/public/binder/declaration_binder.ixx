/**
 * @file declaration_binder.ixx
 * @author Francesco Corso
 * @date 8/1/2026
 * @brief
 */
export module prism.core:binder.declaration_binder;

import :util.noncopyable;
import :text.name;
import :syntax.list;

namespace prism
{
    class FunctionDeclarationSyntax;
    class ParameterSyntax;
    class VariableDeclarationSyntax;
    class NamespaceDeclarationSyntax;
    class SyntaxTree;
    class DeclarationSyntax;

    struct NamespaceRecord;
    struct VariableRecord;
    struct FunctionRecord;

    using DeclarationRecord = std::variant<NamespaceRecord, VariableRecord, FunctionRecord>;

    struct NamespaceRecord
    {
        std::vector<Name> names;
        const NamespaceDeclarationSyntax *syntax = nullptr;
        std::vector<DeclarationRecord> declarations;
    };

    struct VariableRecord
    {
        Name name;
        const VariableDeclarationSyntax *syntax = nullptr;
    };

    struct ParameterRecord
    {
        Name name;
        const ParameterSyntax *syntax = nullptr;
    };

    struct FunctionRecord
    {
        Name name;
        const FunctionDeclarationSyntax *syntax = nullptr;
        std::vector<ParameterRecord> parameters;
    };

    std::vector<DeclarationRecord> scan_declarations(const SyntaxTree &tree);

    class DeclarationBinder final : NonCopyable
    {
      public:
        constexpr explicit DeclarationBinder(const SyntaxTree &tree) noexcept : tree_{tree}
        {
        }

        [[nodiscard]] std::vector<DeclarationRecord> bind() const;

      private:
        [[nodiscard]] std::vector<DeclarationRecord> bind_declarations(
            SyntaxList<DeclarationSyntax> declarations) const;
        [[nodiscard]] Optional<DeclarationRecord> bind_declaration(const DeclarationSyntax &syntax) const;

        const SyntaxTree &tree_;
    };
} // namespace prism
