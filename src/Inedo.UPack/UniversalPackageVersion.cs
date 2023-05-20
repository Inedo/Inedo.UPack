using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Inedo.UPack
{
    /// <summary>
    /// Represents a version used by universal packages. This version is compatible with semantic versioning 2.0.
    /// </summary>
    [Serializable]
    public sealed class UniversalPackageVersion : IEquatable<UniversalPackageVersion>, IComparable<UniversalPackageVersion>, IComparable, IFormattable
#if NET6_0_OR_GREATER
, ISpanFormattable
#endif
    {
        private static readonly char[] Dot = new[] { '.' };
        private static readonly Regex SemanticVersionRegex = new(
            @"^(?<1>[0-9]+)\.(?<2>[0-9]+)\.(?<3>[0-9]+)(-(?<4>[0-9a-zA-Z\.-]+))?(\+(?<5>[0-9a-zA-Z\.-]+))?$",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageVersion"/> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch number.</param>
        /// <param name="prerelease">The prerelease version string.</param>
        /// <param name="build">The build metadata.</param>
        public UniversalPackageVersion(BigInteger major, BigInteger minor, BigInteger patch, string? prerelease, string? build)
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
            this.Prerelease = AH.NullIf(prerelease, string.Empty);
            this.Build = AH.NullIf(build, string.Empty);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageVersion"/> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch number.</param>
        /// <param name="prerelease">The prerelease version string.</param>
        public UniversalPackageVersion(BigInteger major, BigInteger minor, BigInteger patch, string? prerelease)
            : this(major, minor, patch, prerelease, null)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageVersion"/> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch number.</param>
        public UniversalPackageVersion(BigInteger major, BigInteger minor, BigInteger patch)
            : this(major, minor, patch, null, null)
        {
        }

        public static bool operator ==(UniversalPackageVersion? a, UniversalPackageVersion? b) => Equals(a, b);
        public static bool operator !=(UniversalPackageVersion? a, UniversalPackageVersion? b) => !Equals(a, b);
        public static bool operator <(UniversalPackageVersion? a, UniversalPackageVersion? b) => Compare(a, b) < 0;
        public static bool operator >(UniversalPackageVersion? a, UniversalPackageVersion? b) => Compare(a, b) > 0;
        public static bool operator <=(UniversalPackageVersion? a, UniversalPackageVersion? b) => Compare(a, b) <= 0;
        public static bool operator >=(UniversalPackageVersion? a, UniversalPackageVersion? b) => Compare(a, b) >= 0;

        /// <summary>
        /// Gets the major version number.
        /// </summary>
        public BigInteger Major { get; }
        /// <summary>
        /// Gets the minor version number.
        /// </summary>
        public BigInteger Minor { get; }
        /// <summary>
        /// Gets the patch number.
        /// </summary>
        public BigInteger Patch { get; }
        /// <summary>
        /// Gets the prerelease string.
        /// </summary>
        public string? Prerelease { get; }
        /// <summary>
        /// Gets the build metadata.
        /// </summary>
        public string? Build { get; }

        public static UniversalPackageVersion? TryParse(string? s)
        {
            if (string.IsNullOrEmpty(s))
                return null;

            return ParseInternal(s, out _);
        }
        public static UniversalPackageVersion Parse(string? s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException(nameof(s));

            var version = ParseInternal(s, out var error);
            return version ?? throw new ArgumentException(error);
        }
        public void Deconstruct(out BigInteger major, out BigInteger minor, out BigInteger patch, out string? prerelease, out string? build)
        {
            major = this.Major;
            minor = this.Minor;
            patch = this.Patch;
            prerelease = this.Prerelease;
            build = this.Build;
        }
        public void Deconstruct(out BigInteger major, out BigInteger minor, out BigInteger patch, out string? prerelease)
        {
            major = this.Major;
            minor = this.Minor;
            patch = this.Patch;
            prerelease = this.Prerelease;
        }
        public void Deconstruct(out BigInteger major, out BigInteger minor, out BigInteger patch)
        {
            major = this.Major;
            minor = this.Minor;
            patch = this.Patch;
        }
        public static bool Equals(UniversalPackageVersion? a, UniversalPackageVersion? b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;

            return a.Major == b.Major
                && a.Minor == b.Minor
                && a.Patch == b.Patch
                && string.Equals(a.Prerelease, b.Prerelease, StringComparison.Ordinal)
                && string.Equals(a.Build, b.Build, StringComparison.Ordinal);
        }
        public static int Compare(UniversalPackageVersion? a, UniversalPackageVersion? b)
        {
            if (ReferenceEquals(a, b))
                return 0;
            if (a is null)
                return -1;
            if (b is null)
                return 1;

            int diff = a.Major.CompareTo(b.Major);
            if (diff != 0)
                return diff;

            diff = a.Minor.CompareTo(b.Minor);
            if (diff != 0)
                return diff;

            diff = a.Patch.CompareTo(b.Patch);
            if (diff != 0)
                return diff;

            diff = ComparePrerelease(a.Prerelease, b.Prerelease);
            if (diff != 0)
                return diff;

            diff = CompareBuild(a.Build, b.Build);
            if (diff != 0)
                return diff;

            return 0;
        }

        public bool Equals(UniversalPackageVersion? other) => Equals(this, other);
        public override bool Equals(object? obj) => this.Equals(obj as UniversalPackageVersion);
        public override int GetHashCode() => ((int)this.Major << 20) | ((int)this.Minor << 10) | (int)this.Patch;
        public override string ToString() => this.ToString(null);
        /// <summary>
        /// Returns a string representation of this version.
        /// </summary>
        /// <param name="format">Version format specification. See Remarks.</param>
        /// <param name="formatProvider">Unused.</param>
        /// <returns>String representation of the version.</returns>
        /// <exception cref="FormatException">Invalid value for <paramref name="format"/>.</exception>
        /// <remarks>
        /// <paramref name="format"/> may be one of:
        /// <list type="bullet">
        /// <item><c>G</c> - full version number, including build metadata (default)</item>
        /// <item><c>U</c> - unique version number (excludes build metadata)</item>
        /// </list>
        /// </remarks>
        public string ToString(string? format, IFormatProvider? formatProvider = null)
        {
            bool includeBuild = true;
            if (!string.IsNullOrEmpty(format))
            {
                includeBuild = format![0] switch
                {
                    'G' or 'g' => true,
                    'U' or 'u' => false,
                    _ => throw new FormatException("Invalid format specification; must be U or G.")
                };
            }

            var buffer = new StringBuilder(50);
            buffer.Append(this.Major);
            buffer.Append('.');
            buffer.Append(this.Minor);
            buffer.Append('.');
            buffer.Append(this.Patch);

            if (this.Prerelease != null)
            {
                buffer.Append('-');
                buffer.Append(this.Prerelease);
            }

            if (includeBuild && this.Build != null)
            {
                buffer.Append('+');
                buffer.Append(this.Build);
            }

            return buffer.ToString();
        }
        public int CompareTo(UniversalPackageVersion? other) => Compare(this, other);
        int IComparable.CompareTo(object? obj) => this.CompareTo(obj as UniversalPackageVersion);

        private static UniversalPackageVersion? ParseInternal(string? s, out string? error)
        {
            var match = SemanticVersionRegex.Match(s ?? string.Empty);
            if (!match.Success)
            {
                error = "String is not a valid semantic version.";
                return null;
            }

#if NET6_0_OR_GREATER
            var major = BigInteger.Parse(match.Groups[1].ValueSpan);
            var minor = BigInteger.Parse(match.Groups[2].ValueSpan);
            var patch = BigInteger.Parse(match.Groups[3].ValueSpan);
#else
            var major = BigInteger.Parse(match.Groups[1].Value);
            var minor = BigInteger.Parse(match.Groups[2].Value);
            var patch = BigInteger.Parse(match.Groups[3].Value);
#endif

            var prerelease = AH.NullIf(match.Groups[4].Value, string.Empty);
            var build = AH.NullIf(match.Groups[5].Value, string.Empty);

            error = null;
            return new UniversalPackageVersion(major, minor, patch, prerelease, build);
        }
        private static int ComparePrerelease(string? a, string? b)
        {
            if (a == null && b == null)
                return 0;
            if (a == null)
                return 1;
            if (b == null)
                return -1;

            var A = a.Split(Dot);
            var B = b.Split(Dot);

            int index = 0;
            while (true)
            {
                var aIdentifier = index < A.Length ? A[index] : null;
                var bIdentifier = index < B.Length ? B[index] : null;

                if (aIdentifier == null && bIdentifier == null)
                    break;
                if (aIdentifier == null)
                    return -1;
                if (bIdentifier == null)
                    return 1;

                bool aIntParsed = BigInteger.TryParse(aIdentifier, out var aInt);
                bool bIntParsed = BigInteger.TryParse(bIdentifier, out var bInt);

                int diff;
                if (aIntParsed && bIntParsed)
                {
                    diff = aInt.CompareTo(bInt);
                    if (diff != 0)
                        return diff;
                }
                else if (!aIntParsed && bIntParsed)
                {
                    return 1;
                }
                else if (aIntParsed)
                {
                    return -1;
                }
                else
                {
                    diff = string.CompareOrdinal(aIdentifier, bIdentifier);
                    if (diff != 0)
                        return diff;
                }

                index++;
            }

            return 0;
        }
        private static int CompareBuild(string? a, string? b)
        {
            if (a == null && b == null)
                return 0;
            if (a == null)
                return 1;
            if (b == null)
                return -1;

            bool isLeftNumeric = BigInteger.TryParse(a, out var leftNumeric);
            bool isRightNumeric = BigInteger.TryParse(b, out var rightNumeric);

            if (isLeftNumeric & isRightNumeric)
                return leftNumeric.CompareTo(rightNumeric);

            return string.CompareOrdinal(a, b);
        }

#if NET6_0_OR_GREATER
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            bool includeBuild = true;
            charsWritten = 0;
            if (!format.IsEmpty)
            {
                includeBuild = format[0] switch
                {
                    'G' or 'g' => true,
                    'U' or 'u' => false,
                    _ => throw new FormatException("Invalid format specification; must be U or G.")
                };
            }

            var dest = destination;
            if (!tryWriteValue(this.Major, ref dest, ref charsWritten))
                return false;

            if (!tryWriteChar('.', ref dest, ref charsWritten))
                return false;

            if (!tryWriteValue(this.Minor, ref dest, ref charsWritten))
                return false;

            if (!tryWriteChar('.', ref dest, ref charsWritten))
                return false;

            if (!tryWriteValue(this.Patch, ref dest, ref charsWritten))
                return false;

            if (this.Prerelease != null)
            {
                if (!tryWriteChar('-', ref dest, ref charsWritten))
                    return false;

                if (!this.Prerelease.AsSpan().TryCopyTo(dest))
                    return false;

                dest = dest[this.Prerelease.Length..];
                charsWritten += this.Prerelease.Length;
            }

            if (includeBuild && this.Build != null)
            {
                if (!tryWriteChar('+', ref dest, ref charsWritten))
                    return false;

                if (!this.Build.AsSpan().TryCopyTo(dest))
                    return false;

                dest = dest[this.Build.Length..];
                charsWritten += this.Build.Length;
            }

            return true;

            static bool tryWriteValue<T>(T value, ref Span<char> dest, ref int written) where T : ISpanFormattable
            {
                bool res = value.TryFormat(dest, out int charsWritten, default, null);
                dest = dest[charsWritten..];
                written += charsWritten;
                return res;
            }

            static bool tryWriteChar(char value, ref Span<char> dest, ref int written)
            {
                if (dest.IsEmpty)
                    return false;

                dest[0] = value;
                dest = dest[1..];
                written++;
                return true;
            }
        }
#endif
    }
}
