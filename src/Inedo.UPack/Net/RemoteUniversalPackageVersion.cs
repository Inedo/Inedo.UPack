using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace Inedo.UPack.Net
{
    /// <summary>
    /// Represents metadata for a specific version of a universal package contained in a remote feed.
    /// </summary>
    public sealed class RemoteUniversalPackageVersion
    {
        internal RemoteUniversalPackageVersion(JObject obj)
        {
            var group = (string)obj["group"];
            var name = (string)obj["name"];
            if (string.IsNullOrEmpty(name))
                throw new FormatException("Missing \"name\" property.");

            this.FullName = new UniversalPackageId(group, name);

            var version = (string)obj["version"];
            if (string.IsNullOrEmpty(version))
                throw new FormatException("Missing \"version\" property.");

            this.Version = UniversalPackageVersion.Parse(version);
            this.Title = (string)obj["title"];
            this.Icon = (string)obj["icon"];
            this.Description = (string)obj["description"];
            this.Size = (long)obj["size"];
            this.PublishedDate = (DateTimeOffset)obj["published"];
            this.Downloads = (int?)obj["downloads"] ?? 0;
            this.AllProperties = new ReadOnlyDictionary<string, object>((IDictionary<string, object>)obj.ToObject(typeof(Dictionary<string, object>)));
        }

        /// <summary>
        /// Gets the package group.
        /// </summary>
        public string Group => this.FullName.Group;
        /// <summary>
        /// Gets the package name.
        /// </summary>
        public string Name => this.FullName.Name;
        /// <summary>
        /// Gets the unique ID of the package (group and name).
        /// </summary>
        public UniversalPackageId FullName { get; }
        /// <summary>
        /// Gets the latest version of the package.
        /// </summary>
        public UniversalPackageVersion Version { get; }
        /// <summary>
        /// Gets the package title.
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// Gets the package description.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Gets the package icon URL.
        /// </summary>
        public string Icon { get; }
        /// <summary>
        /// Gets the size of the package in bytes.
        /// </summary>
        public long Size { get; }
        /// <summary>
        /// Gets the date and time when the package was published.
        /// </summary>
        public DateTimeOffset PublishedDate { get; }
        /// <summary>
        /// Gets the number of downloads of this version of the package.
        /// </summary>
        public int Downloads { get; }
        /// <summary>
        /// Gets all of the raw metadata for the package.
        /// </summary>
        public IReadOnlyDictionary<string, object> AllProperties { get; }

        /// <summary>
        /// Returns the package ID and version.
        /// </summary>
        /// <returns>Package ID and version.</returns>
        public override string ToString() => this.FullName + " " + this.Version;
    }
}
