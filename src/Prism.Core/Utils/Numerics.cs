namespace Prism.Core.Utils;

public static class Numerics
{
    extension(char)
    {
        public static int HexDigitValue(char c)
        {
            if (c is >= '0' and <= '9')
            {
                return c - '0';
            }

            if (c is >= 'a' and <= 'f')
            {
                return c - 'a' + 10;
            }

            if (c is >= 'A' and <= 'F')
            {
                return c - 'A' + 10;
            }

            throw new ArgumentException("Invalid hex digit", nameof(c));
        }

        public static bool IsBinaryDigit(char c)
        {
            return c is '0' or '1';
        }
    }
}
