/**
 * @file llvm_emitter.cpp
 * @author Francesco Corso
 * @date 8/22/2026
 * @brief
 */
module;

module prism.core:codegen.llvm_emitter.impl;

import :codegen.llvm_emitter;

namespace prism
{
    LlvmEmitter::LlvmEmitter(LlvmCodeGenOptions options) : options_{std::move(options)}
    {
    }

    LlvmEmitter::~LlvmEmitter() = default;

    void LlvmEmitter::emit(const Compilation &compilation)
    {
    }
} // namespace prism
