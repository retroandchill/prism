/**
 * @file operations.ixx
 * @author Francesco Corso
 * @date 8/8/2026
 * @brief
 */
export module prism.core:semantic.operations;

import std;

namespace prism
{
    export enum class UnaryOperation : std::uint8_t
    {
        identity,
        negation,
        logical_not,
        bitwise_not,
        pre_increment,
        pre_decrement,
        post_increment,
        post_decrement,
    };

    export enum class BinaryOperation : std::uint8_t
    {
        addition,
        subtraction,
        multiplication,
        division,
        modulo,
        bitwise_and,
        bitwise_or,
        bitwise_xor,
        logical_and,
        logical_or,
        equals,
        not_equals,
        less_than,
        less_than_or_equals,
        greater_than,
        greater_than_or_equals,
        three_way_comparison,
        shift_left,
        shift_right,
        unsigned_shift_right,
        null_coalescing,
    };

    export enum class AssignmentOperation : std::uint8_t
    {
        simple,
        addition,
        subtraction,
        multiplication,
        division,
        modulo,
        bitwise_and,
        bitwise_or,
        bitwise_xor,
        logical_and,
        logical_or,
        equals,
        not_equals,
        less_than,
        less_than_or_equals,
        greater_than,
        greater_than_or_equals,
        three_way_comparison,
        shift_left,
        shift_right,
        unsigned_shift_right,
        null_coalescing,
    };
} // namespace prism
