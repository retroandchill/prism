/**
 * @file buffer_pool.ixx
 * @author Francesco Corso
 * @date 7/2/2026
 * @brief
 */
module;

#include "prism/core/exports.h"

export module prism.core.memory.buffer_pool;

import std;
import prism.core.util;

namespace prism
{
    class PooledBuffer
    {
      public:
        constexpr explicit PooledBuffer(std::size_t size) : buffer_{std::make_unique<std::byte[]>(size)}, size_{size}
        {
        }

        [[nodiscard]] constexpr std::span<std::byte> span() const noexcept
        {
            return {buffer_.get(), size_};
        }

      private:
        std::unique_ptr<std::byte[]> buffer_;
        std::size_t size_;
    };

    class BufferPoolBucket
    {
      public:
        constexpr explicit BufferPoolBucket(const std::size_t size) : size_{size}
        {
            static constexpr std::size_t initial_capacity = 32;
            buffers_.reserve(initial_capacity);
            allocated_indices_.reserve(initial_capacity);
        }

        ~BufferPoolBucket() = default;

        BufferPoolBucket(const BufferPoolBucket &) = delete;
        BufferPoolBucket(BufferPoolBucket &&) noexcept = default;
        BufferPoolBucket &operator=(const BufferPoolBucket &) = delete;
        BufferPoolBucket &operator=(BufferPoolBucket &&) noexcept = default;

        [[nodiscard]] constexpr std::size_t size() const noexcept
        {
            return size_;
        }

        constexpr void *allocate() noexcept
        {
            if (free_indices_.empty())
            {
                auto new_index = buffers_.size();
                const auto &new_buffer = buffers_.emplace_back(size_);
                allocated_indices_.emplace(new_buffer.span().data(), new_index);
                return new_buffer.span().data();
            }

            const auto free_index = free_indices_.front();
            free_indices_.pop_front();
            return buffers_[free_index].span().data();
        }

        constexpr void deallocate(const void *buffer) noexcept
        {
            const auto index = allocated_indices_[buffer];
            free_indices_.push_back(index);
        }

      private:
        std::size_t size_;
        std::vector<PooledBuffer> buffers_;
        std::deque<std::size_t> free_indices_;
        std::unordered_map<const void *, std::size_t> allocated_indices_;
    };

    export class PRISM_CORE_API BufferPool
    {
        static constexpr std::size_t min_bucket_size = 32;
        static constexpr std::size_t max_bucket_size = 4096;

        static constexpr std::size_t index_offset = std::bit_width(min_bucket_size);
        static constexpr std::size_t bucket_count = std::bit_width(max_bucket_size) - index_offset + 1;

      public:
        static BufferPool &shared();

        constexpr std::allocation_result<void *> allocate(const std::size_t size)
        {
            const auto bucket_index = bucket_index_for(size);
            if (bucket_index >= buckets_.size())
            {
                auto [ptr, count] = fallback_allocator_.allocate_at_least(size);
                return {ptr, count};
            }

            auto &target_bucket = buckets_[bucket_index];
            return {target_bucket.allocate(), target_bucket.size()};
        }

        constexpr void deallocate(void *buffer, const std::size_t size) noexcept
        {
            const auto bucket_index = bucket_index_for(size);
            if (bucket_index >= buckets_.size())
            {
                fallback_allocator_.deallocate(static_cast<std::byte *>(buffer), size);
                return;
            }

            buckets_[bucket_index].deallocate(buffer);
        }

      private:
        constexpr static std::size_t bucket_index_for(const std::size_t size) noexcept
        {
            if (size <= min_bucket_size)
                return 0;

            return std::bit_width(size - 1) - index_offset;
        }

        std::array<BufferPoolBucket, bucket_count> buckets_{make_array<BufferPoolBucket, bucket_count>(
            [](const std::size_t i) { return BufferPoolBucket{static_cast<std::size_t>(1) << (i + index_offset)}; })};
        std::allocator<std::byte> fallback_allocator_;
    };

    export template <typename T>
    class BufferPoolAllocator
    {
      public:
        using value_type = T;

        BufferPoolAllocator() = default;

        constexpr explicit BufferPoolAllocator(BufferPool &pool) noexcept : pool_{&pool}
        {
        }

        template <typename U>
            requires(!std::same_as<T, U>)
        constexpr explicit(false) BufferPoolAllocator(const BufferPoolAllocator<U> &other) noexcept : pool_{other.pool_}
        {
        }

        [[nodiscard]] T *allocate(const std::size_t size)
        {
            return allocate_at_least(size).ptr;
        }

        [[nodiscard]] std::allocation_result<T *> allocate_at_least(const std::size_t size)
        {
            if (size > std::numeric_limits<std::size_t>::max() / sizeof(T))
                throw std::bad_array_new_length{};

            const auto byte_count = size * sizeof(T);
            auto [ptr, count] = pool_->allocate(byte_count);
            return {static_cast<T *>(ptr), count};
        }

        void deallocate(T *ptr, const std::size_t size)
        {
            pool_->deallocate(ptr, size * sizeof(T));
        }

        constexpr friend bool operator==(BufferPoolAllocator lhs, BufferPoolAllocator rhs) noexcept = default;

        template <typename U>
            requires(!std::same_as<T, U>)
        constexpr friend bool operator==(BufferPoolAllocator lhs, const BufferPoolAllocator<U> &rhs) noexcept
        {
            return lhs.pool_ == rhs.pool_;
        }

      private:
        template <typename U>
        friend class BufferPoolAllocator;

        BufferPool *pool_ = &BufferPool::shared();
    };

    export template <typename T>
    using PooledVector = std::vector<T, BufferPoolAllocator<T>>;
} // namespace prism
