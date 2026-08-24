/**
 * @file main.cpp
 * @author Francesco Corso
 * @date 8/23/2026
 * @brief
 */
import std;
import prism.core;

constexpr auto *program_path = SAMPLE_PROGRAM;

int main()
{
    std::string program;
    {
        const std::ifstream file{program_path};
        std::stringstream buffer;
        buffer << file.rdbuf();
        program = std::move(buffer).str();
    }

    auto syntax_tree = prism::SyntaxTree::parse(std::move(program));
    auto compilation = prism::Compilation::create("test", {std::move(syntax_tree)});

    return 0;
}
