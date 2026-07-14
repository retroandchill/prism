/**
 * @file boost_deps.hpp
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
#pragma once

#include <boost/core/allocator_access.hpp>
#include <boost/core/allocator_traits.hpp>
#include <boost/core/bit.hpp>
#include <boost/core/checked_delete.hpp>
#include <boost/core/cmath.hpp>
#include <boost/core/default_allocator.hpp>
#include <boost/core/demangle.hpp>
#include <boost/core/empty_value.hpp>
#include <boost/core/enable_if.hpp>
#include <boost/core/exchange.hpp>
#include <boost/core/fclose_deleter.hpp>
#include <boost/core/first_scalar.hpp>
#include <boost/core/functor.hpp>
#include <boost/core/identity.hpp>
#include <boost/core/ignore_unused.hpp>
#include <boost/core/is_same.hpp>
#include <boost/core/launder.hpp>
#include <boost/core/lightweight_test.hpp>
#include <boost/core/make_span.hpp>
#include <boost/core/max_align.hpp>
#include <boost/core/memory_resource.hpp>
#include <boost/core/noinit_adaptor.hpp>
#include <boost/core/noncopyable.hpp>
#include <boost/core/null_deleter.hpp>
#include <boost/core/nvp.hpp>
#include <boost/core/pointer_in_range.hpp>
#include <boost/core/pointer_traits.hpp>
#include <boost/core/quick_exit.hpp>
#include <boost/core/ref.hpp>
#include <boost/core/serialization.hpp>
#include <boost/core/size.hpp>
#include <boost/core/span.hpp>
#include <boost/core/swap.hpp>
#include <boost/core/type_name.hpp>
#include <boost/core/typeinfo.hpp>
#include <boost/core/uncaught_exceptions.hpp>
#include <boost/core/use_default.hpp>
#include <boost/core/verbose_terminate_handler.hpp>
#include <boost/core/yield_primitives.hpp>
#include <boost/is_placeholder.hpp>
#include <boost/multiprecision/cpp_bin_float.hpp>
#include <boost/multiprecision/cpp_dec_float.hpp>
#include <boost/multiprecision/cpp_double_fp.hpp>
#include <boost/multiprecision/cpp_int.hpp>
#include <boost/multiprecision/number.hpp>
#if __has_include(<gmp.hpp>)
#include <boost/multiprecision/gmp.hpp>
#endif
#if __has_include(<tommath.hpp>)
#include <boost/multiprecision/tommath.hpp>
#endif
#if __has_include(<mpfr.hpp>)
#include <boost/multiprecision/mpfr.hpp>
#endif
#include <boost/container/adaptive_pool.hpp>
#include <boost/container/allocator.hpp>
#include <boost/container/allocator_traits.hpp>
#include <boost/container/container_fwd.hpp>
#include <boost/container/deque.hpp>
#include <boost/container/flat_map.hpp>
#include <boost/container/flat_set.hpp>
#include <boost/container/list.hpp>
#include <boost/container/map.hpp>
#include <boost/container/node_allocator.hpp>
#include <boost/container/options.hpp>
#include <boost/container/scoped_allocator.hpp>
#include <boost/container/scoped_allocator_fwd.hpp>
#include <boost/container/set.hpp>
#include <boost/container/slist.hpp>
#include <boost/container/small_vector.hpp>
#include <boost/container/stable_vector.hpp>
#include <boost/container/static_vector.hpp>
#include <boost/container/string.hpp>
#include <boost/container/throw_exception.hpp>
#include <boost/container/vector.hpp>
#include <boost/multiprecision/miller_rabin.hpp>
