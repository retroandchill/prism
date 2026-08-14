set(IMMER_MODULES immer.ixx)

add_library(immer_cppm OBJECT ${IMMER_MODULES})

target_sources(immer_cppm PUBLIC
        FILE_SET CXX_MODULES FILES
        ${IMMER_MODULES})

find_package(Immer CONFIG REQUIRED)
target_link_libraries(immer_cppm PUBLIC immer)

add_library(immer::cppm ALIAS immer_cppm)
