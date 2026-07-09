set(PRISM_HEADERS include/prism/core/exports.h
        include/prism/core/per_project_boilerplate.hpp
        include/prism/core/names.hpp
        include/prism/core/syntax.hpp
)

set(PRISM_SOURCES
        src/private/strings/name.cpp
        src/private/memory/buffer_pool.cpp
)

set(PRISM_PRIVATE_MODULES )

set(PRISM_PUBLIC_MODULES
        src/public/util.ixx
        src/public/strings/name.ixx
        src/public/memory/alignment.ixx
        src/public/memory/arena_allocator.ixx
        src/public/memory/persistent_allocator.ixx
        src/public/memory/buffer_pool.ixx
        src/public/syntax.ixx
        src/public/syntax/kind.ixx
)

add_library(prism_core ${PRISM_HEADERS} ${PRISM_SOURCES} ${PRISM_PRIVATE_MODULES} ${PRISM_PUBLIC_MODULES})
prism_add_boilerplate(prism_core)

target_include_directories(prism_core PUBLIC include
                                      PRIVATE src/private/inline
                                              src/private)

find_package(Boost CONFIG REQUIRED COMPONENTS preprocessor)
target_link_libraries(prism_core
        PRIVATE xxHash::cpp
                matchit::cppm
                Boost::preprocessor)

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
