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
export import :memory.default_allocator;

export import :collections.immutable_array;
export import :collections.immutable_small_array;
export import :collections.immutable_hash_set;
export import :collections.immutable_hash_map;

export import :text.name;
export import :text.text_span;
export import :text.source_file;
export import :text.writer;

export import :type_traits.basic;
export import :type_traits.comparison;
export import :type_traits.visitor;

export import :util.enum_class_flags;
export import :util.make_array;
export import :util.noncopyable;
export import :util.optional;
export import :util.hashing;
export import :util.numerics;
export import :util.overload;
export import :util.enum_set;
export import :util.lazy;
export import :util.exceptions;

export import :diagnostics.descriptor;
export import :diagnostics.registry;

export import :context.target_settings;

export import :syntax.kind;
export import :syntax.literals;
export import :syntax.token;
export import :syntax.trivia;
export import :syntax.list;
export import :syntax.node_or_token;
export import :syntax.child_list;

export import :symbols.symbol;
export import :symbols.variable_symbol;
export import :symbols.type_symbol;
export import :symbols.function_symbol;
export import :symbols.parameter_symbol;

export import :semantic.compilation;
export import :semantic.semantic_model;
export import :semantic.lookup_result;
