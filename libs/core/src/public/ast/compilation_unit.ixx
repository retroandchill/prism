/**
 * @file compilation_unit.ixx
 * @author Francesco Corso
 * @date 6/24/2026
 * @brief
 */
export module prism.core.ast.compilation_syntax;

import std;
import prism.core.ast.statement_syntax;

namespace prism
{
    export struct CompilationUnitSyntax
    {
        std::vector<StatementSyntax> statements;
    };
} // namespace prism
