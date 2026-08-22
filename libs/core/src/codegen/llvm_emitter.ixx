/**
 * @file llvm_emitter.ixx
 * @author Francesco Corso
 * @date 8/22/2026
 * @brief
 */
export module prism.core:codegen.llvm_emitter;

import std;
import :util.noncopyable;

namespace prism
{
    class Compilation;

    struct LlvmCodeGenOptions
    {
        std::filesystem::path output_directory{};
    };

    class LlvmEmitter final : NonCopyable
    {
      public:
        explicit LlvmEmitter(LlvmCodeGenOptions options);
        ~LlvmEmitter();

        void emit(const Compilation &compilation);

      private:
        LlvmCodeGenOptions options_;
    };
} // namespace prism
