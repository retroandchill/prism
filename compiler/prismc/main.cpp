/**
 * @file main.cpp
 * @author Francesco Corso
 * @date 8/23/2026
 * @brief
 */
#ifdef _MSC_VER
#include <crtdbg.h>
#endif

import std;
import prism.core;

constexpr auto *program_path = SAMPLE_PROGRAM;

int main()
{
#ifdef _MSC_VER
    // Direct standard assertions (assert) to the debugger output
    _CrtSetReportMode(_CRT_ASSERT, _CRTDBG_MODE_DEBUG);

    // Direct CRT errors and runtime checks to the debugger output
    _CrtSetReportMode(_CRT_ERROR, _CRTDBG_MODE_DEBUG);
#endif

    std::string program;
    {
        const std::ifstream file{program_path};
        std::stringstream buffer;
        buffer << file.rdbuf();
        program = std::move(buffer).str();
    }

    if (program.size() >= 3 && static_cast<unsigned char>(program[0]) == 0xEF &&
        static_cast<unsigned char>(program[1]) == 0xBB && static_cast<unsigned char>(program[2]) == 0xBF)
    {
        program.erase(0, 3);
    }

    auto syntax_tree = prism::SyntaxTree::parse(std::move(program));
    const auto compilation = prism::Compilation::create("test", {std::move(syntax_tree)});

    if (auto [is_success, diagnostics] = compilation->emit(std::filesystem::path{program_path}.parent_path());
        !is_success)
    {
        std::println("Compilation failed");
        return 1;
    }

    return 0;
}
