﻿using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Inedo.UPack.Net
{
    /// <summary>
    /// Represents metadata for a universal package contained in a remote feed.
    /// </summary>
    /// <remarks>
    /// This class represents the package generically, not a specific version of it.
    /// </remarks>
    public sealed class RemoteUniversalPackage
    {
        internal RemoteUniversalPackage(JsonElement obj)
        {
            var group = obj.GetStringOrDefault("group");
            var name = obj.GetStringOrDefault("name");
            if (string.IsNullOrEmpty(name))
                throw new FormatException("Missing \"name\" property.");

            this.FullName = new UniversalPackageId(group, name!);

            var latestVersion = obj.GetStringOrDefault("latestVersion");
            if (string.IsNullOrEmpty(latestVersion))
                throw new FormatException("Missing \"latestVersion\" property.");

            this.LatestVersion = UniversalPackageVersion.Parse(latestVersion!);

            this.Title = obj.GetStringOrDefault("title");
            this.Icon = obj.GetStringOrDefault("icon");
            this.Description = obj.GetStringOrDefault("description");
            this.Downloads = obj.GetInt32OrDefault("downloads") ?? 0;

            var versionProp = obj.GetProperty("versions");
            var versions = new UniversalPackageVersion[versionProp.GetArrayLength()];
            int i = 0;
            foreach (var v in versionProp.EnumerateArray())
                versions[i++] = UniversalPackageVersion.Parse(v.GetString()!);

            this.AllVersions = versions;
            this.AllProperties = new ReadOnlyDictionary<string, object?>((IDictionary<string, object?>?)AH.CanonicalizeJsonToken(obj) ?? new Dictionary<string, object?>());
        }
        internal RemoteUniversalPackage(JsonObject obj)
        {
            var group = (string?)obj["group"];
            var name = (string?)obj["name"];
            if (string.IsNullOrEmpty(name))
                throw new FormatException("Missing \"name\" property.");

            this.FullName = new UniversalPackageId(group, name!);

            var latestVersion = (string?)obj["latestVersion"];
            if (string.IsNullOrEmpty(latestVersion))
                throw new FormatException("Missing \"latestVersion\" property.");

            this.LatestVersion = UniversalPackageVersion.Parse(latestVersion!);

            this.Title = (string?)obj["title"];
            this.Icon = (string?)obj["icon"];
            this.Description = (string?)obj["description"];
            this.Downloads = int.TryParse(obj["downloads"]?.ToString(), out int d) ? d : 0;

            this.AllVersions = Array.AsReadOnly(((JsonArray?)obj["versions"])!.Select(t => UniversalPackageVersion.Parse((string)t!)).ToArray());
            this.AllProperties = new ReadOnlyDictionary<string, object?>((IDictionary<string, object?>?)AH.CanonicalizeJsonToken(obj) ?? new Dictionary<string, object?>());
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
        public UniversalPackageVersion LatestVersion { get; }
        /// <summary>
        /// Gets the package title.
        /// </summary>
        public string? Title { get; }
        /// <summary>
        /// Gets the package icon URL.
        /// </summary>
        public string? Icon { get; }
        /// <summary>
        /// Gets the package description.
        /// </summary>
        public string? Description { get; }
        /// <summary>
        /// Gets the number of downloads of all versions of the package.
        /// </summary>
        public int Downloads { get; }
        /// <summary>
        /// Gets all of the versions of the package in descending order.
        /// </summary>
        public IReadOnlyList<UniversalPackageVersion> AllVersions { get; }
        /// <summary>
        /// Gets all of the raw metadata for the package.
        /// </summary>
        public IReadOnlyDictionary<string, object?> AllProperties { get; }

        /// <summary>
        /// Returns the package ID.
        /// </summary>
        /// <returns>Package ID.</returns>
        public override string ToString() => this.FullName.ToString();
    }
}
