using Prism.Core.Ast;
using Prism.Core.Semantic.Binding;
using Prism.Core.Utils;

namespace Prism.Core.Tests;

public class OperatorResolutionTests
{
    private static readonly TargetPlatform Platform = new() { PointerSize = 8 };

    [TestCase(BuiltInType.I8, BuiltInType.I8, BuiltInType.I32)]
    [TestCase(BuiltInType.I16, BuiltInType.I16, BuiltInType.I32)]
    [TestCase(BuiltInType.U8, BuiltInType.U8, BuiltInType.I32)]
    [TestCase(BuiltInType.U16, BuiltInType.U16, BuiltInType.I32)]
    [TestCase(BuiltInType.I8, BuiltInType.I16, BuiltInType.I32)]
    [TestCase(BuiltInType.I16, BuiltInType.U8, BuiltInType.I32)]
    [TestCase(BuiltInType.U8, BuiltInType.U16, BuiltInType.I32)]
    [TestCase(BuiltInType.U16, BuiltInType.I16, BuiltInType.I32)]
    [TestCase(BuiltInType.U16, BuiltInType.U32, BuiltInType.U32)]
    [TestCase(BuiltInType.U8, BuiltInType.U64, BuiltInType.U64)]
    [TestCase(BuiltInType.U16, BuiltInType.F32, BuiltInType.F32)]
    [TestCase(BuiltInType.U8, BuiltInType.F32, BuiltInType.F32)]
    [TestCase(BuiltInType.I32, BuiltInType.F32, BuiltInType.F64)]
    [TestCase(BuiltInType.I32, BuiltInType.U32, BuiltInType.I64)]
    public void BinaryArithmeticPromotion(BuiltInType left, BuiltInType right, BuiltInType common)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                Operators.TryResolveBinary(
                    BoundBinaryOperator.Add,
                    left,
                    right,
                    Platform,
                    out var result
                )
            );
            Assert.That(result.LeftType, Is.EqualTo(common));
            Assert.That(result.RightType, Is.EqualTo(common));
            Assert.That(result.ResultType, Is.EqualTo(common));
        }
    }

    [TestCase(BuiltInType.I128, BuiltInType.U128)]
    [TestCase(BuiltInType.I64, BuiltInType.F32)]
    [TestCase(BuiltInType.U64, BuiltInType.F64)]
    [TestCase(BuiltInType.Bool, BuiltInType.Bool)]
    public void InvalidBinaryArithmetic(BuiltInType left, BuiltInType right)
    {
        Assert.That(
            Operators.TryResolveBinary(BoundBinaryOperator.Add, left, right, Platform, out _),
            Is.False
        );
    }

    [TestCase(BuiltInType.I8, BuiltInType.I8, BuiltInType.I32)]
    [TestCase(BuiltInType.I16, BuiltInType.I16, BuiltInType.I32)]
    [TestCase(BuiltInType.U8, BuiltInType.U8, BuiltInType.I32)]
    [TestCase(BuiltInType.U16, BuiltInType.U16, BuiltInType.I32)]
    [TestCase(BuiltInType.I8, BuiltInType.I16, BuiltInType.I32)]
    [TestCase(BuiltInType.I16, BuiltInType.U8, BuiltInType.I32)]
    [TestCase(BuiltInType.U8, BuiltInType.U16, BuiltInType.I32)]
    [TestCase(BuiltInType.U16, BuiltInType.I16, BuiltInType.I32)]
    [TestCase(BuiltInType.U16, BuiltInType.U32, BuiltInType.U32)]
    [TestCase(BuiltInType.U8, BuiltInType.U64, BuiltInType.U64)]
    [TestCase(BuiltInType.U16, BuiltInType.F32, BuiltInType.F32)]
    [TestCase(BuiltInType.U8, BuiltInType.F32, BuiltInType.F32)]
    [TestCase(BuiltInType.I32, BuiltInType.F32, BuiltInType.F64)]
    [TestCase(BuiltInType.I32, BuiltInType.U32, BuiltInType.I64)]
    [TestCase(BuiltInType.Bool, BuiltInType.Bool, BuiltInType.Bool)]
    [TestCase(BuiltInType.Char, BuiltInType.Char, BuiltInType.Char)]
    [TestCase(BuiltInType.Char, BuiltInType.Char16, BuiltInType.Char16)]
    [TestCase(BuiltInType.Rune, BuiltInType.Char16, BuiltInType.Rune)]
    [TestCase(BuiltInType.Bool, BuiltInType.Bool, BuiltInType.Bool)]
    [TestCase(BuiltInType.Str, BuiltInType.Str, BuiltInType.Str)]
    public void BinaryEqualityPromotion(BuiltInType left, BuiltInType right, BuiltInType common)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                Operators.TryResolveBinary(
                    BoundBinaryOperator.Equal,
                    left,
                    right,
                    Platform,
                    out var result
                )
            );
            Assert.That(result.LeftType, Is.EqualTo(common));
            Assert.That(result.RightType, Is.EqualTo(common));
            Assert.That(result.ResultType, Is.EqualTo(BuiltInType.Bool));
        }
    }

    [TestCase(BuiltInType.I128, BuiltInType.U128)]
    [TestCase(BuiltInType.I64, BuiltInType.F32)]
    [TestCase(BuiltInType.U64, BuiltInType.F64)]
    public void InvalidBinaryComparison(BuiltInType left, BuiltInType right)
    {
        Assert.That(
            Operators.TryResolveBinary(BoundBinaryOperator.Add, left, right, Platform, out _),
            Is.False
        );
    }
}
