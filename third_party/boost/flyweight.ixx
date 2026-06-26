/**
 * @file flyweight.ixx
 * @author Francesco Corso
 * @date 6/26/2026
 * @brief
 */
module;

#include <boost/flyweight.hpp>
#include <boost/flyweight/concurrent_factory.hpp>
#include <boost/flyweight/intermodule_holder.hpp>
#include <boost/flyweight/key_value.hpp>
#include <boost/flyweight/no_locking.hpp>
#include <boost/flyweight/no_tracking.hpp>
#include <boost/flyweight/set_factory.hpp>

export module boost.flyweight;

export namespace boost::flyweights
{
    using flyweights::flyweight;
    using flyweights::operator==;
    using flyweights::operator!=;
    using flyweights::operator>;
    using flyweights::operator>=;
    using flyweights::operator<;
    using flyweights::operator<=;
    using flyweights::hash_value;

    using flyweights::key_value;
    using flyweights::no_key_from_value;

    using flyweights::tag;

    using flyweights::factory;
    using flyweights::factory_marker;
    using flyweights::is_factory;

    using flyweights::hashed_factory;
    using flyweights::hashed_factory_class;

    using flyweights::concurrent_factory;
    using flyweights::concurrent_factory_class;

    using flyweights::set_factory;
    using flyweights::set_factory_class;

    using flyweights::assoc_container_factory;
    using flyweights::assoc_container_factory_class;

    using flyweights::holder;
    using flyweights::holder_marker;
    using flyweights::is_holder;

    using flyweights::static_holder;
    using flyweights::static_holder_class;

    using flyweights::intermodule_holder;
    using flyweights::intermodule_holder_class;

    using flyweights::is_locking;
    using flyweights::locking;
    using flyweights::locking_marker;

    using flyweights::simple_locking;

    using flyweights::no_locking;

    using flyweights::is_tracking;
    using flyweights::tracking;
    using flyweights::tracking_marker;

    using flyweights::no_tracking;
    using flyweights::refcounted;
} // namespace boost::flyweights
