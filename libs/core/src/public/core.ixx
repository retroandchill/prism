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
export import :memory.persistent_allocator;

export import :text.name;
export import :text.text_span;
export import :text.source_file;

export import :type_traits.basic;
export import :type_traits.comparison;

export import :util.enum_class_flags;
export import :util.make_array;
export import :util.noncopyable;
export import :util.optional;
export import :util.hashing;
export import :util.numerics;

export import :diagnostics.diagnostic_descriptor;

export import :syntax.kind;
export import :syntax.literals;
export import :syntax.token;
export import :syntax.trivia;
