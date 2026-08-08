set(PRISM_HEADERS include/prism/core/exports.h
        include/prism/core/per_project_boilerplate.hpp
        include/prism/core/names.hpp
)

set(PRISM_SOURCES
        src/private/text/name.cpp
        src/private/memory/buffer_pool.cpp
        src/private/syntax/green/node.cpp
        src/private/syntax/green/token.cpp
        src/private/diagnostics/info.cpp
        src/private/syntax/trivia.cpp
        src/private/parser/lexer.cpp
        src/private/syntax/green/trivia.cpp
        src/private/parser/token_stream.cpp
        generated/private/syntax/green/types.cpp
        generated/private/syntax/green/declarations.cpp
        generated/private/syntax/green/expressions.cpp
        generated/private/syntax/green/clauses.cpp
        generated/private/syntax/green/statements.cpp
        src/private/syntax/green/list.cpp
        generated/private/syntax/clauses.cpp
        generated/private/syntax/declarations.cpp
        generated/private/syntax/expressions.cpp
        generated/private/syntax/statements.cpp
        generated/private/syntax/types.cpp
        generated/private/syntax/top_level.cpp
        generated/private/syntax/green/top_level.cpp
        src/private/parser/syntax_parser.cpp
        src/private/parser/language_parser.cpp
        src/private/syntax/green/child_list.cpp
        src/private/syntax/green/view.cpp
        generated/private/syntax/green/structured_trivia.cpp
        generated/private/syntax/structured_trivia.cpp
        generated/private/syntax/names.cpp
        generated/private/syntax/green/names.cpp
        generated/private/syntax/directives.cpp
        generated/private/syntax/green/directives.cpp
        src/private/syntax/node_or_token.cpp
        src/private/symbols/merged_namespace_symbol.cpp
        src/private/binder/signature_binder.cpp
        src/private/binder/binder_helpers.cpp
        src/private/symbols/namespace_symbol.cpp
)

set(PRISM_PRIVATE_MODULES
        src/private/symbols/intrinsic.cpp)

set(PRISM_PUBLIC_MODULES
        src/public/text/name.ixx
        src/public/memory/alignment.ixx
        src/public/memory/arena_allocator.ixx
        src/public/memory/buffer_pool.ixx
        generated/public/syntax/kind.ixx
        src/public/syntax/green/node.ixx
        src/public/syntax/flags.ixx
        src/public/util/enum_class_flags.ixx
        src/public/util/noncopyable.ixx
        src/public/util/make_array.ixx
        src/public/memory/ref_counted_ptr.ixx
        src/public/type_traits/basic.ixx
        src/public/core.ixx
        src/public/diagnostics/descriptor.ixx
        src/public/diagnostics/severity.ixx
        src/public/diagnostics/info.ixx
        src/public/syntax/token.ixx
        src/public/syntax/green/token.ixx
        src/public/syntax/green/trivia.ixx
        src/public/syntax/literals.ixx
        src/public/util/optional.ixx
        src/public/util/hashing.ixx
        src/public/type_traits/comparison.ixx
        src/public/util/numerics.ixx
        src/public/syntax/trivia.ixx
        src/public/text/text_span.ixx
        src/public/text/source_file.ixx
        src/public/syntax/list_view.ixx
        src/public/parser/lexer.ixx
        src/public/text/cursor.ixx
        src/public/parser/token_stream.ixx
        src/public/syntax/node.ixx
        src/public/syntax/tree.ixx
        generated/public/syntax/green/types.ixx
        generated/public/syntax/green/declarations.ixx
        src/public/syntax/green/list.ixx
        generated/public/syntax/green/expressions.ixx
        generated/public/syntax/green/clauses.ixx
        src/public/syntax/green/separated_list.ixx
        generated/public/syntax/green/statements.ixx
        generated/public/syntax/lexing_utils.ixx
        src/public/memory/persistent_allocator.ixx
        src/public/syntax/lifetime.ixx
        src/public/util/exceptions.ixx
        src/private/syntax/tree.cpp
        src/private/syntax/node.cpp
        src/private/syntax/token.cpp
        src/private/syntax/lifetime.cpp
        src/public/syntax/list.ixx
        src/private/syntax/list.cpp
        src/public/syntax/child_list.ixx
        src/private/syntax/child_list.cpp
        src/public/util/overload.ixx
        src/public/syntax/node_or_token.ixx
        generated/public/syntax/clauses.ixx
        generated/public/syntax/declarations.ixx
        generated/public/syntax/expressions.ixx
        generated/public/syntax/statements.ixx
        generated/public/syntax/types.ixx
        src/public/parser/language_parser.ixx
        generated/public/syntax/top_level.ixx
        generated/public/syntax/green/top_level.ixx
        src/public/parser/syntax_parser.ixx
        generated/public/diagnostics/codes.ixx
        generated/public/diagnostics/registry.ixx
        src/public/diagnostics/syntax_info.ixx
        src/private/diagnostics/syntax_info.cpp
        generated/public/diagnostics/traits.ixx
        src/public/text/writer.ixx
        src/public/text/string_writer.ixx
        src/public/syntax/green/last_token_replacer.ixx
        src/public/syntax/green/first_token_replacer.ixx
        src/public/syntax/green/view.ixx
        src/public/syntax/green/child_list.ixx
        generated/public/syntax/green/structured_trivia.ixx
        generated/public/syntax/structured_trivia.ixx
        src/public/type_traits/visitor.ixx
        generated/public/syntax/green/visit.ixx
        src/public/util/enum_set.ixx
        src/public/symbols/symbol.ixx
        src/public/symbols/type_symbol.ixx
        src/public/symbols/function_symbol.ixx
        src/public/symbols/parameter_symbol.ixx
        src/public/symbols/variable_symbol.ixx
        src/public/util/ref.ixx
        src/public/symbols/kind.ixx
        src/public/symbols/assembly_symbol.ixx
        src/public/symbols/namespace_symbol.ixx
        src/public/symbols/named_type_symbol.ixx
        generated/public/syntax/green/names.ixx
        generated/public/syntax/names.ixx
        generated/public/syntax/directives.ixx
        generated/public/syntax/green/directives.ixx
        src/public/semantic/semantic_lifetime.ixx
        src/public/binder/declaration_binder.ixx
        generated/public/syntax/visit.ixx
        src/public/symbols/source.ixx
        src/private/binder/declaration_binder.cpp
        src/private/symbols/symbol.cpp
        src/public/symbols/visit.ixx
        src/public/util/lazy.ixx
        src/private/symbols/source.cpp
        src/public/semantic/compilation.ixx
        src/private/semantic/compilation.cpp
        src/public/binder/declaration_merger.ixx
        src/private/binder/declaration_merger.cpp
        src/public/binder/declaration_scope.ixx
        src/private/binder/declaration_scope.cpp
        src/public/diagnostics/diagnostic.ixx
        src/private/diagnostics/diagnostic.cpp
        src/public/diagnostics/location.ixx
        src/private/diagnostics/location.cpp
        src/public/diagnostics/diagnostic_bag.ixx
        src/private/diagnostics/diagnostic_bag.cpp
        src/public/semantic/semantic_model.ixx
        src/private/semantic/semantic_model.cpp
        src/public/syntax/reference.ixx
        src/public/binder/declaration_scope_builder.ixx
        src/private/binder/declaration_scope_builder.cpp
        src/public/binder/semantic_mappings.ixx
        src/private/binder/semantic_mappings.cpp
        src/public/symbols/merged_namespace_symbol.ixx
        src/public/symbols/intrinsic.ixx
        src/public/binder/signature_binder.ixx
        src/public/binder/binding_helpers.ixx
        src/public/symbols/error_type_symbol.ixx
        src/public/semantic/lookup_result.ixx
        src/public/semantic/bound/bound_node.ixx
        src/public/semantic/bound/bound_statement.ixx
        src/public/semantic/bound/bound_expression.ixx
        src/public/semantic/constant_value.ixx
        src/public/semantic/operations.ixx
)

add_library(prism_core ${PRISM_HEADERS} ${PRISM_SOURCES} ${PRISM_PRIVATE_MODULES} ${PRISM_PUBLIC_MODULES})
prism_add_boilerplate(prism_core)

target_include_directories(prism_core PUBLIC include
                                      PRIVATE src/private/inline
                                              src/private)

target_link_libraries(prism_core
        PUBLIC Boost::cppm
                uni-algo::cppm
                libassert::cppm
        PRIVATE xxHash::cpp)

target_sources(prism_core PUBLIC
        FILE_SET prism_core_private
        TYPE CXX_MODULES
        FILES
        ${PRISM_SOURCES}
        ${PRISM_PRIVATE_MODULES})

target_sources(prism_core PUBLIC
        FILE_SET prism_core_public
        TYPE CXX_MODULES
        FILES
        ${PRISM_PUBLIC_MODULES})

target_compile_definitions(prism_core PRIVATE PRISM_CORE_EXPORT)

if (PRISM_BUILD_TESTS)
    add_subdirectory(tests)
endif()
