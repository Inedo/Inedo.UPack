using System;

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
        public UniversalPackageDependency(UniversalPackageId id, UniversalPackageVersion version)
        {
            this.FullName = id ?? throw new ArgumentNullException(nameof(id));
            this.Version = version;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageDependency"/> class.
        /// </summary>
        /// <param name="group">The package group.</param>
        /// <param name="name">The package name.</param>
        /// <param name="version">The required version; null indicates any version.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null or empty.</exception>
        public UniversalPackageDependency(string group, string name, UniversalPackageVersion version)
            : this(new UniversalPackageId(group, name), version)
        {
            this.FullName = new UniversalPackageId(group, name);
            this.Version = version;
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
        /// Gets the group of the dependency.
        /// </summary>
        public string Group => this.FullName.Group;
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
        public UniversalPackageVersion Version { get; }

        /// <summary>
        /// Returns a <see cref="UniversalPackageDependency"/> instance parsed from the specified string.
        /// </summary>
        /// <param name="s">String containing the text to parse.</param>
        /// <returns>Parsed <see cref="UniversalPackageDependency"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is null or empty.</exception>
        public static UniversalPackageDependency Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentNullException(nameof(s));

            var parts = s.Split(new[] { ':' }, 3, StringSplitOptions.None);
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

                var v = UniversalPackageVersion.TryParse(parts[1]);
                if (v != null)
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
                    return new UniversalPackageDependency(parts[0], parts[1], UniversalPackageVersion.Parse(parts[2]));
            }
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns>Striing representation of this instance.</returns>
        public override string ToString() => this.FullName + ":" + (this.Version?.ToString() ?? "*");
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool Equals(UniversalPackageDependency other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (ReferenceEquals(other, null))
                return false;

            return this.Group == other.Group
                && this.Name == other.Name
                && this.Version == other.Version;
        }
        public override bool Equals(object obj) => this.Equals(obj as UniversalPackageDependency);
        public override int GetHashCode()
        {
            int ver = 0;
            if (this.Version != null)
                ver = this.Version.GetHashCode();

            return this.FullName.GetHashCode() ^ ver;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

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
