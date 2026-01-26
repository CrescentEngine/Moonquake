// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake
{
    public static class StringUtil
    {
        public static readonly string LowerAlpha = "abcdefghijklmnopqrstuvwxyz";
        public static readonly string UpperAlpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static readonly string Numeric    = "0123456789";
        public static readonly string Alpha      = LowerAlpha + UpperAlpha;
        public static readonly string Alnum      = Alpha + Numeric;
        public static readonly string Whitespace = " \f\n\r\t\v";

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
