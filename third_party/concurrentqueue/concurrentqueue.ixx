/**
 * @file concurrentqueue.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <moodycamel/concurrentqueue.h>

export module concurrentqueue;

export namespace moodycamel
{
    using moodycamel::BlockingConcurrentQueue;
    using moodycamel::ConcurrentQueue;
    using moodycamel::ConcurrentQueueDefaultTraits;
    using moodycamel::ConsumerToken;
    using moodycamel::ProducerToken;
} // namespace moodycamel
