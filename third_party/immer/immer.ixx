module;

#include <immer/algorithm.hpp>
#include <immer/array.hpp>
#include <immer/array_transient.hpp>
#include <immer/atom.hpp>
#include <immer/box.hpp>
#include <immer/flex_vector.hpp>
#include <immer/flex_vector_transient.hpp>
#include <immer/map.hpp>
#include <immer/map_transient.hpp>
#include <immer/set.hpp>
#include <immer/set_transient.hpp>
#include <immer/table.hpp>
#include <immer/table_transient.hpp>
#include <immer/vector.hpp>
#include <immer/vector_transient.hpp>

export module immer;

/*
Discovered Macros:
- IMMER_ASAN_ENABLED
- IMMER_ASSERT_TAGGED
- IMMER_BROKEN_STANDARD_LAYOUT_DETECTION
- IMMER_CATCH
- IMMER_COMPILER_HAS_FEATURE
- IMMER_DEBUG_DEEP_CHECK
- IMMER_DEBUG_PRINT
- IMMER_DEBUG_STATS
- IMMER_DEBUG_TRACES
- IMMER_DESCENT_DEEP
- IMMER_ENABLE_DEBUG_SIZE_HEAP
- IMMER_FORCEINLINE
- IMMER_HAS_BUILTIN_POPCOUNT
- IMMER_HAS_CPP17
- IMMER_INVALID_STATE_ASSERT
- IMMER_LIKELY
- IMMER_NODISCARD
- IMMER_NO_EXCEPTIONS
- IMMER_PREFETCH
- IMMER_RETHROW
- IMMER_SMT_PAUSE
- IMMER_TAGGED_NODE
- IMMER_THROW
- IMMER_THROW_ON_INVALID_STATE
- IMMER_TRACE
- IMMER_TRACE_E
- IMMER_TRACE_F
- IMMER_TRY
- IMMER_UNLIKELY
- IMMER_UNREACHABLE
- immer_offsetof
*/
namespace immer
{
    export using immer::for_each_chunk;
    export using immer::for_each_chunk_p;
    namespace detail
    {
        export using detail::accumulate_move;
        export using detail::combine_standard_layout;
        export using detail::combine_standard_layout_t;
        namespace csl
        {
            export using csl::type_t;
            export using csl::get;
            export using csl::inherit;
            export using csl::member;
            export using csl::member_two;
            export using csl::combine_standard_layout_aux;
        } // namespace csl
        export using csl::get;
        export using detail::make_void;
        export using detail::void_t;
        export using detail::is_dereferenceable;
        export using detail::is_dereferenceable_v;
        export using detail::is_equality_comparable;
        export using detail::is_equality_comparable_v;
        export using detail::is_inequality_comparable;
        export using detail::is_inequality_comparable_v;
        export using detail::is_preincrementable;
        export using detail::is_preincrementable_v;
        export using detail::is_subtractable;
        export using detail::is_subtractable_v;
        namespace swappable
        {
            export using std::swap;
            export using swappable::with;
        } // namespace swappable
        export using detail::is_swappable_with;
        export using detail::is_swappable;
        export using detail::is_swappable_v;
        export using detail::is_iterator;
        export using detail::is_iterator_v;
        export using detail::compatible_sentinel;
        export using detail::compatible_sentinel_v;
        export using detail::is_forward_iterator;
        export using detail::is_forward_iterator_v;
        export using detail::std_distance_supports;
        export using detail::std_distance_supports_v;
        export using detail::as_const;
        export using detail::aligned_storage;
        export using detail::aligned_storage_t;
        export using detail::aligned_storage_for;
        export using detail::auto_const_cast;
        export using detail::destroy_at;
        export using detail::can_trivially_detroy;
        export using detail::destroy;
        export using detail::destroy_n;
        export using detail::can_trivially_copy;
        export using detail::uninitialized_move;
        export using detail::uninitialized_copy;
        export using detail::make;
        export using detail::not_supported_t;
        export using detail::empty_t;
        export using detail::exact_t;
        export using detail::clz_;
        export using detail::log2_aux;
        export using detail::log2;
        export using detail::ipow;
        export using detail::static_if;
        export using detail::constantly;
        export using detail::distance;
        namespace arrays
        {
            export using arrays::node;
            export using arrays::no_capacity;
            export using arrays::with_capacity;
        } // namespace arrays
        export using detail::unsafe_free_list_storage;
        export using detail::unsafe_free_list_heap_impl;
        export using detail::thread_local_free_list_storage;
        export using detail::gc_atom_impl;
        export using detail::refcount_atom_impl;
        namespace rbts
        {
            export using rbts::bits_t;
            export using rbts::shift_t;
            export using rbts::count_t;
            export using rbts::size_t;
            export using rbts::branches;
            export using rbts::mask;
            export using rbts::endshift;
            export using rbts::node;
            export using rbts::derive_bits_leaf_aux;
            export using rbts::derive_bits_leaf;
            export using rbts::bits;
            export using rbts::bits_leaf;
            export using rbts::node_type;
            export using rbts::edit_type;
            export using rbts::empty_regular_pos;
            export using rbts::make_empty_regular_pos;
            export using rbts::empty_leaf_pos;
            export using rbts::make_empty_leaf_pos;
            export using rbts::leaf_pos;
            export using rbts::make_leaf_pos;
            export using rbts::leaf_sub_pos;
            export using rbts::make_leaf_sub_pos;
            export using rbts::leaf_descent_pos;
            export using rbts::make_leaf_descent_pos;
            export using rbts::full_leaf_pos;
            export using rbts::make_full_leaf_pos;
            export using rbts::regular_pos;
            export using rbts::each_regular;
            export using rbts::each_pred_regular;
            export using rbts::each_pred_zip_regular;
            export using rbts::each_pred_i_regular;
            export using rbts::each_pred_left_regular;
            export using rbts::each_pred_right_regular;
            export using rbts::each_i_regular;
            export using rbts::each_left_regular;
            export using rbts::each_right_regular;
            export using rbts::towards_oh_ch_regular;
            export using rbts::towards_sub_oh_regular;
            export using rbts::last_oh_regular;
            export using rbts::make_regular_pos;
            export using rbts::null_sub_pos;
            export using rbts::singleton_regular_sub_pos;
            export using rbts::make_singleton_regular_sub_pos;
            export using rbts::regular_sub_pos;
            export using rbts::make_regular_sub_pos;
            export using rbts::regular_descent_pos;
            export using rbts::visit_regular_descent;
            export using rbts::full_pos;
            export using rbts::make_full_pos;
            export using rbts::relaxed_pos;
            export using rbts::is_relaxed;
            export using rbts::is_relaxed_v;
            export using rbts::make_relaxed_pos;
            export using rbts::visit_maybe_relaxed_sub;
            export using rbts::relaxed_descent_pos;
            export using rbts::visit_maybe_relaxed_descent;
            export using rbts::visitor_base;
            export using rbts::array_for_visitor;
            export using rbts::region_for_visitor;
            export using rbts::get_visitor;
            export using rbts::for_each_chunk_visitor;
            export using rbts::for_each_chunk_p_visitor;
            export using rbts::for_each_chunk_left_visitor;
            export using rbts::for_each_chunk_right_visitor;
            export using rbts::for_each_chunk_i_visitor;
            export using rbts::for_each_chunk_p_left_visitor;
            export using rbts::for_each_chunk_p_right_visitor;
            export using rbts::for_each_chunk_p_i_visitor;
            export using rbts::equals_visitor;
            export using rbts::update_visitor;
            export using rbts::dec_visitor;
            export using rbts::dec_leaf;
            export using rbts::dec_inner;
            export using rbts::dec_relaxed;
            export using rbts::dec_regular;
            export using rbts::dec_empty_regular;
            export using rbts::get_mut_visitor;
            export using rbts::push_tail_mut_visitor;
            export using rbts::push_tail_visitor;
            export using rbts::dec_right_visitor;
            export using rbts::slice_right_mut_visitor;
            export using rbts::slice_right_visitor;
            export using rbts::dec_left_visitor;
            export using rbts::slice_left_mut_visitor;
            export using rbts::slice_left_visitor;
            export using rbts::concat_center_pos;
            export using rbts::concat_merger;
            export using rbts::concat_merger_visitor;
            export using rbts::concat_rebalance_plan_fill_visitor;
            export using rbts::concat_rebalance_plan;
            export using rbts::concat_rebalance;
            export using rbts::concat_leafs;
            export using rbts::concat_left_visitor;
            export using rbts::concat_right_visitor;
            export using rbts::concat_both_visitor;
            export using rbts::concat_inners;
            export using rbts::concat_trees_right_visitor;
            export using rbts::concat_trees_left_visitor;
            export using rbts::concat_trees;
            export using rbts::concat_center_mut_pos;
            export using rbts::concat_merger_mut;
            export using rbts::concat_merger_mut_visitor;
            export using rbts::concat_rebalance_plan_mut;
            export using rbts::concat_rebalance_mut;
            export using rbts::concat_leafs_mut;
            export using rbts::concat_left_mut_visitor;
            export using rbts::concat_right_mut_visitor;
            export using rbts::concat_both_mut_visitor;
            export using rbts::concat_inners_mut;
            export using rbts::concat_trees_right_mut_visitor;
            export using rbts::concat_trees_left_mut_visitor;
            export using rbts::concat_trees_mut;
#if IMMER_THROW_ON_INVALID_STATE
            export using rbts::invalid_tree;
#endif
            export using rbts::rrbtree_iterator;
            export using rbts::rrbtree;
            export using rbts::rbtree;
            export using rbts::rbtree_iterator;
        } // namespace rbts
        export using detail::iterator_core_access;
        export using detail::iterator_facade;
        namespace hamts
        {
            export using hamts::size_t;
            export using hamts::bits_t;
            export using hamts::count_t;
            export using hamts::shift_t;
            export using hamts::get_bitmap_type;
            export using hamts::branches;
            export using hamts::mask;
            export using hamts::max_depth;
            export using hamts::max_shift;
            export using hamts::popcount_fallback;
            export using hamts::popcount;
            export using hamts::set_bits_range;
            export using hamts::destroy_at;
            export using hamts::node;
#if IMMER_DEBUG_STATS
            export using hamts::champ_debug_stats;
#endif
            export using hamts::champ;
            export using hamts::champ_iterator;
        } // namespace hamts
    }     // namespace detail
    export using immer::accumulate;
    export using immer::copy;
    export using immer::all_of;
    export using immer::differ;
    export using immer::make_differ;
    export using immer::diff;
    export using immer::cpp_heap;
    export using immer::identity_heap;
#if IMMER_ENABLE_DEBUG_SIZE_HEAP
    export using immer::debug_size_heap;
#endif
    export using immer::free_list_heap;
    export using immer::split_heap;
    export using immer::unsafe_free_list_heap;
    export using immer::thread_local_free_list_heap;
    export using immer::heap_policy;
    export using immer::enable_optimized_heap_policy;
    export using immer::free_list_heap_policy;
    export using immer::unsafe_free_list_heap_policy;
    export using immer::no_lock_policy;
    export using immer::spinlock_policy;
    export using immer::disowned;
    export using immer::no_refcount_policy;
    export using immer::refcount_policy;
    export using immer::unsafe_refcount_policy;
    export using immer::norefs_tag;
    export using immer::gc_transience_policy;
    export using immer::no_transience_policy;
    export using immer::get_transience_policy;
    export using immer::get_transience_policy_t;
    export using immer::get_prefer_fewer_bigger_objects;
    export using immer::get_prefer_fewer_bigger_objects_v;
    export using immer::get_use_transient_rvalues;
    export using immer::get_use_transient_rvalues_v;
    export using immer::memory_policy;
#if IMMER_NO_FREE_LIST
    export using immer::default_heap_policy;
#endif
#if IMMER_NO_THREAD_SAFETY
    export using immer::default_refcount_policy;
    export using immer::default_lock_policy;
#endif
    export using immer::default_memory_policy;
    export using immer::array_transient;
    export using immer::array;
    export using immer::box;
    export using immer::operator==;
    export using immer::operator!=;
    export using immer::operator<;
    export using immer::atom;
    export using immer::vector;
    export using immer::flex_vector_transient;
    export using immer::flex_vector;
    export using immer::vector_transient;
    export using immer::map_transient;
    export using immer::map;
    export using immer::set_transient;
    export using immer::set;
    export using immer::table_transient;
    export using immer::get_table_key;
    export using immer::set_table_key;
    export using immer::table_key_fn;
    export using immer::table_key_t;
    export using immer::table;
} // namespace immer
