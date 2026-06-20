set(PRISM_HEADERS include/prism/core/exports.h)

set(PRISM_SOURCES
        src/private/lexer/lexer.cpp)

set(PRISM_PUBLIC_MODULES
        src/public/lexer/token.ixx
        src/public/source/source_file.ixx
        src/public/lexer/text_cursor.ixx
        src/public/lexer/lexer.ixx
)



add_library(prism_core ${PRISM_HEADERS} ${PRISM_SOURCES} ${PRISM_PUBLIC_MODULES})

target_include_directories(prism_core PUBLIC include)

target_sources(prism_core PUBLIC
        FILE_SET prism_core_public
        TYPE CXX_MODULES
        FILES
        ${PRISM_PUBLIC_MODULES})

target_compile_definitions(prism_core PRIVATE PRISM_CORE_EXPORT)

if (PRISM_BUILD_TESTS)
    add_subdirectory(tests)
endif()
