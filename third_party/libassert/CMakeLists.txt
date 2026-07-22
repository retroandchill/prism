# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(libassert_cppm OBJECT)

target_sources(libassert_cppm PUBLIC
        FILE_SET CXX_MODULES FILES
        libassert.cppm)


find_package(libassert CONFIG REQUIRED)
target_link_libraries(libassert_cppm PUBLIC libassert::assert)

add_library(libassert::cppm ALIAS libassert_cppm)
