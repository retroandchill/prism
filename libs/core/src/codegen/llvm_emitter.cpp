/**
 * @file llvm_emitter.cpp
 * @author Francesco Corso
 * @date 8/22/2026
 * @brief
 */
module;

#include <llvm/IR/IRBuilder.h>
#include <llvm/IR/LLVMContext.h>
#include <llvm/IR/Module.h>

module prism.core:codegen.llvm_emitter.impl;

import :codegen.llvm_emitter;
import :semantic.compilation;

namespace prism
{
    class LlvmEmitter::Impl final : NonCopyable
    {
      public:
        explicit Impl(const Compilation &compilation, LlvmCodeGenOptions options)
            : compilation_{compilation}, options_{std::move(options)},
              module_{compilation.assembly_name().as_string_view(), context_}
        {
        }

        void emit()
        {
        }

      private:
        const Compilation &compilation_;
        LlvmCodeGenOptions options_;
        llvm::LLVMContext context_;
        llvm::Module module_;
        llvm::IRBuilder<> builder_{context_};

        std::unordered_map<const Symbol *, llvm::Value *> symbol_to_value_;
    };

    LlvmEmitter::LlvmEmitter(const Compilation &compilation, LlvmCodeGenOptions options)
        : impl_{std::make_unique<Impl>(compilation, std::move(options))}
    {
    }

    LlvmEmitter::~LlvmEmitter() = default;

    void LlvmEmitter::emit() const
    {
        return impl_->emit();
    }
} // namespace prism
