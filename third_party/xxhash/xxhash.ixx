/**
 * @file xxhash.ixx
 * @author Francesco Corso
 * @date 6/30/2026
 * @brief
 */
module;

#include <xxhash.h>

export module xxhash;

/*
Discovered Macros:
- XXH128
- XXH128_canonicalFromHash
- XXH128_canonical_t
- XXH128_cmp
- XXH128_hashFromCanonical
- XXH128_hash_t
- XXH128_isEqual
- XXH32
- XXH32_canonicalFromHash
- XXH32_canonical_t
- XXH32_copyState
- XXH32_createState
- XXH32_digest
- XXH32_freeState
- XXH32_hashFromCanonical
- XXH32_reset
- XXH32_state_s
- XXH32_state_t
- XXH32_update
- XXH3_128bits
- XXH3_128bits_digest
- XXH3_128bits_reset
- XXH3_128bits_reset_withSecret
- XXH3_128bits_reset_withSecretandSeed
- XXH3_128bits_reset_withSeed
- XXH3_128bits_update
- XXH3_128bits_withSecret
- XXH3_128bits_withSecretandSeed
- XXH3_128bits_withSeed
- XXH3_64bits
- XXH3_64bits_digest
- XXH3_64bits_reset
- XXH3_64bits_reset_withSecret
- XXH3_64bits_reset_withSecretandSeed
- XXH3_64bits_reset_withSeed
- XXH3_64bits_update
- XXH3_64bits_withSecret
- XXH3_64bits_withSecretandSeed
- XXH3_64bits_withSeed
- XXH3_SECRET_SIZE_MIN
- XXH3_copyState
- XXH3_createState
- XXH3_freeState
- XXH3_generateSecret
- XXH3_generateSecret_fromSeed
- XXH3_state_s
- XXH3_state_t
- XXH64
- XXH64_canonicalFromHash
- XXH64_canonical_t
- XXH64_copyState
- XXH64_createState
- XXH64_digest
- XXH64_freeState
- XXH64_hashFromCanonical
- XXH64_reset
- XXH64_state_s
- XXH64_state_t
- XXH64_update
- XXHASH_H_5627135585666179
- XXH_AVX2
- XXH_AVX512
- XXH_C23_VN
- XXH_CAT
- XXH_CONSTF
- XXH_ERROR
- XXH_FALLTHROUGH
- XXH_HAS_ATTRIBUTE
- XXH_HAS_CPP_ATTRIBUTE
- XXH_HAS_C_ATTRIBUTE
- XXH_IMPLEMENTATION
- XXH_INLINE_ALL_31684351384
- XXH_IPREF
- XXH_LSX
- XXH_MALLOCF
- XXH_NAME2
- XXH_NAMESPACE
- XXH_NEON
- XXH_NOESCAPE
- XXH_OK
- XXH_PUBLIC_API
- XXH_PUREF
- XXH_SCALAR
- XXH_SSE2
- XXH_STATIC_LINKING_ONLY
- XXH_SVE
- XXH_VERSION_MAJOR
- XXH_VERSION_MINOR
- XXH_VERSION_NUMBER
- XXH_VERSION_RELEASE
- XXH_VSX
- XXH_errorcode
- XXH_versionNumber
*/
export using ::XXH_versionNumber;
export using ::XXH32_hash_t;
export using ::XXH32;
#ifndef XXH_NO_STREAM
export using ::XXH32_state_t;
export using ::XXH32_createState;
export using ::XXH32_freeState;
export using ::XXH32_copyState;
export using ::XXH32_reset;
export using ::XXH32_update;
export using ::XXH32_digest;
#endif
export using ::XXH32_canonicalFromHash;
export using ::XXH32_hashFromCanonical;
export using ::XXH64_hash_t;

export using ::XXH64;
#ifndef XXH_NO_STREAM
export using ::XXH64_state_t;
export using ::XXH64_createState;
export using ::XXH64_freeState;
export using ::XXH64_copyState;
export using ::XXH64_reset;
export using ::XXH64_update;
export using ::XXH64_digest;
#endif
export using ::XXH64_canonicalFromHash;
export using ::XXH64_hashFromCanonical;
#ifndef XXH_NO_XXH3
export using ::XXH3_64bits;
export using ::XXH3_64bits_withSeed;
export using ::XXH3_64bits_withSecret;
#ifndef XXH_NO_STREAM
export using ::XXH3_state_t;
export using ::XXH3_createState;
export using ::XXH3_freeState;
export using ::XXH3_copyState;
export using ::XXH3_64bits_reset;
export using ::XXH3_64bits_reset_withSeed;
export using ::XXH3_64bits_reset_withSecret;
export using ::XXH3_64bits_update;
export using ::XXH3_64bits_digest;
#endif
export using ::XXH3_128bits;
export using ::XXH3_128bits_withSeed;
export using ::XXH3_128bits_withSecret;
#ifndef XXH_NO_STREAM
export using ::XXH3_128bits_reset;
export using ::XXH3_128bits_reset_withSeed;
export using ::XXH3_128bits_reset_withSecret;
export using ::XXH3_128bits_update;
export using ::XXH3_128bits_digest;
#endif
export using ::XXH128_isEqual;
export using ::XXH128_cmp;
export using ::XXH128_canonicalFromHash;
export using ::XXH128_hashFromCanonical;
#endif
