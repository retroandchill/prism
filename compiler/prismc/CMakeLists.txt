add_executable(prismc main.cpp)
prism_add_boilerplate(prismc)

set(sample_program "${CMAKE_CURRENT_SOURCE_DIR}/../../examples/hello_world/main.pr")

cmake_path(ABSOLUTE_PATH sample_program NORMALIZE OUTPUT_VARIABLE SAMPLE_PROGRAM_PATH)

target_compile_definitions(prismc PRIVATE
    SAMPLE_PROGRAM="${SAMPLE_PROGRAM_PATH}")

target_link_libraries(prismc PRIVATE prism_core)
