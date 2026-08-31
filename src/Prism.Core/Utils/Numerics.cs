using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using Cysharp.Text;
using Singulink.Numerics;

namespace Prism.Core.Utils;

public static class Numerics
{
    extension(char)
    {
        public static int HexDigitValue(char c)
        {
            return c switch
            {
                >= '0' and <= '9' => c - '0',
                >= 'a' and <= 'f' => c - 'a' + 10,
                >= 'A' and <= 'F' => c - 'A' + 10,
                _ => throw new ArgumentException("Invalid hex digit", nameof(c)),
            };
        }

        public static bool IsBinaryDigit(char c)
        {
            return c is '0' or '1';
        }
    }

    private static class BinaryIntegerLimits<T>
        where T : unmanaged, INumber<T>, IMinMaxValue<T>
    {
        public static readonly BigInteger MaxValue = BigInteger.CreateChecked(T.MaxValue);
        public static readonly BigInteger MinValue = BigInteger.CreateChecked(T.MinValue);
    }

    private static class BinaryFloatLimits<T>
        where T : IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        public static readonly int MaxExponent = int.CreateChecked(Log2Helper(T.MaxValue));
        public static readonly int MaxExponent10 = int.CreateChecked(T.Log10(T.MaxValue));
        public static readonly int Digits = GetBinaryPrecision();

        private static int GetBinaryPrecision()
        {
            // Radix is 2 for binary systems.
            // We can determine the explicit significand length using structural metadata or standard properties
            if (typeof(T) == typeof(float))
                return 24;
            if (typeof(T) == typeof(double))
                return 53;
            throw new NotSupportedException($"Unsupported type {typeof(T)}");
        }
    }

    private static T Log2Helper<T>(T value)
        where T : IBinaryNumber<T>
    {
        return T.Log2(value);
    }

    extension(BigInteger value)
    {
        internal bool FitsIn<T>()
            where T : unmanaged, INumber<T>, IMinMaxValue<T>
        {
            var max = BinaryIntegerLimits<T>.MaxValue;
            var min = BinaryIntegerLimits<T>.MinValue;
            return value <= max && value >= min;
        }

        internal int CountDecimalDigits()
        {
            Debug.Assert(value >= 0, "Value must be non-negative");

            var digits = 1;
            while (value >= 10)
            {
                value /= 10;
                ++digits;
            }

            return digits;
        }
    }

    extension<T>(T)
        where T : IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        internal static bool FitsInFiniteFloatMagnitude(BigInteger significand, int exponent10)
        {
            if (significand == 0)
                return true;

            var literalExponent10 = significand.CountDecimalDigits() * exponent10 - 1;
            if (literalExponent10 < BinaryFloatLimits<T>.MaxExponent10)
                return true;

            if (literalExponent10 > BinaryFloatLimits<T>.MaxExponent10)
                return false;

            var maxSignificand = (new BigInteger(1) << BinaryFloatLimits<T>.Digits) - 1;

            if (exponent10 >= 0)
            {
                var lhs = significand * BigInteger.Pow(5, exponent10);

                var binaryShift =
                    BinaryFloatLimits<T>.MaxExponent - BinaryFloatLimits<T>.Digits - exponent10;
                if (binaryShift >= 0)
                {
                    var rhs = maxSignificand << binaryShift;
                    return lhs <= rhs;
                }

                lhs <<= -binaryShift;
                return lhs <= maxSignificand;
            }
            else
            {
                var negativeExponent10 = -exponent10;
                var rhs = maxSignificand * BigInteger.Pow(5, negativeExponent10);
                var binaryShift = BinaryFloatLimits<T>.Digits - negativeExponent10;
                Debug.Assert(binaryShift >= 0);

                rhs <<= binaryShift;
                return significand <= rhs;
            }
        }

        internal static T ParseDecimalFloat(BigInteger significand, int exponent10, bool isNegative)
        {
            Debug.Assert(significand >= 0);

            if (significand == 0)
                return T.Zero;

            var digits = significand.ToString();
            var scientificExponent = exponent10 + digits.Length - 1;

            using var text = ZString.CreateStringBuilder();
            text.Append(digits[0]);
            if (digits.Length > 1)
            {
                text.Append('.');
                text.Append(digits.AsSpan(1));
            }

            text.Append('E');
            if (scientificExponent >= 0)
                text.Append('+');

            text.Append(scientificExponent);

            var result = T.Parse(text.AsSpan(), NumberStyles.Any, null);
            Debug.Assert(T.IsFinite(result));

            return isNegative ? -result : result;
        }
    }
}
