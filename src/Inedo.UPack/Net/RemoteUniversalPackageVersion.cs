using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Inedo.UPack.Net
{
    /// <summary>
    /// Represents metadata for a specific version of a universal package contained in a remote feed.
    /// </summary>
    public sealed class RemoteUniversalPackageVersion
    {
        internal RemoteUniversalPackageVersion(JsonElement obj)
        {
            var group = obj.GetStringOrDefault("group");
            var name = obj.GetStringOrDefault("name");
            if (string.IsNullOrEmpty(name))
                throw new FormatException("Missing \"name\" property.");

            this.FullName = new UniversalPackageId(group, name!);

            var version = obj.GetStringOrDefault("version");
            if (string.IsNullOrEmpty(version))
                throw new FormatException("Missing \"version\" property.");

            this.Version = UniversalPackageVersion.Parse(version!);
            this.Title = obj.GetStringOrDefault("title");
            this.Icon = obj.GetStringOrDefault("icon");
            this.Description = obj.GetStringOrDefault("description");
            this.Size = obj.GetInt64OrDefault("size") ?? 0;
            this.PublishedDate = obj.GetDateTimeOffsetOrDefault("published") ?? default;
            this.Downloads = obj.GetInt32OrDefault("downloads") ?? 0;
            var sha1String = obj.GetStringOrDefault("sha1");
            if (!string.IsNullOrEmpty(sha1String))
                this.SHA1 = HexString.Parse(sha1String!);

            if (obj.TryGetProperty("tags", out var tagsProp) && tagsProp.ValueKind == JsonValueKind.Array && tagsProp.GetArrayLength() > 0)
            {
                var tags = new string[tagsProp.GetArrayLength()];
                int i = 0;
                foreach (var t in tagsProp.EnumerateArray())
                    tags[i++] = t.GetString()!;

                this.Tags = tags;
            }
            else
            {
                this.Tags = Array.Empty<string>();
            }

            this.AllProperties = new ReadOnlyDictionary<string, object>((IDictionary<string, object>?)AH.CanonicalizeJsonToken(obj) ?? new Dictionary<string, object>());

            if (obj.TryGetProperty("dependencies", out var depsProp) && depsProp.ValueKind == JsonValueKind.Array && depsProp.GetArrayLength() > 0)
            {
                var deps = new UniversalPackageDependency[depsProp.GetArrayLength()];
                int i = 0;
                foreach (var d in depsProp.EnumerateArray())
                    deps[i++] = UniversalPackageDependency.Parse(d.GetString());

                this.Dependencies = deps;
            }
            else
            {
                this.Dependencies = Array.Empty<UniversalPackageDependency>();
            }
        }
        internal RemoteUniversalPackageVersion(JsonObject obj)
        {
            var group = (string?)obj["group"];
            var name = (string?)obj["name"];
            if (string.IsNullOrEmpty(name))
                throw new FormatException("Missing \"name\" property.");

            this.FullName = new UniversalPackageId(group, name!);

            var version = (string?)obj["version"];
            if (string.IsNullOrEmpty(version))
                throw new FormatException("Missing \"version\" property.");

            this.Version = UniversalPackageVersion.Parse(version!);
            this.Title = (string?)obj["title"];
            this.Icon = (string?)obj["icon"];
            this.Description = (string?)obj["description"];
            this.Size = (long?)obj["size"] ?? 0;
            this.PublishedDate = (DateTimeOffset?)obj["published"] ?? default;
            this.Downloads = (int?)obj["downloads"] ?? 0;
            var sha1String = (string?)obj["sha1"];
            if (!string.IsNullOrEmpty(sha1String))
                this.SHA1 = HexString.Parse(sha1String!);

            this.Tags = ((JsonArray?)obj["tags"])?.Select(t => (string)t!)?.ToArray() ?? Array.Empty<string>();
            this.AllProperties = new ReadOnlyDictionary<string, object>((IDictionary<string, object>?)AH.CanonicalizeJsonToken(obj) ?? new Dictionary<string, object>());
            this.Dependencies = ((JsonArray?)obj["dependencies"])?.Select(d => UniversalPackageDependency.Parse((string?)d))?.ToArray() ?? Array.Empty<UniversalPackageDependency>();
        }

        /// <summary>
        /// Gets the package group.
        /// </summary>
        public string? Group => this.FullName.Group;
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
        public string? Title { get; }
        /// <summary>
        /// Gets the package description.
        /// </summary>
        public string? Description { get; }
        /// <summary>
        /// Gets the package icon URL.
        /// </summary>
        public string? Icon { get; }
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
        /// Gets the SHA1 hash of the package.
        /// </summary>
        public HexString SHA1 { get; }
        /// <summary>
        /// Gets all of the raw metadata for the package.
        /// </summary>
        public IReadOnlyDictionary<string, object> AllProperties { get; }
        /// <summary>
        /// Gets the package's tags.
        /// </summary>
        public IReadOnlyCollection<string> Tags { get; }
        /// <summary>
        /// Gets the package's dependencies.
        /// </summary>
        public IReadOnlyCollection<UniversalPackageDependency> Dependencies { get; }

        /// <summary>
        /// Returns the package ID and version.
        /// </summary>
        /// <returns>Package ID and version.</returns>
        public override string ToString() => this.FullName + " " + this.Version;
    }
}
