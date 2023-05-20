using System.Text.RegularExpressions;

namespace Inedo.UPack
{
    /// <summary>
    /// Uniquely identifies a universal package using a name and a group.
    /// </summary>
    [Serializable]
    public sealed class UniversalPackageId : IEquatable<UniversalPackageId>, IComparable<UniversalPackageId>, IComparable
    {
        private static readonly Regex GroupRegex = new(@"^[0-9A-Za-z\-\./_]+$", RegexOptions.Compiled);
        private static readonly Regex NameRegex = new(@"^[0-9A-Za-z\-\._]+$", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageId"/> class.
        /// </summary>
        /// <param name="name">The name of the package.</param>
        /// <exception cref="ArgumentException"><paramref name="name"/> is invalid.</exception>
        public UniversalPackageId(string name)
            : this(null, name)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageId"/> class.
        /// </summary>
        /// <param name="group">The group of the package.</param>
        /// <param name="name">The name of the package.</param>
        /// <exception cref="ArgumentException"><paramref name="group"/> is invalid or <paramref name="name"/> is invalid.</exception>
        public UniversalPackageId(string? group, string name)
        {
            if (!IsValidGroup(group))
                throw new ArgumentException("Invalid group.");
            if (!IsValidName(name))
                throw new ArgumentException("Invalid name.");

            this.Group = AH.NullIf(group, string.Empty);
            this.Name = name;
        }

        public static bool operator ==(UniversalPackageId? a, UniversalPackageId? b) => Equals(a, b);
        public static bool operator !=(UniversalPackageId? a, UniversalPackageId? b) => !Equals(a, b);

        /// <summary>
        /// Gets the group part of the identifier.
        /// </summary>
        public string? Group { get; }
        /// <summary>
        /// Gets the name part of the identifier.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Returns a <see cref="UniversalPackageId"/> instance parsed from the specified string.
        /// </summary>
        /// <param name="s">String containing the text to parse.</param>
        /// <returns>Parsed <see cref="UniversalPackageId"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is null or empty.</exception>
        public static UniversalPackageId Parse(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException(nameof(s));

            var parts = s.Split(new[] { ':' }, 2);
            if (parts.Length == 2)
                return new UniversalPackageId(parts[0], parts[1]);

            int i = s.LastIndexOf('/');
            if (i >= 0)
            {
                var group = s.Substring(0, i);
                var name = s.Substring(i + 1);
                return new UniversalPackageId(group, name);
            }

            return new UniversalPackageId(null, s);
        }
        public void Deconstruct(out string name)
        {
            name = this.Name;
        }
        public void Deconstruct(out string? group, out string name)
        {
            group = this.Group;
            name = this.Name;
        }
        /// <summary>
        /// Returns a value indicating whether the specified string is a valid upack group.
        /// </summary>
        /// <param name="s">String to test.</param>
        /// <returns>True if string can be used as a group; otherwise false.</returns>
        public static bool IsValidGroup(string? s)
        {
            if (string.IsNullOrEmpty(s))
                return true;

            if (!GroupRegex.IsMatch(s) || s!.Contains("//"))
                return false;

            return true;
        }
        /// <summary>
        /// Returns a value indicating whether the specified string is a valid upack name.
        /// </summary>
        /// <param name="s">String to test.</param>
        /// <returns>True if string can be used as a name; otherwise false.</returns>
        public static bool IsValidName(string s)
        {
            if (string.IsNullOrEmpty(s))
                return true;

            if (!NameRegex.IsMatch(s))
                return false;

            return true;
        }
        public static bool Equals(UniversalPackageId? a, UniversalPackageId? b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;

            return string.Equals(a.Group, b.Group, StringComparison.OrdinalIgnoreCase)
                && string.Equals(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
        }
        public static int Compare(UniversalPackageId? a, UniversalPackageId? b)
        {
            if (ReferenceEquals(a, b))
                return 0;
            if (a is null)
                return -1;
            if (b is null)
                return 1;

            int res = string.Compare(a.Group, b.Group, StringComparison.OrdinalIgnoreCase);
            if (res == 0)
                res = string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);

            return res;
        }

        public bool Equals(UniversalPackageId? other) => Equals(this, other);
        public int CompareTo(UniversalPackageId? other) => Compare(this, other);
        public override bool Equals(object? obj) => this.Equals(obj as UniversalPackageId);
        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(this.Group ?? string.Empty) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(this.Name);
        public override string ToString() => this.Group == null ? this.Name : (this.Group + "/" + this.Name);

        int IComparable.CompareTo(object? obj)
        {
            if (obj is not UniversalPackageId id)
                throw new ArgumentException("Object is not a " + nameof(UniversalPackageId));

            return this.CompareTo(id);
        }
    }
}
