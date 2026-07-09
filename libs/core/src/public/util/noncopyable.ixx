/**
 * @file noncopyable.ixx
 * @author Francesco Corso
 * @date 7/9/2026
 * @brief
 */
export module prism.core:util.noncopyable;

namespace prism
{
    /**
     * Mixin class to define a simple type that is not able to be copied or moved.
     */
    export class NonCopyable
    {
      protected:
        /**
         * Default constructor, marked as protected to prevent direct instantiation.
         */
        NonCopyable() = default;

        /**
         * Default no-op destructor, marked protected to discourage managing a pointer to this type.
         */
        ~NonCopyable() = default;

      public:
        NonCopyable(const NonCopyable &) = delete;
        NonCopyable(NonCopyable &&) = delete;

        NonCopyable &operator=(const NonCopyable &) = delete;
        NonCopyable &operator=(NonCopyable &&) = delete;
    };
} // namespace prism
