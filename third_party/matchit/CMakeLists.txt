set(MATCHIT_MODULES matchit.ixx)

add_library(matchit_cppm OBJECT ${MATCHIT_MODULES})

find_package(matchit CONFIG REQUIRED)
target_link_libraries(matchit_cppm PUBLIC matchit::matchit)

target_sources(matchit_cppm PUBLIC
        FILE_SET CXX_MODULES
        FILES
        ${MATCHIT_MODULES})

add_library(matchit::cppm ALIAS matchit_cppm)
