/**
 * @file exports.h
 * @author Francesco Corso
 * @date 6/20/2026
 * @brief
 */
#pragma once

#ifdef PRISM_SHARED
#ifdef _WIN32
#ifdef MODULIZER_EXPORT
#define PRISM_CORE_API __declspec(dllexport)
#else
#define PRISM_CORE_API __declspec(dllimport)
#endif
#else
#define PRISM_CORE_API = __attribute__((visibility("default")))
#endif
#else
#define PRISM_CORE_API
#endif
