set(PRISM_HEADERS include/prism/core/exports.h)

set(PRISM_SOURCES
        src/private/lexer/lexer.cpp
        src/private/parser/parser.cpp
        src/private/lexer/token_stream.cpp
        src/private/strings/name.cpp)

set(PRISM_PRIVATE_MODULES )

set(PRISM_PUBLIC_MODULES
        src/public/lexer/token.ixx
        src/public/source/source_file.ixx
        src/public/lexer/text_cursor.ixx
        src/public/lexer/lexer.ixx
        src/public/parser/token_cursor.ixx
        src/public/ast/expression_syntax.ixx
        src/public/ast/type_syntax.ixx
        src/public/ast/statement_syntax.ixx
        src/public/parser/parser.ixx
        src/public/lexer/token_stream.ixx
        src/public/diagnostic.ixx
        src/public/util.ixx
        src/public/ast/common_syntax.ixx
        src/public/strings/name.ixx
        src/public/memory/alignment.ixx
)

add_library(prism_core ${PRISM_HEADERS} ${PRISM_SOURCES} ${PRISM_PRIVATE_MODULES} ${PRISM_PUBLIC_MODULES})

target_include_directories(prism_core PUBLIC include
                                      PRIVATE src/private/inline)

target_link_libraries(prism_core
        PUBLIC Boost::cppm
        PRIVATE xxHash::cpp)

target_sources(prism_core PUBLIC
        FILE_SET prism_core_private
        TYPE CXX_MODULES
        FILES
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
