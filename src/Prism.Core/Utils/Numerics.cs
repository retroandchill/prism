using System.Diagnostics;
using System.Numerics;

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
}
