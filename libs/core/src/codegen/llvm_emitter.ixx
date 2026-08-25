/**
 * @file llvm_emitter.ixx
 * @author Francesco Corso
 * @date 8/22/2026
 * @brief
 */
export module prism.core:codegen.llvm_emitter;

import std;
import :util.noncopyable;
import :compilation;

namespace prism
{
    struct LlvmCodeGenOptions
    {
        std::filesystem::path output_directory{};
    };

    class LlvmEmitter final : NonCopyable
    {
      public:
        explicit LlvmEmitter(const Compilation &compilation, LlvmCodeGenOptions options);
        ~LlvmEmitter();

        EmitResult emit() const;

      private:
        class Impl;
        std::unique_ptr<Impl> impl_;
    };
} // namespace prism
