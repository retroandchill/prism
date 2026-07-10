/**
 * @file core.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core;

export import :memory.alignment;
export import :memory.arena_allocator;
export import :memory.buffer_pool;
export import :memory.ref_counted_ptr;

export import :strings.name;

export import :type_traits.basic;

export import :util.enum_class_flags;
export import :util.make_array;
export import :util.noncopyable;

export import :diagnostics.diagnostic_descriptor;

export import :syntax.kind;
