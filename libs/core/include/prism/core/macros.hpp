/**
 * @file macros.hpp
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
#pragma once

#ifdef _MSC_VER
#define PRISM_NO_UNIQUE_ADDRESS [[msvc::no_unique_address]]
#else
#define MY_NO_UNIQUE_ADDRESS [[no_unique_address]]
#endif
