/**
 * @file llvm_emitter.cpp
 * @author Francesco Corso
 * @date 8/22/2026
 * @brief
 */
module;

#include <libassert/assert-macros.hpp>
#include <llvm/IR/IRBuilder.h>
#include <llvm/IR/LLVMContext.h>
#include <llvm/IR/Module.h>

module prism.core:codegen.llvm_emitter.impl;

import :codegen.llvm_emitter;
import :semantic.compilation;
import :symbols.visit;

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
            for (auto &global : CompilationInternal::get_global_variables(compilation_))
            {
                get_or_create_global(global);
            }

            for (auto &function : CompilationInternal::get_functions(compilation_))
            {
                get_or_create_function(function);
            }

            for (const auto &global : CompilationInternal::get_global_variables(compilation_))
                emit_global_initializer(global);

            for (const auto &function : CompilationInternal::get_functions(compilation_))
                emit_function_body(function);
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
            auto *variable =
                new llvm::GlobalVariable(type, false, llvm::GlobalVariable::ExternalLinkage, nullptr, name);
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

        void emit_global_initializer(const VariableSymbol &symbol)
        {
        }

        void emit_function_body(const FunctionSymbol &symbol)
        {
        }

        llvm::Type *create_type(const TypeSymbol &symbol)
        {
            switch (symbol.special_type())
            {
                case SpecialType::void_:
                    return llvm::Type::getVoidTy(context_);
                case SpecialType::bool_:
                    return llvm::Type::getInt1Ty(context_);
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
            while (current.has_value())
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

        const Compilation &compilation_;
        LlvmCodeGenOptions options_;
        llvm::LLVMContext context_;
        llvm::Module module_;
        llvm::IRBuilder<> builder_{context_};

        std::unordered_map<const Symbol *, llvm::Value *> symbol_to_value_;
        std::unordered_map<const TypeSymbol *, llvm::Type *> symbol_to_type_;
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
