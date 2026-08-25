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

    if (program.size() >= 3 && static_cast<unsigned char>(program[0]) == 0xEF &&
        static_cast<unsigned char>(program[1]) == 0xBB && static_cast<unsigned char>(program[2]) == 0xBF)
    {
        program.erase(0, 3);
    }

    auto syntax_tree = prism::SyntaxTree::parse(std::move(program));
    auto compilation = prism::Compilation::create("test", {std::move(syntax_tree)});

    auto result = compilation->emit(std::filesystem::current_path());
    if (!result.success)
    {
        std::println("Compilation failed");
        return 1;
    }

    return 0;
}
