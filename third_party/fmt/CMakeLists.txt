# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(fmt_cppm OBJECT)

target_sources(fmt_cppm PUBLIC
        FILE_SET CXX_MODULES FILES
        fmt.ixx)


find_package(fmt CONFIG REQUIRED)
target_link_libraries(fmt_cppm PUBLIC fmt::fmt)

add_library(fmt::cppm ALIAS fmt_cppm)
