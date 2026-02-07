// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake
{
    public static class StringUtil
    {
        public const string LowerAlpha = "abcdefghijklmnopqrstuvwxyz";
        public const string UpperAlpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string Numeric    = "0123456789";
        public const string Alpha      = LowerAlpha + UpperAlpha;
        public const string Alnum      = Alpha + Numeric;
        public const string Whitespace = " \f\n\r\t\v";

        public static int FindFirstNotOf(string InStr, string Charset, int Position = 0)
        {
            for (int i = Position; i < InStr.Length; i++)
            {
                if (!Charset.Contains(InStr[i]))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
