/**
 * @file llvm_emitter.cpp
 * @author Francesco Corso
 * @date 8/22/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>
#include <lld/Common/Driver.h>
#include <llvm/IR/IRBuilder.h>
#include <llvm/IR/LegacyPassManager.h>
#include <llvm/IR/LLVMContext.h>
#include <llvm/IR/Module.h>
#include <llvm/MC/TargetRegistry.h>
#include <llvm/Support/Error.h>
#include <llvm/Support/FileSystem.h>
#include <llvm/Support/raw_os_ostream.h>
#include <llvm/Support/raw_ostream.h>
#include <llvm/Support/TargetSelect.h>
#include <llvm/Target/TargetMachine.h>
#include <llvm/Target/TargetOptions.h>
#include <llvm/Transforms/Utils/ModuleUtils.h>

module prism.core:codegen.llvm_emitter.impl;

import :codegen.llvm_emitter;
import :symbols.visit;
import :semantic.constant_value;
import :syntax.visit;
import :semantic.bound.visit;

extern "C++"
{
    LLD_HAS_DRIVER(coff)
    LLD_HAS_DRIVER(elf)
    LLD_HAS_DRIVER(mingw)
    LLD_HAS_DRIVER(macho)
    LLD_HAS_DRIVER(darwin)
    LLD_HAS_DRIVER(wasm)
}

namespace prism
{
    namespace
    {
        [[nodiscard]] BinaryOperation to_binary_operation(const AssignmentOperation operation)
        {
            switch (operation)
            {
                case AssignmentOperation::addition:
                    return BinaryOperation::addition;
                case AssignmentOperation::subtraction:
                    return BinaryOperation::subtraction;
                case AssignmentOperation::multiplication:
                    return BinaryOperation::multiplication;
                case AssignmentOperation::division:
                    return BinaryOperation::division;
                case AssignmentOperation::modulo:
                    return BinaryOperation::modulo;
                case AssignmentOperation::bitwise_and:
                    return BinaryOperation::bitwise_and;
                case AssignmentOperation::bitwise_or:
                    return BinaryOperation::bitwise_or;
                case AssignmentOperation::bitwise_xor:
                    return BinaryOperation::bitwise_xor;
                case AssignmentOperation::logical_and:
                    return BinaryOperation::logical_and;
                case AssignmentOperation::logical_or:
                    return BinaryOperation::logical_or;
                case AssignmentOperation::shift_left:
                    return BinaryOperation::shift_left;
                case AssignmentOperation::shift_right:
                    return BinaryOperation::shift_right;
                case AssignmentOperation::unsigned_shift_right:
                    return BinaryOperation::unsigned_shift_right;
                default:
                    UNREACHABLE("Doesn't correspond to an assignment operation");
            }
        }
        [[nodiscard]] constexpr std::string_view to_llvm_arch_string(const CompilationSettings &settings)
        {
            switch (settings.architecture)
            {
                case Architecture::x86:
                    return settings.pointer_width == PointerWidth::x64 ? "x86_64" : "i386";
                case Architecture::arm:
                    return settings.pointer_width == PointerWidth::x64 ? "aarch64" : "arm";
                case Architecture::riscv:
                    return settings.pointer_width == PointerWidth::x64 ? "riscv64" : "riscv32";
                case Architecture::wasm:
                    return settings.pointer_width == PointerWidth::x64 ? "wasm64" : "wasm32";
                case Architecture::unknown:
                default:
                    return "unknown";
            }
        }

        [[nodiscard]] constexpr std::string_view to_llvm_os_string(const OperatingSystem os)
        {
            switch (os)
            {
                case OperatingSystem::linux:
                    return "linux";
                case OperatingSystem::windows:
                    return "windows";
                case OperatingSystem::macos:
                    return "darwin";
                case OperatingSystem::freestanding:
                    return "none";
                default:
                    return "unknown";
            }
        }

        [[nodiscard]] constexpr std::string_view to_llvm_env_string(const Environment env) noexcept
        {
            switch (env)
            {
                case Environment::gnu:
                    return "gnu";
                case Environment::musl:
                    return "musl";
                case Environment::msvc:
                    return "msvc";
                case Environment::none:
                default:
                    return "";
            }
        }

        [[nodiscard]] std::string get_llvm_triple(const CompilationSettings &settings)
        {
            std::string triple;

            triple += to_llvm_arch_string(settings);
            triple += "-unknown-";
            triple += to_llvm_os_string(settings.operating_system);
            if (const auto env = to_llvm_env_string(settings.environment); !env.empty())
            {
                triple += '-';
                triple += env;
            }

            return triple;
        }
    } // namespace

    struct ScopeFrame
    {
        std::unordered_map<const Symbol *, llvm::Value *> storage;
    };

    class FunctionEmissionContext
    {
      public:
        FunctionEmissionContext(llvm::Function *function, llvm::BasicBlock *entry_block)
            : function_{function}, entry_block_{entry_block}, scopes_{1}
        {
        }

        [[nodiscard]] llvm::Function *function() const
        {
            return function_;
        }

        [[nodiscard]] llvm::BasicBlock *entry_block() const
        {
            return entry_block_;
        }

        void push_scope()
        {
            scopes_.emplace_back();
        }

        void pop_scope()
        {
            scopes_.pop_back();
        }

        void bind_storage(const Symbol &symbol, llvm::Value *storage)
        {
            scopes_.back().storage.emplace(&symbol, storage);
        }

        llvm::Value *lookup_storage(const Symbol &symbol)
        {
            for (auto &[storage] : scopes_ | std::views::reverse)
            {
                if (auto it = storage.find(&symbol); it != storage.end())
                {
                    return it->second;
                }
            }

            return nullptr;
        }

      private:
        llvm::Function *function_ = nullptr;
        llvm::BasicBlock *entry_block_ = nullptr;
        std::vector<ScopeFrame> scopes_;
    };

    class InsertPointScope final : NonCopyable
    {
      public:
        explicit InsertPointScope(llvm::IRBuilder<> &builder, llvm::BasicBlock *block) noexcept
            : builder_{builder}, old_insert_block_{builder.GetInsertBlock()}
        {
            builder_.SetInsertPoint(block);
        }

        ~InsertPointScope()
        {
            builder_.SetInsertPoint(old_insert_block_);
        }

      private:
        llvm::IRBuilder<> &builder_;
        llvm::BasicBlock *old_insert_block_ = nullptr;
    };

    enum class UnaryReturnType : std::uint8_t
    {
        prefix,
        postfix
    };

    enum class UnaryArithmeticType : std::uint8_t
    {
        increment,
        decrement
    };

    class LlvmEmitter::Impl final : NonCopyable
    {
      public:
        explicit Impl(const Compilation &compilation, LlvmCodeGenOptions options)
            : compilation_{compilation}, options_{std::move(options)},
              module_{compilation.assembly_name().as_string_view(), context_}
        {
        }

        EmitResult emit()
        {
            for (auto &global : CompilationInternal::get_global_variables(compilation_))
            {
                get_or_create_global(global);
            }

            for (auto &function : CompilationInternal::get_functions(compilation_))
            {
                get_or_create_function(function);
            }

            Optional<FunctionEmissionContext> assembly_initializer_context;
            for (const auto &global : CompilationInternal::get_global_variables(compilation_))
                emit_global_initializer(global, assembly_initializer_context);

            if (assembly_initializer_context.has_value())
            {
                builder_.CreateRetVoid();
                llvm::appendToGlobalCtors(module_, assembly_initializer_context->function(), 65535);
            }

            for (const auto &function : CompilationInternal::get_functions(compilation_))
                emit_function_body(function);

            if (compilation_.target_settings().is_application() && !emit_entry_point())
            {
                return {.is_success = false};
            }

            auto result = write_ir();
            return output_binary();
        }

      private:
        llvm::Function *get_or_create_function(const FunctionSymbol &symbol)
        {
            if (const auto it = symbol_to_value_.find(&symbol); it != symbol_to_value_.end())
                return llvm::cast<llvm::Function>(it->second);

            auto *return_type = get_or_create_type(symbol.return_type());
            const auto parameters = symbol.parameters() |
                                    std::views::transform([this](const ParameterSymbol &parameter)
                                                          { return get_or_create_type(parameter.type()); }) |
                                    std::ranges::to<std::vector>();

            auto name = mangle_function(symbol);
            auto *function = llvm::Function::Create(llvm::FunctionType::get(return_type, parameters, false),
                                                    llvm::Function::ExternalLinkage,
                                                    std::move(name),
                                                    &module_);
            symbol_to_value_[&symbol] = function;
            return function;
        }

        llvm::GlobalVariable *get_or_create_global(const VariableSymbol &symbol)
        {
            if (const auto it = symbol_to_value_.find(&symbol); it != symbol_to_value_.end())
                return llvm::cast<llvm::GlobalVariable>(it->second);

            auto *type = get_or_create_type(symbol.type());

            const auto name = mangle_global(symbol);
            auto *variable = new llvm::GlobalVariable(module_,
                                                      type,
                                                      false,
                                                      llvm::GlobalVariable::ExternalLinkage,
                                                      llvm::Constant::getNullValue(type),
                                                      name);
            symbol_to_value_[&symbol] = variable;
            return variable;
        }

        llvm::Type *get_or_create_type(const TypeSymbol &symbol)
        {
            if (const auto it = symbol_to_type_.find(&symbol); it != symbol_to_type_.end())
                return it->second;

            auto *type = create_type(symbol);
            symbol_to_type_[&symbol] = type;
            return type;
        }

        void emit_global_initializer(const VariableSymbol &symbol,
                                     Optional<FunctionEmissionContext> &assembly_initializer_context)
        {
            auto *variable = get_or_create_global(symbol);
            if (!symbol.has_initializer())
            {
                return;
            }

            const auto initializer = CompilationInternal::get_bound_initializer(compilation_, symbol);
            if (!initializer.has_value())
            {
                return;
            }

            if (auto &constant = initializer->constant_value(); constant.has_value())
            {
                variable->setInitializer(make_constant(*constant));
                return;
            }

            if (!assembly_initializer_context.has_value())
            {
                auto *function_type = llvm::FunctionType::get(builder_.getVoidTy(), false);

                const auto initializer_name = std::format("{}_<GlobalInitializer>", compilation_.assembly_name());
                auto *assembly_initializer =
                    llvm::Function::Create(function_type, llvm::Function::InternalLinkage, initializer_name, module_);
                auto *block = llvm::BasicBlock::Create(context_, "entry", assembly_initializer);

                assembly_initializer_context = FunctionEmissionContext{assembly_initializer, block};
                builder_.SetInsertPoint(block);
            }

            auto *initialized_value = emit_expression(*initializer, *assembly_initializer_context);
            builder_.CreateStore(initialized_value, variable);
        }

        void emit_function_body(const FunctionSymbol &symbol)
        {
            auto *function = get_or_create_function(symbol);
            const auto body = CompilationInternal::get_bound_body(compilation_, symbol);
            if (!body.has_value())
            {
                function->setLinkage(llvm::Function::AvailableExternallyLinkage);
                return;
            }

            auto *entry = llvm::BasicBlock::Create(context_, "entry", function);
            InsertPointScope scope{builder_, entry};
            FunctionEmissionContext context{
                function,
                entry,
            };
            for (auto [symbol_param, llvm_param] : std::views::zip(symbol.parameters(), function->args()))
            {
                context.bind_storage(symbol_param, &llvm_param);
            }
            emit_statement(*body, context);
        }

        bool emit_entry_point()
        {
            DEBUG_ASSERT(compilation_.target_settings().is_application());
            const auto entry_point = compilation_.get_entry_point();
            if (!entry_point.has_value())
                return false;

            auto *function_type = llvm::FunctionType::get(builder_.getInt32Ty(), false);
            auto *main_func = llvm::Function::Create(function_type, llvm::Function::ExternalLinkage, "main", module_);
            auto *entry = llvm::BasicBlock::Create(context_, "entry", main_func);
            builder_.SetInsertPoint(entry);

            auto *call_entry_point = builder_.CreateCall(get_or_create_function(*entry_point));
            if (entry_point->returns_void())
            {
                builder_.CreateRet(builder_.getInt32(0));
            }
            else
            {
                builder_.CreateRet(call_entry_point);
            }

            return true;
        }

        llvm::Type *create_type(const TypeSymbol &symbol)
        {
            switch (symbol.special_type())
            {
                case SpecialType::void_:
                    return llvm::Type::getVoidTy(context_);
                case SpecialType::bool_:
                case SpecialType::i8:
                case SpecialType::u8:
                case SpecialType::char_:
                    return llvm::Type::getInt8Ty(context_);
                case SpecialType::i16:
                case SpecialType::u16:
                case SpecialType::char16:
                    return llvm::Type::getInt16Ty(context_);
                case SpecialType::i32:
                case SpecialType::u32:
                case SpecialType::rune:
                    return llvm::Type::getInt32Ty(context_);
                case SpecialType::i64:
                case SpecialType::u64:
                    return llvm::Type::getInt64Ty(context_);
                case SpecialType::i128:
                case SpecialType::u128:
                    return llvm::Type::getInt128Ty(context_);
                case SpecialType::isize:
                case SpecialType::usize:
                    switch (compilation_.target_settings().pointer_width)
                    {

                        case PointerWidth::x32:
                            return llvm::Type::getInt32Ty(context_);
                            break;
                        case PointerWidth::x64:
                            return llvm::Type::getInt64Ty(context_);
                        default:
                            UNREACHABLE("Invalid pointer width");
                    }
                    return llvm::Type::getInt8Ty(context_);
                case SpecialType::f32:
                    return llvm::Type::getFloatTy(context_);
                case SpecialType::f64:
                    return llvm::Type::getDoubleTy(context_);
                default:
                    throw NotImplementedException{};
            }
        }

        static std::string mangle_global(const VariableSymbol &variable)
        {
            std::string name;

            name.push_back('$');
            name.append(variable.containing_assembly().value().name().as_string_view());
            name.append("_V_");
            mangle_container_path(variable, name);
            mangle_name(variable.name(), name);

            return name;
        }
        static std::string mangle_function(const FunctionSymbol &function)
        {
            std::string name;

            name.push_back('$');
            name.append(function.containing_assembly().value().name().as_string_view());
            name.append("_F_");
            mangle_container_path(function, name);
            mangle_name(function.name(), name);
            name.append("E_");
            for (auto param : function.parameters())
            {
                mangle_type(param->type(), name);
                name.push_back('_');
            }
            name.append("R_");
            mangle_type(function.return_type(), name);
            return name;
        }

        static void mangle_type(const TypeSymbol &type, std::string &out)
        {
            switch (type.special_type())
            {

                case SpecialType::none:
                    mangle_container_path(type, out);
                    mangle_name(type.name(), out);
                    break;
                case SpecialType::void_:
                    out.push_back('V');
                    break;
                case SpecialType::bool_:
                    out.push_back('B');
                    break;
                case SpecialType::i8:
                    out.append("I8");
                    break;
                case SpecialType::i16:
                    out.append("I16");
                    break;
                case SpecialType::i32:
                    out.append("I32");
                    break;
                case SpecialType::i64:
                    out.append("I64");
                    break;
                case SpecialType::i128:
                    out.append("I128");
                    break;
                case SpecialType::isize:
                    out.append("IZ");
                    break;
                case SpecialType::u8:
                    out.append("U8");
                    break;
                case SpecialType::u16:
                    out.append("U16");
                    break;
                case SpecialType::u32:
                    out.append("U32");
                    break;
                case SpecialType::u64:
                    out.append("U64");
                    break;
                case SpecialType::u128:
                    out.append("U128");
                    break;
                case SpecialType::usize:
                    out.append("UZ");
                    break;
                case SpecialType::f32:
                    out.append("F32");
                    break;
                case SpecialType::f64:
                    out.append("F64");
                    break;
                case SpecialType::char_:
                    out.push_back('C');
                    break;
                case SpecialType::char16:
                    out.append("C16");
                    break;
                case SpecialType::rune:
                    out.push_back('R');
                    break;
                case SpecialType::str:
                    out.push_back('S');
                    break;
            }
        }

        static void mangle_container_path(const Symbol &symbol, std::string &out)
        {
            out.push_back('N');
            PooledVector<Name> names;
            auto current = symbol.containing_symbol();
            while (current.has_value() && !current->is<AssemblySymbol>())
            {
                names.push_back(current->name());
                current = current->containing_symbol().value_ptr();
            }

            for (const auto n : names)
            {
                mangle_name(n, out);
            }
        }

        static void mangle_name(const Name name, std::string &out)
        {
            auto view = name.as_string_view();
            std::format_to(std::back_inserter(out), "{}{}", view.size(), view);
        }

        llvm::Constant *make_constant(const ConstantValue &value)
        {
            switch (value.kind())
            {
                case ConstantValue::Kind::bool_:
                    return value.as_boolean() ? llvm::ConstantInt::getTrue(context_)
                                              : llvm::ConstantInt::getFalse(context_);
                case ConstantValue::Kind::char_:
                    return llvm::ConstantInt::get(llvm::Type::getInt8Ty(context_), value.as_character());
                case ConstantValue::Kind::char16:
                    return llvm::ConstantInt::get(llvm::Type::getInt16Ty(context_), value.as_character());
                case ConstantValue::Kind::rune:
                    return llvm::ConstantInt::get(llvm::Type::getInt32Ty(context_), value.as_character());
                case ConstantValue::Kind::i8:
                    return llvm::ConstantInt::get(llvm::Type::getInt8Ty(context_), value.as_i64(), true);
                case ConstantValue::Kind::i16:
                    return llvm::ConstantInt::get(llvm::Type::getInt16Ty(context_), value.as_i64(), true);
                case ConstantValue::Kind::i32:
                    return llvm::ConstantInt::get(llvm::Type::getInt32Ty(context_), value.as_i64(), true);
                case ConstantValue::Kind::i64:
                    return llvm::ConstantInt::get(llvm::Type::getInt64Ty(context_), value.as_i64(), true);
                case ConstantValue::Kind::i128:
                    return llvm::ConstantInt::get(llvm::Type::getInt128Ty(context_), value.as_i64(), true);
                case ConstantValue::Kind::isize:
                    switch (compilation_.target_settings().pointer_width)
                    {
                        case PointerWidth::x32:
                            return llvm::ConstantInt::get(llvm::Type::getInt32Ty(context_), value.as_i64(), true);
                        case PointerWidth::x64:
                            return llvm::ConstantInt::get(llvm::Type::getInt64Ty(context_), value.as_i64(), true);
                        default:
                            UNREACHABLE("Invalid pointer size");
                    }
                case ConstantValue::Kind::u8:
                    return llvm::ConstantInt::get(llvm::Type::getInt8Ty(context_), value.as_u64());
                case ConstantValue::Kind::u16:
                    return llvm::ConstantInt::get(llvm::Type::getInt16Ty(context_), value.as_u64());
                case ConstantValue::Kind::u32:
                    return llvm::ConstantInt::get(llvm::Type::getInt32Ty(context_), value.as_u64());
                case ConstantValue::Kind::u64:
                    return llvm::ConstantInt::get(llvm::Type::getInt64Ty(context_), value.as_u64());
                case ConstantValue::Kind::u128:
                    return llvm::ConstantInt::get(llvm::Type::getInt128Ty(context_), value.as_u128());
                case ConstantValue::Kind::usize:
                    switch (compilation_.target_settings().pointer_width)
                    {
                        case PointerWidth::x32:
                            return llvm::ConstantInt::get(llvm::Type::getInt32Ty(context_), value.as_u64());
                        case PointerWidth::x64:
                            return llvm::ConstantInt::get(llvm::Type::getInt64Ty(context_), value.as_u64());
                        default:
                            UNREACHABLE("Invalid pointer size");
                    }
                case ConstantValue::Kind::f32:
                    return llvm::ConstantFP::get(llvm::Type::getFloatTy(context_), value.as_f32());
                case ConstantValue::Kind::f64:
                    return llvm::ConstantFP::get(llvm::Type::getDoubleTy(context_), value.as_f64());
                case ConstantValue::Kind::str:
                    return builder_.CreateGlobalString(value.as_str().view());
                default:
                    UNREACHABLE("Invalid constant value kind");
            }
        }

        void emit_statement(const BoundStatement &statement, FunctionEmissionContext &context)
        {
            visit(statement,
                  Overload{[&](const BoundBlock &block) { emit_block(block, context); },
                           [&](const BoundVariableDeclaration &declaration) { emit_local(declaration, context); },
                           [&](const BoundExpressionStatement &expression)
                           { emit_expression_statement(expression, context); },
                           [&](const BoundReturnStatement &return_statement)
                           {
                               emit_return(return_statement, context);
                           }});
        }

        void emit_block(const BoundBlock &block, FunctionEmissionContext &context)
        {
            context.push_scope();
            for (auto statement : block.statements())
            {
                emit_statement(statement, context);
            }

            context.pop_scope();
        }

        void emit_local(const BoundVariableDeclaration &declaration, FunctionEmissionContext &context)
        {
            auto &symbol = declaration.symbol();
            if (!symbol.is_mutable())
            {
                auto *value = emit_expression(*declaration.initializer(), context);
                context.bind_storage(symbol, value);
                return;
            }

            auto *type = get_or_create_type(symbol.type());
            auto *slot = create_entry_alloca(type, symbol.name(), context);

            context.bind_storage(symbol, slot);

            if (declaration.initializer().has_value())
            {
                auto *value = emit_expression(*declaration.initializer(), context);
                builder_.CreateStore(value, slot);
            }
        }

        void emit_expression_statement(const BoundExpressionStatement &statement, FunctionEmissionContext &context)
        {
            std::ignore = emit_expression(statement.expression(), context);
        }

        void emit_return(const BoundReturnStatement &return_statement, FunctionEmissionContext &context)
        {
            if (!return_statement.expression().has_value())
            {
                builder_.CreateRetVoid();
                return;
            }

            auto *expression = emit_expression(*return_statement.expression(), context);
            builder_.CreateRet(expression);
        }

        llvm::AllocaInst *create_entry_alloca(llvm::Type *type, const Name name, FunctionEmissionContext &context)
        {
            auto &entry = context.function()->getEntryBlock();
            llvm::IRBuilder entry_builder{context_};
            entry_builder.SetInsertPoint(&entry, entry.begin());

            return builder_.CreateAlloca(type, nullptr, name.as_string_view());
        }

        llvm::Value *emit_expression(const BoundExpression &expression, FunctionEmissionContext &context)
        {
            return visit(
                expression,
                Overload{[](const BoundBadExpression &bad) -> llvm::Value *
                         { throw InvalidStateException{"Should only emit LLVM IR, if the compilation is valid."}; },
                         [&](const BoundLiteral &literal) { return emit_literal(literal, context); },
                         [&](const BoundVariableAccess &access) { return emit_access(access, context); },
                         [&](const BoundParameterAccess &access) { return emit_access(access, context); },
                         [&](const BoundUnaryExpression &unary) { return emit_operation(unary, context); },
                         [&](const BoundBinaryExpression &binary) { return emit_operation(binary, context); },
                         [&](const BoundAssignmentExpression &assignment)
                         { return emit_assignment(assignment, context); },
                         [&](const BoundConditionalExpression &conditional)
                         { return emit_conditional(conditional, context); },
                         [&](const BoundCallExpression &call) { return emit_call(call, context); },
                         [&](const BoundConversionExpression &conversion)
                         {
                             return emit_conversion(conversion, context);
                         }});
        }

        llvm::Value *emit_literal(const BoundLiteral &literal, FunctionEmissionContext &context)
        {
            return make_constant(literal.value());
        }

        llvm::Value *emit_access(const BoundVariableAccess &access, FunctionEmissionContext &context)
        {
            auto *val = emit_access_core(access, context);
            if (!access.symbol().is_mutable() && !access.symbol().is_global())
                return val;

            auto *type = get_or_create_type(access.symbol().type());
            return builder_.CreateLoad(type, val);
        }

        llvm::Value *emit_access(const BoundParameterAccess &access, FunctionEmissionContext &context)
        {
            auto *val = emit_access_core(access, context);
            if (!access.symbol().is_mutable())
                return val;

            auto *type = get_or_create_type(access.symbol().type());
            return builder_.CreateLoad(type, val);
        }

        llvm::Value *emit_operation(const BoundUnaryExpression &operation, FunctionEmissionContext &context)
        {
            switch (operation.operation())
            {
                case UnaryOperation::identity:
                    return emit_expression(operation.operand(), context);
                case UnaryOperation::negation:
                    {
                        auto *operand = emit_expression(operation.operand(), context);
                        if (is_integer(operation.operand().type().special_type()))
                            return builder_.CreateNeg(operand);

                        return builder_.CreateFNeg(operand);
                    }
                case UnaryOperation::logical_not:
                case UnaryOperation::bitwise_not:
                    return builder_.CreateNot(emit_expression(operation.operand(), context));
                case UnaryOperation::pre_increment:
                    return emit_unary_increment_decrement(operation,
                                                          UnaryReturnType::prefix,
                                                          UnaryArithmeticType::increment,
                                                          context);
                case UnaryOperation::pre_decrement:
                    return emit_unary_increment_decrement(operation,
                                                          UnaryReturnType::prefix,
                                                          UnaryArithmeticType::decrement,
                                                          context);
                case UnaryOperation::post_increment:
                    return emit_unary_increment_decrement(operation,
                                                          UnaryReturnType::postfix,
                                                          UnaryArithmeticType::increment,
                                                          context);
                case UnaryOperation::post_decrement:
                    return emit_unary_increment_decrement(operation,
                                                          UnaryReturnType::postfix,
                                                          UnaryArithmeticType::decrement,
                                                          context);
                default:
                    UNREACHABLE("Invalid unary operation");
            }
        }

        llvm::Value *emit_unary_increment_decrement(const BoundUnaryExpression &operation,
                                                    UnaryReturnType return_type,
                                                    UnaryArithmeticType direction,
                                                    FunctionEmissionContext &context)
        {
            auto *operand = emit_address(operation.operand(), context);
            auto *type = get_or_create_type(operation.operand().type());
            auto *value = builder_.CreateLoad(type, operand);
            llvm::Value *updated;
            switch (direction)
            {
                case UnaryArithmeticType::increment:
                    updated = builder_.CreateAdd(value, llvm::ConstantInt::get(value->getType(), 1));
                    break;
                case UnaryArithmeticType::decrement:
                    updated = builder_.CreateSub(value, llvm::ConstantInt::get(value->getType(), 1));
                    break;
                default:
                    UNREACHABLE("Invalid UnaryArithmeticType");
            }
            builder_.CreateStore(updated, operand);
            switch (return_type)
            {
                case UnaryReturnType::prefix:
                    return value;
                case UnaryReturnType::postfix:
                    return updated;
                default:
                    UNREACHABLE("Invalid UnaryReturnType");
            }
        }

        llvm::Value *emit_operation(const BoundBinaryExpression &operation, FunctionEmissionContext &context)
        {
            auto *left = emit_expression(operation.left(), context);
            auto *right = emit_expression(operation.right(), context);

            return emit_binary_operation(operation.type(), left, right, operation.operation());
        }

        llvm::Value *emit_binary_operation(const TypeSymbol &type,
                                           llvm::Value *left,
                                           llvm::Value *right,
                                           BinaryOperation operation)
        {
            switch (operation)
            {
                case BinaryOperation::addition:
                    if (is_integer(type.special_type()))
                        return builder_.CreateAdd(left, right);

                    return builder_.CreateFAdd(left, right);
                case BinaryOperation::subtraction:
                    if (is_integer(type.special_type()))
                        return builder_.CreateSub(left, right);

                    return builder_.CreateFSub(left, right);
                case BinaryOperation::multiplication:
                    if (is_integer(type.special_type()))
                        return builder_.CreateMul(left, right);

                    return builder_.CreateFMul(left, right);
                case BinaryOperation::division:
                    if (is_signed_integer(type.special_type()))
                        return builder_.CreateSDiv(left, right);

                    if (is_unsigned_integer(type.special_type()))
                        return builder_.CreateUDiv(left, right);

                    return builder_.CreateFDiv(left, right);
                case BinaryOperation::modulo:
                    if (is_signed_integer(type.special_type()))
                        return builder_.CreateSRem(left, right);

                    if (is_unsigned_integer(type.special_type()))
                        return builder_.CreateURem(left, right);

                    return builder_.CreateFRem(left, right);
                case BinaryOperation::bitwise_and:
                    return builder_.CreateAnd(left, right);
                case BinaryOperation::bitwise_or:
                    return builder_.CreateOr(left, right);
                case BinaryOperation::bitwise_xor:
                    return builder_.CreateXor(left, right);
                case BinaryOperation::logical_and:
                    return builder_.CreateLogicalAnd(left, right);
                case BinaryOperation::logical_or:
                    return builder_.CreateLogicalOr(left, right);
                case BinaryOperation::equals:
                    return builder_.CreateICmpEQ(left, right);
                case BinaryOperation::not_equals:
                    return builder_.CreateICmpNE(left, right);
                case BinaryOperation::less_than:
                    if (is_signed_integer(type.special_type()))
                        return builder_.CreateICmpSLT(left, right);

                    if (is_unsigned_integer(type.special_type()))
                        return builder_.CreateICmpULT(left, right);

                    return builder_.CreateFCmpOLT(left, right);
                case BinaryOperation::less_than_or_equals:
                    if (is_signed_integer(type.special_type()))
                        return builder_.CreateICmpSLE(left, right);

                    if (is_unsigned_integer(type.special_type()))
                        return builder_.CreateICmpULE(left, right);

                    return builder_.CreateFCmpOLE(left, right);
                case BinaryOperation::greater_than:
                    if (is_signed_integer(type.special_type()))
                        return builder_.CreateICmpSGT(left, right);

                    if (is_unsigned_integer(type.special_type()))
                        return builder_.CreateICmpUGT(left, right);

                    return builder_.CreateFCmpOGT(left, right);
                case BinaryOperation::greater_than_or_equals:
                    if (is_signed_integer(type.special_type()))
                        return builder_.CreateICmpSGE(left, right);

                    if (is_unsigned_integer(type.special_type()))
                        return builder_.CreateICmpUGE(left, right);

                    return builder_.CreateFCmpOGE(left, right);
                case BinaryOperation::three_way_comparison:
                    // TODO: Three-way comparisons are not fully supported yet
                    throw NotImplementedException{};
                case BinaryOperation::shift_left:
                    return builder_.CreateShl(left, right);
                case BinaryOperation::shift_right:
                    return builder_.CreateAShr(left, right);
                case BinaryOperation::unsigned_shift_right:
                    return builder_.CreateLShr(left, right);
                default:
                    UNREACHABLE("Unknown binary operation");
            }
        }

        llvm::Value *emit_assignment(const BoundAssignmentExpression &operation, FunctionEmissionContext &context)
        {
            auto *assignee = emit_address(operation.left(), context);
            auto &type = operation.left().type();
            auto *value = emit_expression(operation, context);
            if (operation.operation() == AssignmentOperation::simple)
            {
                builder_.CreateStore(value, assignee);
            }
            else
            {
                auto binary_op = to_binary_operation(operation.operation());
                emit_assignment(assignee,
                                type,
                                value,
                                [&](llvm::Value *left, llvm::Value *right)
                                { return emit_binary_operation(type, left, right, binary_op); });
            }

            // Assignments do not return a value
            return nullptr;
        }

        template <std::invocable<llvm::Value *, llvm::Value *> Functor>
            requires std::convertible_to<std::invoke_result_t<Functor, llvm::Value *, llvm::Value *>, llvm::Value *>
        void emit_assignment(llvm::Value *assignee,
                             const TypeSymbol &assignee_type,
                             llvm::Value *value,
                             Functor &&functor)
        {
            auto *type = get_or_create_type(assignee_type);
            auto *assignee_value = builder_.CreateLoad(type, assignee);
            llvm::Value *result = std::invoke(std::forward<Functor>(functor), assignee_value, value);
            builder_.CreateStore(result, assignee);
        }

        llvm::Value *emit_conditional(const BoundConditionalExpression &conditional, FunctionEmissionContext &context)
        {
            auto *function = context.function();
            auto *then_block = llvm::BasicBlock::Create(context_, "cond.then", function);
            auto *else_block = llvm::BasicBlock::Create(context_, "cond.else");
            auto *merge_block = llvm::BasicBlock::Create(context_, "cond.merge");

            auto *condition = emit_expression(conditional.condition(), context);
            condition = convert_byte_bool_to_i1_if_needed(condition);
            builder_.CreateCondBr(condition, then_block, else_block);

            builder_.SetInsertPoint(then_block);
            auto *then_value = emit_expression(conditional.when_true(), context);
            auto *actual_then_block = builder_.GetInsertBlock();
            builder_.CreateBr(merge_block);

            function->insert(function->end(), else_block);
            builder_.SetInsertPoint(else_block);
            auto *else_value = emit_expression(conditional.when_false(), context);
            auto *actual_else_block = builder_.GetInsertBlock();
            builder_.CreateBr(merge_block);

            function->insert(function->end(), merge_block); // if needed
            builder_.SetInsertPoint(merge_block);

            auto *result_type = get_or_create_type(conditional.type());
            auto *phi = builder_.CreatePHI(result_type, 2);
            phi->addIncoming(then_value, actual_then_block);
            phi->addIncoming(else_value, actual_else_block);

            return phi;
        }

        llvm::Value *convert_byte_bool_to_i1_if_needed(llvm::Value *value)
        {
            if (value->getType()->isIntegerTy(8))
                return builder_.CreateICmpNE(value, builder_.getInt8(0));

            return value;
        }

        llvm::Value *emit_call(const BoundCallExpression &call, FunctionEmissionContext &context)
        {
            const auto &target = call.symbol();
            auto *callee = get_or_create_function(target);

            auto arguments =
                call.arguments() |
                std::views::transform([&](const auto &argument) { return emit_expression(argument, context); }) |
                std::ranges::to<std::vector>();

            return builder_.CreateCall(callee, arguments);
        }

        llvm::Value *emit_conversion(const BoundConversionExpression &conversion, FunctionEmissionContext &context)
        {
            auto *operand = emit_expression(conversion.operand(), context);

            const auto &source_type = conversion.operand().type();
            const auto &target_type = conversion.type();

            if (&source_type == &target_type)
                return operand;

            return emit_scalar_conversion(operand, conversion.conversion(), source_type, target_type);
        }

        llvm::Value *emit_scalar_conversion(llvm::Value *operand,
                                            const Conversion &conversion,
                                            const TypeSymbol &source_type,
                                            const TypeSymbol &target_type)
        {
            auto *source = get_or_create_type(source_type);
            auto *target = get_or_create_type(target_type);

            if (source == target)
                return operand;

            if (conversion.is_numeric())
            {
                if (is_integer(source_type.special_type()) && is_integer(target_type.special_type()))
                {
                    const auto *source_int = llvm::cast<llvm::IntegerType>(source);
                    const auto *dest_int = llvm::cast<llvm::IntegerType>(target);

                    if (source_int->getBitWidth() < dest_int->getBitWidth())
                    {
                        if (is_signed_integer(source_type.special_type()))
                            return builder_.CreateSExt(operand, target);

                        return builder_.CreateZExt(operand, target);
                    }

                    DEBUG_ASSERT(source_int->getBitWidth() > dest_int->getBitWidth());
                    return builder_.CreateTrunc(operand, target);
                }

                if (is_floating_point(source_type.special_type()) && is_floating_point(target_type.special_type()))
                {
                    if (source_type.special_type() == SpecialType::f32)
                    {
                        DEBUG_ASSERT(target_type.special_type() == SpecialType::f64);
                        return builder_.CreateFPExt(operand, target);
                    }

                    DEBUG_ASSERT(source_type.special_type() == SpecialType::f64);
                    DEBUG_ASSERT(target_type.special_type() == SpecialType::f32);
                    return builder_.CreateFPTrunc(operand, target);
                }

                if (is_floating_point(source_type.special_type()) && is_integer(target_type.special_type()))
                {
                    if (is_signed_integer(target_type.special_type()))
                    {
                        return builder_.CreateFPToSI(operand, target);
                    }

                    return builder_.CreateFPToUI(operand, target);
                }

                if (is_integer(source_type.special_type()) && is_floating_point(target_type.special_type()))
                {
                    if (is_signed_integer(source_type.special_type()))
                    {
                        return builder_.CreateSIToFP(operand, target);
                    }

                    return builder_.CreateUIToFP(operand, target);
                }
            }

            if (conversion.is_character())
            {
                const auto *source_int = llvm::cast<llvm::IntegerType>(source);
                const auto *dest_int = llvm::cast<llvm::IntegerType>(target);

                if (source_int->getBitWidth() < dest_int->getBitWidth())
                {
                    return builder_.CreateZExt(operand, target);
                }

                DEBUG_ASSERT(source_int->getBitWidth() > dest_int->getBitWidth());
                return builder_.CreateTrunc(operand, target);
            }

            UNREACHABLE("If we get here, the conversion is invalid");
        }

        llvm::Value *emit_address(const BoundExpression &expression, FunctionEmissionContext &context)
        {
            return visit(expression,
                         Overload{
                             [&](const BoundVariableAccess &access) { return emit_access_core(access, context); },
                             [&](const BoundParameterAccess &access) { return emit_access_core(access, context); },
                             [&](const BoundExpression &) -> llvm::Value *
                             { throw std::invalid_argument{"Cannot take address of non-addressable expression"}; },
                         });
        }

        llvm::Value *emit_access_core(const BoundVariableAccess &access, FunctionEmissionContext &context)
        {
            if (auto *local = context.lookup_storage(access.symbol()); local != nullptr)
                return local;

            return get_or_create_global(access.symbol());
        }

        static llvm::Value *emit_access_core(const BoundParameterAccess &access, FunctionEmissionContext &context)
        {
            return context.lookup_storage(access.symbol());
        }

        llvm::Error write_ir() const
        {
            std::error_code ec;

            const auto target_path = options_.output_directory / std::format("{}.ll", compilation_.assembly_name());
            llvm::raw_fd_ostream output{target_path.string(), ec, llvm::sys::fs::OF_Text};
            if (ec)
                return llvm::make_error<llvm::StringError>(ec.message(), llvm::inconvertibleErrorCode());

            if (ec)
            {
                return llvm::errorCodeToError(ec);
            }

            module_.print(output, nullptr);

            return llvm::Error::success();
        }

        EmitResult output_binary()
        {
            llvm::InitializeAllTargetInfos();
            llvm::InitializeAllTargets();
            llvm::InitializeAllTargetMCs();
            llvm::InitializeAllAsmParsers();
            llvm::InitializeAllAsmPrinters();

            const auto triple_string = get_llvm_triple(compilation_.target_settings());
            std::string error;
            auto *target = llvm::TargetRegistry::lookupTarget(triple_string, error);
            if (target == nullptr)
            {
                // TODO: Emit a diagnostic
                return EmitResult::failure();
            }

            constexpr static std::string_view cpu = "generic";
            constexpr static std::string_view features = "";
            llvm::TargetOptions options;
            auto target_machine = std::unique_ptr<llvm::TargetMachine>{
                target->createTargetMachine(triple_string, cpu, features, options, llvm::Reloc::PIC_)};

            if (target_machine == nullptr)
            {
                // TODO: Emit diagnostic
                return EmitResult::failure();
            }

            module_.setTargetTriple(triple_string);
            module_.setDataLayout(target_machine->createDataLayout());

            std::error_code ec;
            const auto output_filename =
                options_.output_directory / std::format("{}.obj", compilation_.assembly_name());
            llvm::raw_fd_ostream output_stream(output_filename.string(), ec, llvm::sys::fs::OF_None);
            if (ec)
            {
                llvm::errs() << "Compiler OS Error: Could not open output file: " << ec.message() << "\n";
                return EmitResult::failure();
            }

            llvm::legacy::PassManager pass_manager;
            static constexpr auto file_type = llvm::CodeGenFileType::ObjectFile;
            if (target_machine->addPassesToEmitFile(pass_manager, output_stream, nullptr, file_type))
            {
                llvm::errs() << "LLVM Error: Target machine cannot emit an object file for this configuration.\n";
                return EmitResult::failure();
            }

            pass_manager.run(module_);
            output_stream.flush();

            return link_binary(output_filename);
        }

        EmitResult link_binary(const std::filesystem::path &obj_file)
        {
            auto output_filename = options_.output_directory / compilation_.assembly_name().as_string_view();
            std::vector<std::string> args_storage;
            args_storage.emplace_back("lld-link");

            auto &settings = compilation_.target_settings();
            if (settings.is_application())
            {
                args_storage.emplace_back("/subsystem:console");
                args_storage.emplace_back("/entry:main");
                args_storage.push_back(std::format("/out:{}.exe", output_filename.string()));
            }
            else if (settings.is_shared_library())
            {
                args_storage.emplace_back("/dll");
                args_storage.push_back(std::format("/out:{}.dll", output_filename.string()));
            }
            else
            {
                args_storage.push_back(std::format("/out:{}.lib", output_filename.string()));
            }

            // TODO: Add functionality to automatically decect MSVC and the Win32 SDK. This also only applied when
            // compiler for Windows on Windows.
            static constexpr std::string_view msvc_ver = "14.51.36231";
            static constexpr std::string_view sdk_ver = "10.0.22621.0";

            static const auto msvc_lib =
                std::format("C:/Program Files/Microsoft Visual Studio/18/Community/VC/Tools/MSVC/{}/lib/x64", msvc_ver);
            static const auto ucrt_lib = std::format("C:/Program Files (x86)/Windows Kits/10/Lib/{}/ucrt/x64", sdk_ver);
            static const auto um_lib = std::format("C:/Program Files (x86)/Windows Kits/10/Lib/{}/um/x64", sdk_ver);

            args_storage.emplace_back("/libpath:\"" + msvc_lib + "\"");
            args_storage.emplace_back("/libpath:\"" + ucrt_lib + "\"");
            args_storage.emplace_back("/libpath:\"" + um_lib + "\"");

            args_storage.push_back(obj_file.string());
            args_storage.emplace_back("libcmt.lib");
            args_storage.emplace_back("ucrt.lib");
            args_storage.emplace_back("kernel32.lib");

            const auto args = args_storage | std::views::transform([](const std::string &s) { return s.c_str(); }) |
                              std::ranges::to<std::vector>();

            llvm::raw_os_ostream out(std::cout);
            llvm::raw_os_ostream err(std::cerr);

            auto [retCode, canRunAgain] = lld::lldMain(args, out, err, LLD_ALL_DRIVERS);

            return EmitResult{.is_success = retCode == 0};
        }

        const Compilation &compilation_;
        LlvmCodeGenOptions options_;
        llvm::LLVMContext context_;
        llvm::Module module_;
        llvm::IRBuilder<> builder_{context_};

        std::unordered_map<const Symbol *, llvm::Value *> symbol_to_value_{};
        std::unordered_map<const TypeSymbol *, llvm::Type *> symbol_to_type_{};
    };

    LlvmEmitter::LlvmEmitter(const Compilation &compilation, LlvmCodeGenOptions options)
        : impl_{std::make_unique<Impl>(compilation, std::move(options))}
    {
    }

    LlvmEmitter::~LlvmEmitter() = default;

    EmitResult LlvmEmitter::emit() const
    {
        return impl_->emit();
    }
} // namespace prism
