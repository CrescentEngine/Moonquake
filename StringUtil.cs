// Copyright (C) 2026 ychgen, all rights reserved.

using System.Collections.Concurrent;
using System.Text.RegularExpressions;

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

        private static readonly ConcurrentDictionary<string, Regex> PatternCache = new();
        public static readonly string[] HeaderFileExtensions = [
            ".h", ".hh", ".hpp", ".hxx", ".h++"
        ];
        public static readonly string[] TranslationUnitExtensions = [
            ".cpp", ".c", ".cc", ".cxx", ".c++"
        ];

        public static bool IsHeaderFile(string Filepath)
        {
            foreach (string HeaderFileExtension in HeaderFileExtensions)
            {
                if (Filepath.EndsWith(HeaderFileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsTranslationUnit(string Filepath)
        {
            foreach (string TranslationUnitExtension in TranslationUnitExtensions)
            {
                if (Filepath.EndsWith(TranslationUnitExtension, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Equivalent to std::basic_string<T, Traits>::find_first_not_of(chset, pos)
        /// </summary>
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

        public static void ClearPatternCache()
        {
            PatternCache.Clear();
        }
        public static bool IsMatch(string Str, string Pattern)
        {
            Regex Rgx = PatternCache.GetOrAdd(Pattern, p =>
            {
                string Escaped = Regex.Escape(p);
                string Final   = "^" + Escaped.Replace(@"\*", ".*?") + "$";
                return new Regex(Final, RegexOptions.Compiled);
            });

            return Rgx.IsMatch(Str);
        }
    }
}
