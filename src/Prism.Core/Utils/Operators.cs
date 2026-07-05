// @file Operators.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Semantic;
using Prism.Core.Semantic.Symbols;

namespace Prism.Core.Utils;

internal readonly record struct BinaryOperatorInfo(
    TypeSymbol LeftType,
    TypeSymbol RightType,
    TypeSymbol ResultType
);

internal static class Operators
{
    public static bool TryResolveUnary(
        UnaryOperator @operator,
        TypeSymbol type,
        out TypeSymbol resultType
    )
    {
        switch (@operator)
        {
            case UnaryOperator.LogicalNot
                when type is NamedTypeSymbol { BuiltInType: BuiltInType.Bool }:
                resultType = type;
                return true;
            case UnaryOperator.Negate
                when type is NamedTypeSymbol { BuiltInType: { } builtIn } && builtIn.IsSigned():
                resultType = type;
                return true;
            case UnaryOperator.BitNot
                when type is NamedTypeSymbol { BuiltInType: { } builtIn } && builtIn.IsInteger():
                resultType = type;
                return true;
            case UnaryOperator.Positive
            or UnaryOperator.PreIncrement
            or UnaryOperator.PostIncrement
            or UnaryOperator.PreDecrement
            or UnaryOperator.PostDecrement
                when type is NamedTypeSymbol { BuiltInType: { } builtIn } && builtIn.IsNumeric():
                resultType = type;
                return true;
        }

        resultType = ErrorTypeSymbol.Default;
        return false;
    }

    public static bool TryResolveBinary(
        BinaryOperator @operator,
        TypeSymbol leftType,
        TypeSymbol rightType,
        out BinaryOperatorInfo result
    )
    {
        throw new NotImplementedException();
    }
}
