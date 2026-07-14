/**
 * @file boost_sample.hpp
 * @author Francesco Corso
 * @date 7/11/2026
 * @brief
 */
#pragma once

namespace boost
{
    template <class T>
    T *addressof(T &x);

}

namespace boost
{
    template <class A>
    struct allocator_value_type;

    template <class A>
    using allocator_value_type_t = typename allocator_value_type<A>::type;

    template <class A>
    struct allocator_pointer;

    template <class A>
    using allocator_pointer_t = typename allocator_pointer<A>::type;

    template <class A>
    struct allocator_const_pointer;

    template <class A>
    using allocator_const_pointer_t = typename allocator_const_pointer<A>::type;

    template <class A>
    struct allocator_void_pointer;

    template <class A>
    using allocator_void_pointer_t = typename allocator_void_pointer<A>::type;

    template <class A>
    struct allocator_const_void_pointer;

    template <class A>
    using allocator_const_void_pointer_t = typename allocator_const_void_pointer<A>::type;

    template <class A>
    struct allocator_difference_type;

    template <class A>
    using allocator_difference_type_t = typename allocator_difference_type<A>::type;

    template <class A>
    struct allocator_size_type;

    template <class A>
    using allocator_size_type_t = typename allocator_size_type<A>::type;

    template <class A>
    struct allocator_propagate_on_container_copy_assignment;

    template <class A>
    using allocator_propagate_on_container_copy_assignment_t =
        typename allocator_propagate_on_container_copy_assignment<A>::type;

    template <class A>
    struct allocator_propagate_on_container_move_assignment;

    template <class A>
    using allocator_propagate_on_container_move_assignment_t =
        typename allocator_propagate_on_container_move_assignment<A>::type;

    template <class A>
    struct allocator_propagate_on_container_swap;

    template <class A>
    using allocator_propagate_on_container_swap_t = typename allocator_propagate_on_container_swap<A>::type;

    template <class A>
    struct allocator_is_always_equal;

    template <class A>
    using allocator_is_always_equal_t = typename allocator_is_always_equal<A>::type;

    template <class A, class T>
    struct allocator_rebind;

    template <class A, class T>
    using allocator_rebind_t = typename allocator_rebind<A, T>::type;

    template <class A>
    allocator_pointer_t<A> allocator_allocate(A &a, allocator_size_type_t<A> n);

    template <class A>
    allocator_pointer_t<A> allocator_allocate(A &a, allocator_size_type_t<A> n, allocator_const_void_pointer_t<A> h);

    template <class A>
    void allocator_deallocate(A &a, allocator_pointer_t<A> p, allocator_size_type_t<A> n);

    template <class A, class T, class... Args>
    void allocator_construct(A &a, T *p, Args &&...args);

    template <class A, class T>
    void allocator_construct_n(A &a, T *p, std::size_t n);

    template <class A, class T>
    void allocator_construct_n(A &a, T *p, std::size_t n, const T *l, std::size_t m);

    template <class A, class T, class I>
    void allocator_construct_n(A &a, T *p, std::size_t n, I begin);

    template <class A, class T>
    void allocator_destroy(A &a, T *p);

    template <class A, class T>
    void allocator_destroy_n(A &a, T *p, std::size_t n);

    template <class A>
    allocator_size_type_t<A> allocator_max_size(const A &a);

    template <class A>
    A allocator_select_on_container_copy_construction(const A &a);
} // namespace boost

namespace boost
{

    template <class A>
    struct allocator_traits
    {
        using allocator_type = A;

        using value_type = allocator_value_type_t<A>;

        using pointer = allocator_pointer_t<A>;

        using const_pointer = allocator_const_pointer_t<A>;

        using void_pointer = allocator_void_pointer_t<A>;

        using const_pointer = allocator_const_void_pointer_t<A>;

        using difference_type = allocator_difference_type_t<A>;

        using size_type = allocator_size_type_t<A>;

        using propagate_on_container_copy_assignment = allocator_propagate_on_container_copy_assignment_t<A>;

        using propagate_on_container_move_assignment = allocator_propagate_on_container_move_assignment_t<A>;

        using propagate_on_container_swap = allocator_propagate_on_container_swap_t<A>;

        using is_always_equal = allocator_is_always_equal_t<A>;

        template <class T>
        using rebind_traits = allocator_traits<allocator_rebind_t<A, T>>;

        static pointer allocate(A &a, size_type n);

        static pointer allocate(A &a, size_type n, const_void_pointer h);

        static void deallocate(A &a, pointer p, size_type n);

        template <class T, class... Args>
        static void construct(A &a, T *p, Args &&...args);

        static void destroy(A &a, T *p);

        static size_type max_size(const A &a) noexcept;

        static A select_on_container_copy_construction(const A &a);
    };

} // namespace boost

namespace boost
{
    namespace core
    {

        // bit_cast

        template <class To, class From>
        To bit_cast(From const &from) noexcept;

        // byteswap

        template <class T>
        constexpr T byteswap(T x) noexcept;

        // Integral powers of 2

        template <class T>
        constexpr bool has_single_bit(T x) noexcept;

        template <class T>
        constexpr T bit_ceil(T x) noexcept;

        template <class T>
        constexpr T bit_floor(T x) noexcept;

        template <class T>
        constexpr int bit_width(T x) noexcept;

        // Rotating

        template <class T>
        constexpr T rotl(T x, int s) noexcept;

        template <class T>
        constexpr T rotr(T x, int s) noexcept;

        // Counting

        template <class T>
        constexpr int countl_zero(T x) noexcept;

        template <class T>
        constexpr int countl_one(T x) noexcept;

        template <class T>
        constexpr int countr_zero(T x) noexcept;

        template <class T>
        constexpr int countr_one(T x) noexcept;

        template <class T>
        constexpr int popcount(T x) noexcept;

        // Endian

        enum class endian
        {
            little = see below,
            big = see below,
            native = see below
        };

        using endian_type = endian; // portable alias for C++03 code

    } // namespace core
} // namespace boost

namespace boost
{
    template <class T>
    void checked_delete(T *p);
    template <class T>
    void checked_array_delete(T *p);
    template <class T>
    struct checked_deleter;
    template <class T>
    struct checked_array_deleter;
} // namespace boost

namespace boost
{
    namespace core
    {

        // fpclassify return values

        int const fp_zero = /*unspecified*/;
        int const fp_subnormal = /*unspecified*/;
        int const fp_normal = /*unspecified*/;
        int const fp_infinite = /*unspecified*/;
        int const fp_nan = /*unspecified*/;

        // Classification functions

        template <class T>
        bool isfinite(T x);
        template <class T>
        bool isnan(T x);
        template <class T>
        bool isinf(T x);
        template <class T>
        bool isnormal(T x);
        template <class T>
        int fpclassify(T x);

        // Sign manipulation functions

        template <class T>
        bool signbit(T x);
        template <class T>
        T copysign(T x, T y);

    } // namespace core
} // namespace boost

namespace boost
{

    template <class C>
    constexpr auto data(C &c) noexcept(noexcept(c.data())) -> decltype(c.data());

    template <class C>
    constexpr auto data(const C &c) noexcept(noexcept(c.data())) -> decltype(c.data());

    template <class T, std::size_t N>
    constexpr T *data(T (&a)[N]) noexcept;

    template <class T>
    constexpr const T *data(std::initializer_list<T> l) noexcept;

} // namespace boost

namespace boost
{

    template <class T>
    struct default_allocator
    {
        typedef T value_type;
        typedef T *pointer;
        typedef const T *const_pointer;
        typedef std::add_lvalue_reference_t<T> reference;
        typedef std::add_lvalue_reference_t<const T> const_reference;
        typedef std::size_t size_type;
        typedef std::ptrdiff_t difference_type;
        typedef true_type propagate_on_container_move_assignment;
        typedef true_type is_always_equal;

        template <class U>
        struct rebind
        {
            typedef default_allocator<U> other;
        };

        constexpr default_allocator() = default;

        template <class U>
        constexpr default_allocator(const default_allocator<U> &) noexcept
        {
        }

        constexpr std::size_t max_size() const noexcept;
        T *allocate(std::size_t n);
        void deallocate(T *p, std::size_t);
    };

    template <class T, class U>
    constexpr bool operator==(const default_allocator<T> &, const default_allocator<U> &) noexcept;

    template <class T, class U>
    constexpr bool operator!=(const default_allocator<T> &, const default_allocator<U> &) noexcept;

} // namespace boost

namespace boost
{

    namespace core
    {
        std::string demangle(char const *name);

        char const *demangle_alloc(char const *name) noexcept;
        void demangle_free(char const *demangled_name) noexcept;

        class scoped_demangled_name
        {
          public:
            explicit scoped_demangled_name(char const *name) noexcept;
            ~scoped_demangled_name() noexcept;
            char const *get() const noexcept;

            scoped_demangled_name(scoped_demangled_name const &) = delete;
            scoped_demangled_name &operator=(scoped_demangled_name const &) = delete;
        };
    } // namespace core

} // namespace boost

namespace boost
{

    struct empty_init_t
    {
    };

    template <class T, unsigned Index = 0, bool Empty = see below>
    class empty_value
    {
      public:
        typedef T type;

        empty_value() = default;

        template <class... Args>
        constepxr empty_value(empty_init_t, Args &&...args);

        constepxr const T &get() const noexcept;

        constepxr T &get() noexcept;
    };

    inline constexpr empty_init_t empty_init{};

} // namespace boost

namespace boost
{
    template <class Cond, class T = void>
    struct enable_if;
    template <class Cond, class T = void>
    struct disable_if;
    template <class Cond, class T>
    struct lazy_enable_if;
    template <class Cond, class T>
    struct lazy_disable_if;

    template <bool B, class T = void>
    struct enable_if_c;
    template <bool B, class T = void>
    struct disable_if_c;
    template <bool B, class T>
    struct lazy_enable_if_c;
    template <bool B, class T>
    struct lazy_disable_if_c;
} // namespace boost

namespace boost
{
    template <class T, class U = T>
    constexpr T exchange(T &t, U &&u);
}

namespace boost
{

    template <class T>
    constexpr T *first_scalar(T *p) noexcept;

    template <class T, std::size_t N>
    constexpr auto first_scalar(T (*p)[N]) noexcept;

} // namespace boost

namespace boost::core
{

    template <auto Function>
    struct functor
    {
        template <typename... Args>
        decltype(auto) operator()(Args &&...args) const noexcept(...);
    };

} // namespace boost::core

namespace boost
{

    struct identity
    {
        using is_transparent = unspecified;

        template <class T>
        T &&operator()(T &&value) const noexcept;
    };

} // namespace boost

namespace boost
{
    template <typename... Ts>
    auto ignore_unused(Ts &&...) noexcept -> void;
}

namespace boost
{
    template <class T>
    struct is_placeholder;
}

namespace boost
{

    namespace core
    {
        template <class T1, class T2>
        struct is_same;
    }

} // namespace boost

namespace boost
{
    namespace core
    {

        template <class T>
        T *launder(T *p);

    } // namespace core
} // namespace boost

namespace boost
{

    int report_errors();

    namespace core
    {

        void lwt_init();

    } // namespace core
} // namespace boost

namespace boost
{

    template <class I>
    constexpr span<I> make_span(I *d, std::size_t n) noexcept;

    template <class I>
    constexpr span<I> make_span(I *b, I *e) noexcept;

    template <class T, std::size_t N>
    constexpr span<T, N> make_span(T (&a)[N]) noexcept;

    template <class T, std::size_t N>
    constexpr span<T, N> make_span(std::array<T, N> &a) noexcept;

    template <class T, std::size_t N>
    constexpr span<const T, N> make_span(const std::array<T, N> &a) noexcept;

    template <class R>
    span<remove_pointer_t<iterator_t<R>>> make_span(R &&r);

} // namespace boost

namespace boost
{
    namespace core
    {

        union max_align_t;

        constexpr std::size_t max_align = alignof(max_align_t);

    } // namespace core
} // namespace boost

namespace boost
{
    namespace core
    {

        class memory_resource
        {
          public:
            virtual ~memory_resource() = default;

            [[nodiscard]] void *allocate(std::size_t bytes, std::size_t alignment = max_align);
            void deallocate(void *p, std::size_t bytes, std::size_t alignment = max_align);

            bool is_equal(memory_resource const &other) const noexcept;

          private:
            virtual void *do_allocate(std::size_t bytes, std::size_t alignment) = 0;
            virtual void do_deallocate(void *p, std::size_t bytes, std::size_t alignment) = 0;

            virtual bool do_is_equal(memory_resource const &other) const noexcept = 0;
        };

        inline bool operator==(memory_resource const &a, memory_resource const &b) noexcept;
        inline bool operator!=(memory_resource const &a, memory_resource const &b) noexcept;

    } // namespace core
} // namespace boost

namespace boost
{

    template <class A>
    struct noinit_adaptor : A
    {
        template <class U>
        struct rebind
        {
            typedef noinit_adaptor<allocator_rebind_t<A, U>> other;
        };

        noinit_adaptor() noexcept;

        template <class U>
        noinit_adaptor(U &&u) noexcept;

        template <class U>
        noinit_adaptor(const noinit_adaptor<U> &u) noexcept;

        template <class U>
        void construct(U *p);

        template <class U>
        void destroy(U *p);
    };

    template <class T, class U>
    bool operator==(const noinit_adaptor<T> &lhs, const noinit_adaptor<U> &rhs) noexcept;

    template <class T, class U>
    bool operator!=(const noinit_adaptor<T> &lhs, const noinit_adaptor<U> &rhs) noexcept;

    template <class A>
    noinit_adaptor<A> noinit_adapt(const A &a) noexcept;

} // namespace boost

namespace boost
{
    class noncopyable;
    class null_deleter;
    class fclose_deleter;
} // namespace boost

namespace boost
{

    template <class T>
    class nvp
    {
      public:
        nvp(const char *name, T &value) noexcept;

        const char *name() const noexcept;

        T &value() const noexcept;

        const T &const_value() const noexcept;
    };

    template <class T>
    const nvp<T> make_nvp(const char *name, T &value) noexcept;

} // namespace boost

namespace boost
{

    template <class T>
    constexpr bool pointer_in_range(const T *ptr, const T *begin, const T *end);

} // namespace boost

namespace boost
{

    template <class T>
    struct pointer_traits
    {
        typedef T pointer;
        typedef see below element_type;
        typedef see below difference_type;

        template <class U>
        struct rebind_to
        {
            typedef see below type;
        };

        template <class U>
        using rebind = typename rebind_to<U>::type;

        static pointer pointer_to(element_type &v);
    };

    template <class T>
    struct pointer_traits<T *>
    {
        typedef T *pointer;
        typedef T element_type;
        typedef std::ptrdiff_t difference_type;

        template <class U>
        struct rebind_to
        {
            typedef U *type;
        };

        template <class U>
        using rebind = typename rebind_to<U>::type;

        static pointer pointer_to(see below v) noexcept;
    };

    template <class T>
    constexpr T *to_address(T *v) noexcept;

    template <class T>
    auto to_address(const T &v) noexcept;

} // namespace boost

namespace boost
{
    [[noreturn]] void quick_exit(int code) noexcept;
}

namespace boost
{
    template <typename T>
    struct is_reference_wrapper;

    template <typename T>
    class reference_wrapper;

    template <typename T>
    struct unwrap_reference;
    template <typename T>
    reference_wrapper<T> const ref(T &);
    template <typename T>
    reference_wrapper<T const> const cref(T const &);
    template <typename T>
    void ref(T const &&);
    template <typename T>
    void cref(T const &&);
    template <typename T>
    unwrap_reference<T>::type &unwrap_ref(T &);
} // namespace boost

namespace boost
{
    namespace serialization
    {

        // forward declarations

        template <class T>
        struct version;
        class access;

        // core_version_type

        struct core_version_type;

    } // namespace serialization

    namespace core
    {

        // nvp

        using serialization::make_nvp;
        using serialization::nvp;

        // split_free

        template <class Ar, class T>
        void split_free(Ar &ar, T &t, unsigned int v);

        // split_member

        template <class Ar, class T>
        void split_member(Ar &ar, T &t, unsigned int v);

        // load_construct_data_adl

        template <class Ar, class T>
        void load_construct_data_adl(Ar &ar, T *t, unsigned int v);

        // save_construct_data_adl

        template <class Ar, class T>
        void save_construct_data_adl(Ar &ar, T const *t, unsigned int v);

    } // namespace core
} // namespace boost

namespace boost
{

    template <class C>
    constexpr auto size(const C &c) noexcept(noexcept(c.size())) -> decltype(c.size());

    template <class T, std::size_t N>
    constexpr std::size_t size(T (&)[N]) noexcept;

} // namespace boost

namespace boost
{

    constexpr std::size_t dynamic_extent = -1;

    template <class T, std::size_t E = dynamic_extent>
    class span
    {
      public:
        typedef T element_type;
        typedef std::remove_cv_t<T> value_type;
        typedef std::size_t size_type;
        typedef std::ptrdiff_t difference_type;
        typedef T *pointer;
        typedef const T *const_pointer;
        typedef T &reference;
        typedef const T &const_reference;
        typedef T *iterator;
        typedef const T *const_iterator;
        typedef std::reverse_iterator<T *> reverse_iterator;
        typedef std::reverse_iterator<const T *> const_reverse_iterator;

        static constexpr std::size_t extent = E;

        constexpr span() noexcept;

        explicit(E != dynamic_extent) template <class I>
        constexpr span(I *f, size_type c);

        explicit(E != dynamic_extent) template <class I, class L>
        constexpr span(I *f, L *l);

        template <std::size_t N>
        constexpr span(type_identity_t<T> (&a)[N]);

        template <class U, std::size_t N>
        constexpr span(std::array<U, N> &a) noexcept;

        template <class U, std::size_t N>
        constexpr span(const std::array<U, N> &a) noexcept;

        explicit(E != dynamic_extent) template <class R>
        constexpr span(R &&r);

        explicit(E != dynamic_extent && N == dynamic_extent) template <class U, std::size_t N>
        constexpr span(const span<U, N> &s) noexcept;

        template <std::size_t C>
        constexpr span<T, C> first() const;

        template <std::size_t C>
        constexpr span<T, C> last() const;

        template <std::size_t O, std::size_t C = dynamic_extent>
        constexpr span<T, see below> subspan() const;

        constexpr span<T, dynamic_extent> first(size_type c) const;
        constexpr span<T, dynamic_extent> last(size_type c) const;

        constexpr span<T, dynamic_extent> subspan(size_type o, size_type c = dynamic_extent) const;

        constexpr size_type size() const noexcept;
        constexpr size_type size_bytes() const noexcept;
        constexpr bool empty() const noexcept;

        constexpr reference operator[](size_type i) const;
        constexpr reference front() const;
        constexpr reference back() const;
        constexpr pointer data() const noexcept;

        constexpr iterator begin() const noexcept;
        constexpr iterator end() const noexcept;
        constexpr reverse_iterator rbegin() const noexcept;
        constexpr reverse_iterator rend() const noexcept;
        constexpr const_iterator cbegin() const noexcept;
        constexpr const_iterator cend() const noexcept;
        constexpr const_reverse_iterator crbegin() const noexcept;
        constexpr const_reverse_iterator crend() const noexcept;
    };

    template <class I, class L>
    span(I *, L) -> span<I>;

    template <class T, std::size_t N>
    span(T (&)[N]) -> span<T, N>;

    template <class T, std::size_t N>
    span(std::array<T, N> &) -> span<T, N>;

    template <class T, std::size_t N>
    span(const std::array<T, N> &) -> span<const T, N>;

    template <class R>
    span(R &&) -> span<remove_pointer_t<iterator_t<R>>>;

    template <class T, std::size_t E>
    span<const std::byte, E == dynamic_extent ? dynamic_extent : sizeof(T) * E> as_bytes(span<T, E> s) noexcept;

    template <class T, std::size_t E>
    span<std::byte, E == dynamic_extent ? dynamic_extent : sizeof(T) * E> as_writable_bytes(span<T, E> s) noexcept;

} // namespace boost

namespace boost
{
    template <class T>
    void swap(T &left, T &right);
}

namespace boost
{

    namespace core
    {
        class typeinfo;
        /* char const* or std::string */ demangled_name(typeinfo const &ti);
    } // namespace core

} // namespace boost

namespace boost
{
    namespace core
    {

        template <class T>
        std::string type_name();

        void uncaught_exceptions() noexcept;

    } // namespace core
} // namespace boost

namespace boost
{
    struct use_default
    {
    };
} // namespace boost

namespace boost
{
    namespace core
    {

        [[noreturn]] void verbose_terminate_handler();

    } // namespace core
} // namespace boost

namespace boost
{
    namespace core
    {

        void sp_thread_pause() noexcept;
        void sp_thread_yield() noexcept;
        void sp_thread_sleep() noexcept;

    } // namespace core
} // namespace boost

namespace boost
{
    namespace multiprecision
    {

        enum expression_template_option
        {
            et_on = 1,
            et_off = 0
        };

        template <class Backend>
        struct expression_template_default
        {
            static const expression_template_option value = et_on;
        };

        template <class Backend,
                  expression_template_option ExpressionTemplates = expression_template_default<Backend>::value>
        class number
        {
          public:
            typedef Backend backend_type;
            typedef typename component_type<self_type>::type value_type;

            static constexpr expression_template_option et = ExpressionTemplates;

            number();
            number(see - below);
            number &operator=(see - below);
            number &assign(see - below);

            // Member operators
            number &operator+=(const see - below &);
            number &operator-=(const see - below &);
            number &operator*=(const see - below &);
            number &operator/=(const see - below &);
            number &operator++();
            number &operator--();
            number operator++(int);
            number operator--(int);

            number &operator%=(const see - below &);
            number &operator&=(const see - below &);
            number &operator|=(const see - below &);
            number &operator^=(const see - below &);
            number &operator<<=(const integer - type &);
            number &operator>>=(const integer - type &);

            // Use in Boolean context:
            operator convertible - to - bool - type() const;
            // swap:
            void swap(number &other);
            // Sign:
            bool is_zero() const;
            int sign() const;
            // string conversion:
            std::string str(std::streamsize digits = 0, std::ios_base::fmtflags f = std::ios_base::fmtflags(0)) const;
            // Generic conversion mechanism
            template <class T>
            T convert_to() const;
            template <class T>
            explicit operator T() const;
            // precision control:
            static unsigned default_precision();
            static void default_precision(unsigned digits10);
            unsigned precision() const;
            void precision(unsigned digits10);
            // Comparison:
            int compare(const number<Backend> &o) const;
            template <class V>
            typename std::enable_if<std::is_convertible<V, number<Backend, ExpressionTemplates>::value>, int>::type
            compare(const V &o) const;
            // real and imaginary parts:
            value_type real() const;
            value_type imag() const;
            template <class T>
            void real(const T &val);
            template <class T>
            void imag(const T &val);
            // Access to the underlying implementation:
            Backend &backend();
            const Backend &backend() const;
        };

        // Non member operators:
        unknown operator+(const see - below &);
        unknown operator-(const see - below &);
        unknown operator+(const see - below &, const see - below &);
        unknown operator-(const see - below &, const see - below &);
        unknown operator*(const see - below &, const see - below &);
        unknown operator/(const see - below &, const see - below &);
        // Integer only operations:
        unknown operator%(const see - below &, const see - below &);
        unknown operator&(const see - below &, const see - below &);
        unknown operator|(const see - below &, const see - below &);
        unknown operator^(const see - below &, const see - below &);
        unknown operator<<(const see - below &, const integer - type &);
        unknown operator>>(const see - below &, const integer - type &);
        // Comparison operators:
        bool operator==(const see - below &, const see - below &);
        bool operator!=(const see - below &, const see - below &);
        bool operator<(const see - below &, const see - below &);
        bool operator>(const see - below &, const see - below &);
        bool operator<=(const see - below &, const see - below &);
        bool operator>=(const see - below &, const see - below &);

        // Swap:
        template <class Backend, expression_template_option ExpressionTemplates>
        void swap(number<Backend, ExpressionTemplates> &a, number<Backend, ExpressionTemplates> &b);

        // iostream support:
        template <class Backend, expression_template_option ExpressionTemplates>
        std::ostream &operator<<(std::ostream &os, const number<Backend, ExpressionTemplates> &r);
        std::ostream &operator<<(std::ostream &os, const unknown &r);
        template <class Backend, expression_template_option ExpressionTemplates>
        std::istream &operator>>(std::istream &is, number<Backend, ExpressionTemplates> &r);

        // to_string support:
        template <class Backend, expression_template_option ExpressionTemplates>
        std::string to_string(const number<Backend, ExpressionTemplates> &val);

        // Arithmetic with a higher precision result:
        template <class ResultType, class Source1 class Source2>
        ResultType &add(ResultType &result, const Source1 &a, const Source2 &b);
        template <class ResultType, class Source1 class Source2>
        ResultType &subtract(ResultType &result, const Source1 &a, const Source2 &b);
        template <class ResultType, class Source1 class Source2>
        ResultType &multiply(ResultType &result, const Source1 &a, const Source2 &b);

        // min and max overloads:
        number min(const number - or -expression - template - type &,
                   const number - or -expression - template - type &);
        number max(const number - or -expression - template - type &,
                   const number - or -expression - template - type &);

        // C99 Non-member function standard library support:
        unknown abs(const number - or -expression - template - type &);
        unknown acos(const number - or -expression - template - type &);
        number acosh(const number - or -expression - template - type &);
        unknown asin(const number - or -expression - template - type &);
        number asinh(const number - or -expression - template - type &);
        unknown atan(const number - or -expression - template - type &);
        unknown atan2(const number - or -expression - template - type &,
                      const number - or -expression - template - type &);
        number atanh(const number - or -expression - template - type &);
        number cbrt(const number - or -expression - template - type &);
        unknown ceil(const number - or -expression - template - type &);
        number copysign(const number - or -expression - template - type &,
                        const number - or -expression - template - type &);
        unknown cos(const number - or -expression - template - type &);
        unknown cosh(const number - or -expression - template - type &);
        number erf(const number - or -expression - template - type &);
        number erfc(const number - or -expression - template - type &);
        unknown exp(const number - or -expression - template - type &);
        unknown exp2(const number - or -expression - template - type &);
        number expm1(const number - or -expression - template - type &);
        unknown fabs(const number - or -expression - template - type &);
        unknown fdim(const number - or -expression - template - type &);
        unknown floor(const number - or -expression - template - type &);
        unknown fma(const number - or -expression - template - type &,
                    const number - or -expression - template - type &,
                    const number - or -expression - template - type &);
        unknown fmin(const number - or -expression - template - type &,
                     const number - or -expression - template - type &);
        unknown fmax(const number - or -expression - template - type &,
                     const number - or -expression - template - type &);
        unknown fmod(const number - or -expression - template - type &,
                     const number - or -expression - template - type &);
        unknown frexp(const number - or -expression - template - type &, integer - type *);
        unknown hypot(const number - or -expression - template - type &,
                      const number - or -expression - template - type &);
        integer - type ilogb(const number - or -expression - template - type &);
        unknown ldexp(const number - or -expression - template - type &, integer - type);
        number lgamma(const number - or -expression - template - type &);
        long long llrint(const number - or -expression - template - type &);
        long long llround(const number - or -expression - template - type &);
        unknown log(const number - or -expression - template - type &);
        unknown log2(const number - or -expression - template - type &);
        unknown log10(const number - or -expression - template - type &);
        number log1p(const number - or -expression - template - type &);
        unknown logb(const number - or -expression - template - type &);
        long lrint(const number - or -expression - template - type &);
        long lround(const number - or -expression - template - type &);
        unknown modf(const number - or -expression - template - type &,
                     const number - or -expression - template - type &);
        unknown nearbyint(const number - or -expression - template - type &);
        number nextafter(const number - or -expression - template - type &,
                         const number - or -expression - template - type &);
        number nexttoward(const number - or -expression - template - type &,
                          const number - or -expression - template - type &);
        unknown pow(const number - or -expression - template - type &,
                    const number - or -expression - template - type &);
        unknown remainder(const number - or -expression - template - type &,
                          const number - or -expression - template - type &);
        unknown remquo(const number - or -expression - template - type &,
                       const number - or -expression - template - type &,
                       int *);
        unknown rint(const number - or -expression - template - type &);
        unknown round(const number - or -expression - template - type &);
        unknown scalbn(const number - or -expression - template - type &, integer - type);
        unknown scalbln(const number - or -expression - template - type &, integer - type);
        unknown sin(const number - or -expression - template - type &);
        unknown sinh(const number - or -expression - template - type &);
        unknown sqrt(const number - or -expression - template - type &);
        unknown tan(const number - or -expression - template - type &);
        unknown tanh(const number - or -expression - template - type &);
        number tgamma(const number - or -expression - template - type &);
        unknown trunc(const number - or -expression - template - type &);

        int fpclassify(const number - or -expression - template - type &);
        bool isfinite(const number - or -expression - template - type &);
        bool isinf(const number - or -expression - template - type &);
        bool isnan(const number - or -expression - template - type &);
        bool isnormal(const number - or -expression - template - type &);
        bool signbit(const number - or -expression - template - type &);

        bool isgreater(const number - or -expression - template - type &,
                       const number - or -expression - template - type &);
        bool isgreaterequal(const number - or -expression - template - type &,
                            const number - or -expression - template - type &);
        bool isless(const number - or -expression - template - type &,
                    const number - or -expression - template - type &);
        bool islessequal(const number - or -expression - template - type &,
                         const number - or -expression - template - typearea &);
        bool islessgreater(const number - or -expression - template - type &,
                           const number - or -expression - template - type &);
        bool isunordered(const number - or -expression - template - type &,
                         const number - or -expression - template - type &);
        // Complex number functions:
        number<...>::value_type real(const number - or -expression - template - type &);
        number<...>::value_type imag(const number - or -expression - template - type &);
        number<...>::value_type abs(const number - or -expression - template - type &);
        number<...>::value_type arg(const number - or -expression - template - type &);
        number<...>::value_type norm(const number - or -expression - template - type &);
        number conj(const number - or -expression - template - type &);
        number proj(const number - or -expression - template - type &);
        number polar(const number - or -expression - template - type &,
                     const number - or -expression - template - type &);
        // Misc other common C library functions:
        unknown itrunc(const number - or -expression - template - type &);
        unknown ltrunc(const number - or -expression - template - type &);
        unknown lltrunc(const number - or -expression - template - type &);
        unknown iround(const number - or -expression - template - type &);
        number changesign(const number - or -expression - template - type &);
        number copysign(const number - or -expression - template - type &,
                        const number - or -expression - template - type &);

        // Traits support:
        template <class T>
        struct component_type;
        template <class T>
        struct number_category;
        template <class T>
        struct is_number;
        template <class T>
        struct is_number_expression;

        // Integer specific functions:
        unknown gcd(const number - or -expression - template - type &,
                    const number - or -expression - template - type &);
        unknown lcm(const number - or -expression - template - type &,
                    const number - or -expression - template - type &);
        unknown pow(const number - or -expression - template - type &, unsigned);
        unknown powm(const number - or -expression - template - type & b,
                     const number - or -expression - template - type & p,
                     const number - or -expression - template - type & m);
        unknown sqrt(const number - or -expression - template - type &);
        template <class Backend, expression_template_option ExpressionTemplates>
        number<Backend, EXpressionTemplates> sqrt(const number - or -expression - template - type &,
                                                  number<Backend, EXpressionTemplates> &);
        template <class Backend, expression_template_option ExpressionTemplates>
        void divide_qr(const number - or -expression - template - type & x,
                       const number - or -expression - template - type & y,
                       number<Backend, ExpressionTemplates> &q,
                       number<Backend, ExpressionTemplates> &r);
        template <class Integer>
        Integer integer_modulus(const number - or -expression - template - type & x, Integer val);
        unsigned lsb(const number - or -expression - template - type & x);
        unsigned msb(const number - or -expression - template - type & x);
        template <class Backend, class ExpressionTemplates>
        bool bit_test(const number<Backend, ExpressionTemplates> &val, unsigned index);
        template <class Backend, class ExpressionTemplates>
        number<Backend, ExpressionTemplates> &bit_set(number<Backend, ExpressionTemplates> &val, unsigned index);
        template <class Backend, class ExpressionTemplates>
        number<Backend, ExpressionTemplates> &bit_unset(number<Backend, ExpressionTemplates> &val, unsigned index);
        template <class Backend, class ExpressionTemplates>
        number<Backend, ExpressionTemplates> &bit_flip(number<Backend, ExpressionTemplates> &val, unsigned index);
        template <class Engine>
        bool miller_rabin_test(const number - or -expression - template - type & n, unsigned trials, Engine &gen);
        bool miller_rabin_test(const number - or -expression - template - type & n, unsigned trials);

        // Rational number support:
        typename component_type<number - or -expression - template - type>::type numerator(const number - or
                                                                                           -expression - template -
                                                                                               type &);
        typename component_type<number - or -expression - template - type>::type denominator(const number - or
                                                                                             -expression - template -
                                                                                                 type &);

    } // namespace multiprecision
} // namespace boost

namespace boost
{
    namespace math
    {

        // Boost.Math interoperability functions:
        int fpclassify(const number - or -expression - template - type &, int);
        bool isfinite(const number - or -expression - template - type &, int);
        bool isnan(const number - or -expression - template - type &, int);
        bool isinf(const number - or -expression - template - type &, int);
        bool isnormal(const number - or -expression - template - type &, int);

    } // namespace math
} // namespace boost

// numeric_limits support:
namespace std
{

    template <class Backend, expression_template_option ExpressionTemplates>
    struct numeric_limits<boost::multiprecision<Backend, ExpressionTemplates>>
    {
        /* Usual members here */
    };

} // namespace std

namespace boost
{
    namespace multiprecision
    {

        typedef unspecified - type limb_type;

        enum cpp_integer_type
        {
            signed_magnitude,
            unsigned_magnitude
        };
        enum cpp_int_check_type
        {
            checked,
            unchecked
        };

        template <unsigned MinDigits = 0,
                  unsigned MaxDits = 0,
                  cpp_integer_type SignType = signed_magnitude,
                  cpp_int_check_type Checked = unchecked,
                  class Allocator = std::allocator<limb_type>>
        class cpp_int_backend;
        //
        // Expression templates default to et_off if there is no allocator:
        //
        template <unsigned MinDigits, unsigned MaxDigits, cpp_integer_type SignType, cpp_int_check_type Checked>
        struct expression_template_default<cpp_int_backend<MinDigits, MaxDigits, SignType, Checked, void>>
        {
            static const expression_template_option value = et_off;
        };

        typedef number<cpp_int_backend<>> cpp_int; // arbitrary precision integer
        typedef rational_adaptor<cpp_int_backend<>> cpp_rational_backend;
        typedef number<cpp_rational_backend> cpp_rational; // arbitrary precision rational number

        // Fixed precision unsigned types:
        typedef number<cpp_int_backend<128, 128, unsigned_magnitude, unchecked, void>> uint128_t;
        typedef number<cpp_int_backend<256, 256, unsigned_magnitude, unchecked, void>> uint256_t;
        typedef number<cpp_int_backend<512, 512, unsigned_magnitude, unchecked, void>> uint512_t;
        typedef number<cpp_int_backend<1024, 1024, unsigned_magnitude, unchecked, void>> uint1024_t;

        // Fixed precision signed types:
        typedef number<cpp_int_backend<128, 128, signed_magnitude, unchecked, void>> int128_t;
        typedef number<cpp_int_backend<256, 256, signed_magnitude, unchecked, void>> int256_t;
        typedef number<cpp_int_backend<512, 512, signed_magnitude, unchecked, void>> int512_t;
        typedef number<cpp_int_backend<1024, 1024, signed_magnitude, unchecked, void>> int1024_t;

        // Over again, but with checking enabled this time:
        typedef number<cpp_int_backend<0, 0, signed_magnitude, checked>> checked_cpp_int;
        typedef rational_adaptor<cpp_int_backend<0, 0, signed_magnitude, checked>> checked_cpp_rational_backend;
        typedef number<checked_cpp_rational_backend> checked_cpp_rational;

        // Checked fixed precision unsigned types:
        typedef number<cpp_int_backend<128, 128, unsigned_magnitude, checked, void>> checked_uint128_t;
        typedef number<cpp_int_backend<256, 256, unsigned_magnitude, checked, void>> checked_uint256_t;
        typedef number<cpp_int_backend<512, 512, unsigned_magnitude, checked, void>> checked_uint512_t;
        typedef number<cpp_int_backend<1024, 1024, unsigned_magnitude, checked, void>> checked_uint1024_t;

        // Fixed precision signed types:
        typedef number<cpp_int_backend<128, 128, signed_magnitude, checked, void>> checked_int128_t;
        typedef number<cpp_int_backend<256, 256, signed_magnitude, checked, void>> checked_int256_t;
        typedef number<cpp_int_backend<512, 512, signed_magnitude, checked, void>> checked_int512_t;
        typedef number<cpp_int_backend<1024, 1024, signed_magnitude, checked, void>> checked_int1024_t;

    } // namespace multiprecision
} // namespace boost

#if __has_include(<gmp.hpp>)
namespace boost
{
    namespace multiprecision
    {

        class gmp_int;

        typedef number<gmp_int> mpz_int;

    } // namespace multiprecision
} // namespace boost
#endif

namespace boost
{
    namespace multiprecision
    {

        class tommath_int;

        typedef number<tommath_int> tom_int;

    } // namespace multiprecision
} // namespace boost

namespace boost
{
    namespace multiprecision
    {

        template <unsigned Digits10>
        class gmp_float;

        typedef number<gmp_float<50>> mpf_float_50;
        typedef number<gmp_float<100>> mpf_float_100;
        typedef number<gmp_float<500>> mpf_float_500;
        typedef number<gmp_float<1000>> mpf_float_1000;
        typedef number<gmp_float<0>> mpf_float;

    } // namespace multiprecision
} // namespace boost

namespace boost
{
    namespace multiprecision
    {

        template <unsigned Digits10>
        class mpfr_float_backend;

        typedef number<mpfr_float_backend<50>> mpfr_float_50;
        typedef number<mpfr_float_backend<100>> mpfr_float_100;
        typedef number<mpfr_float_backend<500>> mpfr_float_500;
        typedef number<mpfr_float_backend<1000>> mpfr_float_1000;
        typedef number<mpfr_float_backend<0>> mpfr_float;

    } // namespace multiprecision
} // namespace boost

namespace boost
{
    namespace multiprecision
    {

        enum digit_base_type
        {
            digit_base_2 = 2,
            digit_base_10 = 10
        };

        template <unsigned Digits,
                  digit_base_type base = digit_base_10,
                  class Allocator = void,
                  class Exponent = int,
                  ExponentMin = 0,
                  ExponentMax = 0>
        class cpp_bin_float;

        typedef number<cpp_bin_float<50>> cpp_bin_float_50;
        typedef number<cpp_bin_float<100>> cpp_bin_float_100;

        typedef number<backends::cpp_bin_float<24, backends::digit_base_2, void, std::int16_t, -126, 127>, et_off>
            cpp_bin_float_single;
        typedef number<backends::cpp_bin_float<53, backends::digit_base_2, void, std::int16_t, -1022, 1023>, et_off>
            cpp_bin_float_double;
        typedef number<backends::cpp_bin_float<64, backends::digit_base_2, void, std::int16_t, -16382, 16383>, et_off>
            cpp_bin_float_double_extended;
        typedef number<backends::cpp_bin_float<113, backends::digit_base_2, void, std::int16_t, -16382, 16383>, et_off>
            cpp_bin_float_quad;
        typedef number<backends::cpp_bin_float<237, backends::digit_base_2, void, std::int32_t, -262142, 262143>,
                       et_off>
            cpp_bin_float_oct;

    } // namespace multiprecision
} // namespace boost

namespace boost
{
    namespace multiprecision
    {

        template <unsigned Digits10, class ExponentType = std::int32_t, class Allocator = void>
        class cpp_dec_float;

        typedef number<cpp_dec_float<50>> cpp_dec_float_50;
        typedef number<cpp_dec_float<100>> cpp_dec_float_100;

    } // namespace multiprecision
} // namespace boost

namespace boost
{
    namespace multiprecision
    {

        template <class FloatingPointType>
        class cpp_double_fp_backend;

        typedef number<cpp_double_fp_backend<float>, et_off> cpp_double_float;
        typedef number<cpp_double_fp_backend<double>, et_off> cpp_double_double;
        typedef number<cpp_double_fp_backend<long double>, et_off> cpp_double_long_double;
        typedef number<cpp_double_fp_backend<boost::float128_type>, et_off>
            cpp_double_float128; // Only when boost::float128_type is available

    } // namespace multiprecision
} // namespace boost

namespace boost
{
    namespace container
    {
        template <typename T,
                  std::size_t NodesPerBlock = ADP_nodes_per_block,
                  std::size_t MaxFreeBlocks = ADP_max_free_blocks,
                  std::size_t OverheadPercent = ADP_overhead_percent>
        class adaptive_pool;
    }
} // namespace boost

namespace boost
{
    namespace container
    {
        template <typename T>
        class allocator;
    }
} // namespace boost

namespace boost
{
    namespace container
    {
        template <typename Alloc>
        struct allocator_traits;
    }
} // namespace boost

namespace boost
{
    namespace container
    {
        template <typename T, typename Allocator = std::allocator<T>>
        class deque;
    }
} // namespace boost

namespace boost
{
    namespace container
    {
        template <typename Key,
                  typename T,
                  typename Compare = std::less<Key>,
                  typename Allocator = std::allocator<std::pair<Key, T>>>
        class flat_map;
        template <typename Key,
                  typename T,
                  typename Compare = std::less<Key>,
                  typename Allocator = std::allocator<std::pair<Key, T>>>
        class flat_multimap;
    } // namespace container
} // namespace boost

namespace boost
{
    namespace container
    {
        template <typename Key, typename Compare = std::less<Key>, typename Allocator = std::allocator<Key>>
        class flat_set;
        template <typename Key, typename Compare = std::less<Key>, typename Allocator = std::allocator<Key>>
        class flat_multiset;
    } // namespace container
} // namespace boost

namespace boost
{
    namespace container
    {
        template <typename T, typename Allocator = std::allocator<T>>
        class list;
    }
} // namespace boost

namespace boost
{
    namespace container
    {
        template <typename Key,
                  typename T,
                  typename Compare = std::less<Key>,
                  typename Allocator = std::allocator<std::pair<const Key, T>>,
                  typename MapOptions = tree_assoc_defaults>
        class map;
        template <typename Key,
                  typename T,
                  typename Compare = std::less<Key>,
                  typename Allocator = std::allocator<std::pair<const Key, T>>,
                  typename MultiMapOptions = tree_assoc_defaults>
        class multimap;
    } // namespace container
} // namespace boost

namespace boost
{
    namespace container
    {
        template <typename T, std::size_t NodesPerBlock = NodeAlloc_nodes_per_block>
        class node_allocator;
    }
} // namespace boost

namespace boost
{
    namespace container
    {
        template <tree_type_enum TreeType>
        struct tree_type;
        template <bool Enabled>
        struct optimize_size;
        template <class... Options>
        struct tree_assoc_options;
    } // namespace container
} // namespace boost

namespace boost
{
    namespace container
    {
        template <typename T>
        struct constructible_with_allocator_suffix;
        template <typename T>
        struct constructible_with_allocator_prefix;
        template <typename T, typename Alloc>
        struct uses_allocator;

        template <typename OuterAlloc, typename... InnerAllocs>
        class scoped_allocator_adaptor;
        template <typename OuterA1, typename OuterA2, typename... InnerAllocs>
        bool operator==(const scoped_allocator_adaptor<OuterA1, InnerAllocs...> &a,
                        const scoped_allocator_adaptor<OuterA2, InnerAllocs...> &b);
        template <typename OuterA1, typename OuterA2, typename... InnerAllocs>
        bool operator!=(const scoped_allocator_adaptor<OuterA1, InnerAllocs...> &a,
                        const scoped_allocator_adaptor<OuterA2, InnerAllocs...> &b);
    } // namespace container
} // namespace boost

namespace boost
{
    namespace container
    {
        template <typename Key,
                  typename Compare = std::less<Key>,
                  typename Allocator = std::allocator<Key>,
                  typename SetOptions = tree_assoc_defaults>
        class set;
        template <typename Key,
                  typename Compare = std::less<Key>,
                  typename Allocator = std::allocator<Key>,
                  typename MultiSetOptions = tree_assoc_defaults>
        class multiset;
    } // namespace container
} // namespace boost

namespace boost
{
    namespace container
    {
        template <typename T, typename Allocator = std::allocator<T>>
        class slist;
    }
} // namespace boost

namespace boost
{
    namespace container
    {
        template <typename Value, std::size_t Capacity>
        class static_vector;
        template <typename V, std::size_t C1, std::size_t C2>
        bool operator==(static_vector<V, C1> const &, static_vector<V, C2> const &);
        template <typename V, std::size_t C1, std::size_t C2>
        bool operator!=(static_vector<V, C1> const &, static_vector<V, C2> const &);
        template <typename V, std::size_t C1, std::size_t C2>
        bool operator<(static_vector<V, C1> const &, static_vector<V, C2> const &);
        template <typename V, std::size_t C1, std::size_t C2>
        bool operator>(static_vector<V, C1> const &, static_vector<V, C2> const &);
        template <typename V, std::size_t C1, std::size_t C2>
        bool operator<=(static_vector<V, C1> const &, static_vector<V, C2> const &);
        template <typename V, std::size_t C1, std::size_t C2>
        bool operator>=(static_vector<V, C1> const &, static_vector<V, C2> const &);
        template <typename V, std::size_t C1, std::size_t C2>
        void swap(static_vector<V, C1> &, static_vector<V, C2> &);
    } // namespace container
} // namespace boost

namespace boost
{
    namespace container
    {
        template <typename CharT, typename Traits = std::char_traits<CharT>, typename Allocator = std::allocator<CharT>>
        class basic_string;
        typedef basic_string<char, std::char_traits<char>, std::allocator<char>> string;
        typedef basic_string<wchar_t, std::char_traits<wchar_t>, std::allocator<wchar_t>> wstring;
        template <typename CharT, typename Traits, typename Allocator>
        basic_string<CharT, Traits, Allocator> operator+(const basic_string<CharT, Traits, Allocator> &x,
                                                         const basic_string<CharT, Traits, Allocator> &y);
        template <typename CharT, typename Traits, typename Allocator>
        basic_string<CharT, Traits, Allocator> operator+(basic_string<CharT, Traits, Allocator> &&mx,
                                                         basic_string<CharT, Traits, Allocator> &&my);
        template <typename CharT, typename Traits, typename Allocator>
        basic_string<CharT, Traits, Allocator> operator+(basic_string<CharT, Traits, Allocator> &&mx,
                                                         const basic_string<CharT, Traits, Allocator> &y);
        template <typename CharT, typename Traits, typename Allocator>
        basic_string<CharT, Traits, Allocator> operator+(const basic_string<CharT, Traits, Allocator> &x,
                                                         basic_string<CharT, Traits, Allocator> &&my);
        template <typename CharT, typename Traits, typename Allocator>
        basic_string<CharT, Traits, Allocator> operator+(const CharT *s, basic_string<CharT, Traits, Allocator> y);
        template <typename CharT, typename Traits, typename Allocator>
        basic_string<CharT, Traits, Allocator> operator+(basic_string<CharT, Traits, Allocator> x, const CharT *s);
        template <typename CharT, typename Traits, typename Allocator>
        basic_string<CharT, Traits, Allocator> operator+(CharT c, basic_string<CharT, Traits, Allocator> y);
        template <typename CharT, typename Traits, typename Allocator>
        basic_string<CharT, Traits, Allocator> operator+(basic_string<CharT, Traits, Allocator> x, const CharT c);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator==(const basic_string<CharT, Traits, Allocator> &x,
                        const basic_string<CharT, Traits, Allocator> &y);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator==(const CharT *s, const basic_string<CharT, Traits, Allocator> &y);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator==(const basic_string<CharT, Traits, Allocator> &x, const CharT *s);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator!=(const basic_string<CharT, Traits, Allocator> &x,
                        const basic_string<CharT, Traits, Allocator> &y);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator!=(const CharT *s, const basic_string<CharT, Traits, Allocator> &y);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator!=(const basic_string<CharT, Traits, Allocator> &x, const CharT *s);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator<(const basic_string<CharT, Traits, Allocator> &x,
                       const basic_string<CharT, Traits, Allocator> &y);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator<(const CharT *s, const basic_string<CharT, Traits, Allocator> &y);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator<(const basic_string<CharT, Traits, Allocator> &x, const CharT *s);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator>(const basic_string<CharT, Traits, Allocator> &x,
                       const basic_string<CharT, Traits, Allocator> &y);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator>(const CharT *s, const basic_string<CharT, Traits, Allocator> &y);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator>(const basic_string<CharT, Traits, Allocator> &x, const CharT *s);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator<=(const basic_string<CharT, Traits, Allocator> &x,
                        const basic_string<CharT, Traits, Allocator> &y);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator<=(const CharT *s, const basic_string<CharT, Traits, Allocator> &y);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator<=(const basic_string<CharT, Traits, Allocator> &x, const CharT *s);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator>=(const basic_string<CharT, Traits, Allocator> &x,
                        const basic_string<CharT, Traits, Allocator> &y);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator>=(const CharT *s, const basic_string<CharT, Traits, Allocator> &y);
        template <typename CharT, typename Traits, typename Allocator>
        bool operator>=(const basic_string<CharT, Traits, Allocator> &x, const CharT *s);
        template <typename CharT, typename Traits, typename Allocator>
        void swap(basic_string<CharT, Traits, Allocator> &x, basic_string<CharT, Traits, Allocator> &y);
        template <typename CharT, typename Traits, typename Allocator>
        std::basic_ostream<CharT, Traits> &operator<<(std::basic_ostream<CharT, Traits> &os,
                                                      const basic_string<CharT, Traits, Allocator> &s);
        template <typename CharT, typename Traits, typename Allocator>
        std::basic_istream<CharT, Traits> &operator>>(std::basic_istream<CharT, Traits> &is,
                                                      basic_string<CharT, Traits, Allocator> &s);
        template <typename CharT, typename Traits, typename Allocator>
        std::basic_istream<CharT, Traits> &getline(std::istream &is,
                                                   basic_string<CharT, Traits, Allocator> &s,
                                                   CharT delim);
        template <typename CharT, typename Traits, typename Allocator>
        std::basic_istream<CharT, Traits> &getline(std::basic_istream<CharT, Traits> &is,
                                                   basic_string<CharT, Traits, Allocator> &s);
        template <typename Ch, typename Allocator>
        std::size_t hash_value(basic_string<Ch, std::char_traits<Ch>, Allocator> const &v);
    } // namespace container
} // namespace boost

namespace boost
{
    namespace container
    {
        void throw_bad_alloc();
        void throw_out_of_range(const char *);
        void throw_length_error(const char *);
        void throw_logic_error(const char *);
        void throw_runtime_error(const char *);
    } // namespace container
} // namespace boost

namespace boost
{
    namespace container
    {
        template <typename T, typename Allocator = std::allocator<T>>
        class vector;

        template <typename T, std::size_t N, typename Allocator = new_allocator<T>>
        class small_vector : public boost::container::small_vector_base<T, Allocator>
        {
          public:
            // construct/copy/destruct
            small_vector() noexcept(dtl::is_nothrow_default_constructible< Allocator >::value));
            explicit small_vector(const allocator_type &);
            explicit small_vector(size_type);
            small_vector(size_type, const allocator_type &);
            small_vector(size_type, default_init_t);
            small_vector(size_type, default_init_t, const allocator_type &);
            small_vector(size_type, const value_type &);
            small_vector(size_type, const value_type &, const allocator_type &);
            template <typename InIt>
            small_vector(InIt, InIt last);
            template <typename InIt>
            small_vector(InIt, InIt, const allocator_type &a);
            small_vector(const small_vector &);
            small_vector(const small_vector &, const allocator_type &);
            explicit small_vector(const base_type &);
            explicit small_vector(base_type &&);
            small_vector(small_vector &&);
            small_vector(small_vector &&, const allocator_type &);
            small_vector(std::initializer_list<value_type>, const allocator_type & = allocator_type());
            small_vector &operator=(const small_vector &);
            small_vector &operator=(small_vector &&);
            small_vector &operator=(const base_type &);
            small_vector &operator=(base_type &&);

            // public member functions
            void swap(small_vector &);
        };
    } // namespace container
} // namespace boost
