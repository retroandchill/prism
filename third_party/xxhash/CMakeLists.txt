set(XXHASH_MODULES xxhash.ixx)

add_library(xxhash_cpp OBJECT ${XXHASH_MODULES})

find_package(xxHash CONFIG REQUIRED)
target_link_libraries(xxhash_cpp PUBLIC xxHash::xxhash)

target_sources(xxhash_cpp PUBLIC
        FILE_SET CXX_MODULES
        FILES
        ${XXHASH_MODULES})

add_library(xxHash::cpp ALIAS xxhash_cpp)
