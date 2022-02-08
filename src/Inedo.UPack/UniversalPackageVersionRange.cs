using System;
using System.Text;

namespace Inedo.UPack
{
    /// <summary>
    /// Represents a range of Universal Package versions.
    /// </summary>
    public readonly struct UniversalPackageVersionRange : IEquatable<UniversalPackageVersionRange>
    {
        /// <summary>
        /// Represents an unbound range which matches any version (equivalent to *).
        /// </summary>
        public static readonly UniversalPackageVersionRange Any = default;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageVersionRange"/> struct that matches a single version.
        /// </summary>
        /// <param name="exactVersion">The single version to specify.</param>
        public UniversalPackageVersionRange(UniversalPackageVersion exactVersion) : this(exactVersion, false, exactVersion, false)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageVersionRange"/> struct.
        /// </summary>
        /// <param name="lowerBound">The lower version number.</param>
        /// <param name="lowerExclusive">Value indicating whether <paramref name="lowerBound"/> is excluded from the range.</param>
        /// <param name="upperBound">The upper version number.</param>
        /// <param name="upperExclusive">Value indicating whether <paramref name="upperBound"/> is excluded from the range.</param>
        public UniversalPackageVersionRange(UniversalPackageVersion? lowerBound, bool lowerExclusive, UniversalPackageVersion? upperBound, bool upperExclusive)
        {
            this.LowerBound = lowerBound;
            this.LowerExclusive = lowerExclusive;
            this.UpperBound = upperBound;
            this.UpperExclusive = upperExclusive;
        }

        /// <summary>
        /// Returns a <see cref="UniversalPackageVersionRange"/> that matches a single version.
        /// </summary>
        /// <param name="version">Single version to include in the range.</param>
        public static implicit operator UniversalPackageVersionRange(UniversalPackageVersion? version) => version is null ? Any : new(version);

        public static bool operator ==(UniversalPackageVersionRange a, UniversalPackageVersionRange b) => Equals(a, b);
        public static bool operator !=(UniversalPackageVersionRange a, UniversalPackageVersionRange b) => !Equals(a, b);

        /// <summary>
        /// Gets the lower bound of the range if applicable; <c>null</c> indicates an unbounded lower version.
        /// </summary>
        public UniversalPackageVersion? LowerBound { get; }
        /// <summary>
        /// Gets the lower bound of the range if applicable; <c>null</c> indicates an unbounded upper version.
        /// </summary>
        public UniversalPackageVersion? UpperBound { get; }
        /// <summary>
        /// Gets a value indicating whether <see cref="LowerBound"/> is excluded from the range.
        /// </summary>
        public bool LowerExclusive { get; }
        /// <summary>
        /// Gets a value indicating whether <see cref="UpperBound"/> is excluded from the range.
        /// </summary>
        public bool UpperExclusive { get; }

        /// <summary>
        /// Converts a string to a <see cref="UniversalPackageVersionRange"/> instance.
        /// </summary>
        /// <param name="s">String to parse. See Remarks for the format.</param>
        /// <returns>Parsed <see cref="UniversalPackageVersionRange"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is null or empty.</exception>
        /// <exception cref="FormatException"><paramref name="s"/> is not a valid version range.</exception>
        /// <remarks>
        /// Version range specification examples:
        /// <list type="bullet">
        /// <item><c>*</c> - an unbounded range which matches any version</item>
        /// <item><c>1.0.0</c> - matches only v1.0.0</item>
        /// <item><c>[1.0.0,2.0.0)</c> - matches every version from 1.0.0 up to but not including 2.0.0</item>
        /// <item><c>[3.0.0,]</c> - matches every version starting with 3.0.0 and higher</item>
        /// </list>
        /// </remarks>
        public static UniversalPackageVersionRange Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentNullException(nameof(s));

            if (TryParse(s, out var value))
                return value;
            else
                throw new FormatException("Invalid version range specification.");
        }
        /// <summary>
        /// Attempts to convert a string to a <see cref="UniversalPackageVersionRange"/> instance.
        /// </summary>
        /// <param name="s">String to parse. See Remarks for the format.</param>
        /// <param name="value">Parsed <see cref="UniversalPackageVersionRange"/> instance.</param>
        /// <returns>Value indicating whether <paramref name="s"/> was successfully parsed.</returns>
        /// <remarks>
        /// Version range specification examples:
        /// <list type="bullet">
        /// <item><c>*</c> - an unbounded range which matches any version</item>
        /// <item><c>1.0.0</c> - matches only v1.0.0</item>
        /// <item><c>[1.0.0,2.0.0)</c> - matches every version from 1.0.0 up to but not including 2.0.0</item>
        /// <item><c>[3.0.0,]</c> - matches every version starting with 3.0.0 and higher</item>
        /// </list>
        /// </remarks>
        public static bool TryParse(string s, out UniversalPackageVersionRange value)
        {
            value = default;

            if (string.IsNullOrWhiteSpace(s))
                return false;

            s = s.Trim();
            if (s == "*")
            {
                value = Any;
                return true;
            }

            var ver = UniversalPackageVersion.TryParse(s);
            if (ver != null)
            {
                value = new UniversalPackageVersionRange(ver);
                return true;
            }

            bool lowerExclusive;
            if (s[0] == '(')
                lowerExclusive = true;
            else if (s[0] == '[')
                lowerExclusive = false;
            else
                return false;

            bool upperExclusive;
            if (s[s.Length - 1] == ')')
                upperExclusive = true;
            else if (s[s.Length - 1] == ']')
                upperExclusive = false;
            else
                return false;

            var parts = s.Substring(1, s.Length - 2).Split(new[] { ',' }, 2);
            if (parts.Length != 2)
                return false;

            var lower = UniversalPackageVersion.TryParse(parts[0]);
            if (lower == null)
                return false;

            var upper = UniversalPackageVersion.TryParse(parts[1]);
            if (upper == null)
                return false;

            value = new UniversalPackageVersionRange(lower, lowerExclusive, upper, upperExclusive);
            return true;
        }

        /// <summary>
        /// Determines whether two <see cref="UniversalPackageVersionRange"/> instances are equivalent.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <returns>True if the ranges are the same; otherwise false.</returns>
        public static bool Equals(UniversalPackageVersionRange a, UniversalPackageVersionRange b)
        {
            // when both lower and upper are the same, ignore exclusivity
            if (a.LowerBound == b.LowerBound && a.UpperBound == b.UpperBound)
                return true;

            if (a.LowerBound == b.UpperBound)
            {
                if (a.LowerBound is not null && a.LowerExclusive != b.LowerExclusive)
                    return false;
            }
            else
            {
                return false;
            }

            if (a.UpperBound == b.UpperBound)
            {
                if (a.UpperBound is not null && a.UpperExclusive != b.UpperExclusive)
                    return false;
            }
            else
            {
                return false;
            }

            return true;
        }

        public bool Equals(UniversalPackageVersionRange other) => Equals(this, other);
        public override string ToString()
        {
            if (this.LowerBound is null && this.UpperBound is null)
                return "*";

            if (this.LowerBound == this.UpperBound)
                return this.LowerBound!.ToString();

            var sb = new StringBuilder();
            sb.Append(this.LowerExclusive ? '(' : '[');
            sb.Append(this.LowerBound?.ToString() ?? string.Empty);
            sb.Append(',');
            sb.Append(this.UpperBound?.ToString() ?? string.Empty);
            sb.Append(this.UpperExclusive ? ')' : ']');
            return sb.ToString();
        }
        public override bool Equals(object? obj) => obj is UniversalPackageVersionRange v && this.Equals(v);
        public override int GetHashCode() => this.LowerBound?.GetHashCode() ?? 0;
    }
}
