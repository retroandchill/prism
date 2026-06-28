set(PRISM_HEADERS include/prism/core/exports.h)

set(PRISM_SOURCES
        src/private/lexer/lexer.cpp
        src/private/parser/parser.cpp
        src/private/lexer/token_stream.cpp
        src/private/semantic/symbol.cpp
        src/private/strings/identifier.cpp)

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
        src/public/generated.ixx
        src/public/lexer/token_stream.ixx
        src/public/diagnostic.ixx
        src/public/util.ixx
        src/public/ast/common_syntax.ixx
        src/public/semantic/symbol.ixx
        src/public/ast/syntax_node.ixx
        src/public/strings/identifier.ixx
)

find_package(antlr4-runtime REQUIRED)
find_package(antlr4-generator REQUIRED)

set(ANTLR4_JAR_LOCATION ${TOOLS_DIR}/antlr.jar)
file(DOWNLOAD
        https://www.antlr.org/download/antlr-${antlr4-runtime_VERSION}-complete.jar
        ${ANTLR4_JAR_LOCATION}
        SHOW_PROGRESS
)

antlr4_generate(prism_parser ${CMAKE_SOURCE_DIR}/grammar/Prism.g4 BOTH false true prism::gen)

add_library(prism_core ${PRISM_HEADERS} ${PRISM_SOURCES} ${PRISM_PUBLIC_MODULES} ${ANTLR4_SRC_FILES_prism_parser})

target_include_directories(prism_core PUBLIC include
                                      PRIVATE
                                            ${ANTLR4_INCLUDE_DIR_prism_parser})

target_link_libraries(prism_core
        PUBLIC Boost::cppm
        PRIVATE antlr4::cppm)

target_sources(prism_core PUBLIC
        FILE_SET prism_core_public
        TYPE CXX_MODULES
        FILES
        ${PRISM_PUBLIC_MODULES})

target_compile_definitions(prism_core PRIVATE PRISM_CORE_EXPORT)

if (PRISM_BUILD_TESTS)
    add_subdirectory(tests)
endif()
