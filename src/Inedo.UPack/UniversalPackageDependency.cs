namespace Inedo.UPack
{
    /// <summary>
    /// Represents a universal package dependency specification.
    /// </summary>
    [Serializable]
    public sealed class UniversalPackageDependency : IEquatable<UniversalPackageDependency>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageDependency"/> class.
        /// </summary>
        /// <param name="id">The full identifier of the package.</param>
        /// <exception cref="ArgumentNullException"><paramref name="id"/> is null.</exception>
        public UniversalPackageDependency(UniversalPackageId id)
            : this(id, null)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageDependency"/> class.
        /// </summary>
        /// <param name="id">The full identifier of the package.</param>
        /// <param name="version">The required version; null indicates any version.</param>
        /// <exception cref="ArgumentNullException"><paramref name="id"/> is null.</exception>
        public UniversalPackageDependency(UniversalPackageId id, UniversalPackageVersion? version)
        {
            this.FullName = id ?? throw new ArgumentNullException(nameof(id));
            this.VersionRange = version;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageDependency"/> class.
        /// </summary>
        /// <param name="id">The full identifier of the package.</param>
        /// <param name="versionRange">The required version range.</param>
        /// <exception cref="ArgumentNullException"><paramref name="id"/> is null.</exception>
        public UniversalPackageDependency(UniversalPackageId id, UniversalPackageVersionRange versionRange)
        {
            this.FullName = id ?? throw new ArgumentNullException(nameof(id));
            this.VersionRange = versionRange;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageDependency"/> class.
        /// </summary>
        /// <param name="group">The package group.</param>
        /// <param name="name">The package name.</param>
        /// <param name="version">The required version; null indicates any version.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null or empty.</exception>
        public UniversalPackageDependency(string? group, string name, UniversalPackageVersion? version)
            : this(new UniversalPackageId(group, name), version)
        {
            this.FullName = new UniversalPackageId(group, name);
            this.VersionRange = version;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageDependency"/> class.
        /// </summary>
        /// <param name="group">The package group.</param>
        /// <param name="name">The package name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null or empty.</exception>
        public UniversalPackageDependency(string group, string name)
            : this(group, name, null)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageDependency"/> class.
        /// </summary>
        /// <param name="group">The package group.</param>
        /// <param name="name">The package name.</param>
        /// <param name="versionRange">The required version range.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null or empty.</exception>
        public UniversalPackageDependency(string? group, string name, UniversalPackageVersionRange versionRange)
        {
            this.FullName = new UniversalPackageId(group, name);
            this.VersionRange = versionRange;
        }

        /// <summary>
        /// Gets the group of the dependency.
        /// </summary>
        public string? Group => this.FullName.Group;
        /// <summary>
        /// Gets the name of the dependency.
        /// </summary>
        public string Name => this.FullName.Name;
        /// <summary>
        /// Gets the full identifier of the dependency.
        /// </summary>
        public UniversalPackageId FullName { get; }
        /// <summary>
        /// Gets the version of the dependency or null if any version is allowed.
        /// </summary>
        [Obsolete($"This property only returns the lower bound of a version range. Use the {nameof(VersionRange)} property instead.")]
        public UniversalPackageVersion? Version => this.VersionRange.LowerBound;
        /// <summary>
        /// Gets the version range of the dependency.
        /// </summary>
        public UniversalPackageVersionRange VersionRange { get; }

        /// <summary>
        /// Returns a <see cref="UniversalPackageDependency"/> instance parsed from the specified string.
        /// </summary>
        /// <param name="s">String containing the text to parse.</param>
        /// <returns>Parsed <see cref="UniversalPackageDependency"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is null or empty.</exception>
        public static UniversalPackageDependency Parse(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentNullException(nameof(s));

            var parts = s!.Split(new[] { ':' }, 3, StringSplitOptions.None);
            if (parts.Length == 1)
            {
                var n = ExtractGroup(parts[0]);
                return new UniversalPackageDependency(n);
            }

            if (parts.Length == 2)
            {
                if (parts[1] == "*")
                {
                    var n = ExtractGroup(parts[0]);
                    return new UniversalPackageDependency(n);
                }

                if (UniversalPackageVersionRange.TryParse(parts[1], out var v))
                {
                    var n = ExtractGroup(parts[0]);
                    return new UniversalPackageDependency(n, v);
                }

                return new UniversalPackageDependency(parts[0], parts[1]);
            }
            else
            {
                if (parts[2] == "*")
                    return new UniversalPackageDependency(parts[0], parts[1]);
                else
                    return new UniversalPackageDependency(parts[0], parts[1], UniversalPackageVersionRange.Parse(parts[2]));
            }
        }

        public override string ToString()
        {
            if (this.VersionRange == UniversalPackageVersionRange.Any)
                return this.FullName.ToString();
            else
                return this.FullName + ":" + this.VersionRange;
        }

        public bool Equals(UniversalPackageDependency? other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other is null)
                return false;

            return this.Group == other.Group
                && this.Name == other.Name
                && this.VersionRange == other.VersionRange;
        }
        public override bool Equals(object? obj) => this.Equals(obj as UniversalPackageDependency);
        public override int GetHashCode() => this.FullName.GetHashCode() ^ this.VersionRange.GetHashCode();

        private static UniversalPackageId ExtractGroup(string fullName)
        {
            string group;
            string name;
            int index = fullName.LastIndexOf('/');
            if (index >= 0)
            {
                group = fullName.Substring(0, index);
                name = fullName.Substring(index + 1);
            }
            else
            {
                group = string.Empty;
                name = fullName;
            }

            return new UniversalPackageId(group, name);
        }
    }
}
