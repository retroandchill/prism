add_library(antlr4_cppm OBJECT runtime.ixx)


find_package(antlr4-runtime REQUIRED)
target_link_libraries(antlr4_cppm PUBLIC antlr4_shared)

target_sources(antlr4_cppm PUBLIC
        FILE_SET CXX_MODULES
        FILES
        runtime.ixx)

add_library(antlr4::cppm ALIAS antlr4_cppm)
