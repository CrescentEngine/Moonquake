// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL
{
    public struct LanguageVersion
    {
        public static readonly LanguageVersion Invalid = new LanguageVersion(0, 0, 0);
        public static readonly LanguageVersion Initial = new LanguageVersion(1, 0, 0);
        public static readonly LanguageVersion Latest  = new LanguageVersion(1, 1, 0);

        public int Major = 0, Minor = 0, Patch = 0;

        public LanguageVersion(int InMajor, int InMinor)
        {
            Major = InMajor;
            Minor = InMinor;
        }

        public LanguageVersion(int InMajor, int InMinor, int InPatch)
        {
            Major = InMajor;
            Minor = InMinor;
            Patch = InPatch;
        }

        public bool IsMoreRecent(LanguageVersion Other)
        {
            return Major > Other.Major || (Major == Other.Major && Minor > Other.Minor) || (Major == Other.Major && Minor == Other.Minor && Patch > Other.Patch);
        }

        public static LanguageVersion FromString(string Str)
        {
            if (string.IsNullOrWhiteSpace(Str))
            {
                throw new ArgumentException("Version string cannot be null or empty.", nameof(Str));
            }

            string[] Parts = Str.Split('.');
            if (Parts.Length < 2 || Parts.Length > 3)
            {
                throw new FormatException("Version string must be in format 'Major.Minor[.Patch]'");
            }

            return new LanguageVersion(int.Parse(Parts[0]), int.Parse(Parts[1]), Parts.Length == 3 ? int.Parse(Parts[2]) : 0);
        }
        public override string ToString() => $"{Major}.{Minor}.{Patch}";

        public static bool operator==(LanguageVersion Left, LanguageVersion Right)
        {
            return Left.Major == Right.Major
                && Left.Minor == Right.Minor
                && Left.Patch == Right.Patch;
        }

        public static bool operator!=(LanguageVersion Left, LanguageVersion Right)
        {
            return !(Left == Right);
        }

        public override bool Equals(object? Other)
        {
            if (Other is null)
            {
                return false;
            }
            if (Other is LanguageVersion OtherVersion)
            {
                return this == OtherVersion;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch);
        }
    }
}
