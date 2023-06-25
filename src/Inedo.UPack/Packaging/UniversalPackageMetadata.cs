using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Inedo.UPack.Packaging
{
    /// <summary>
    /// Represents metadata contained in a upack.json file.
    /// </summary>
    [Serializable]
    public sealed partial class UniversalPackageMetadata : IDictionary<string, object?>
    {
        private readonly Dictionary<string, object?> properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageMetadata"/> class.
        /// </summary>
        public UniversalPackageMetadata()
        {
            this.properties = new Dictionary<string, object?>();
            this.Dependencies = new DependencyList(this);
            this.RepackageHistory = new RepackageEntryList(this);
            this.Tags = new TagList(this);
        }
        internal UniversalPackageMetadata(JsonElement obj)
        {
            this.properties = (Dictionary<string, object?>?)AH.CanonicalizeJsonToken(obj) ?? new Dictionary<string, object?>();
            this.Dependencies = new DependencyList(this);
            this.RepackageHistory = new RepackageEntryList(this);
            this.Tags = new TagList(this);
        }

        /// <summary>
        /// Gets or sets the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Value of the property if defined; otherwise null.</returns>
        public object? this[string propertyName]
        {
            get => this.GetInternal(propertyName);
            set => this.AddInternal(propertyName, value);
        }

        /// <summary>
        /// Gets or sets the package group.
        /// </summary>
        public string? Group
        {
            get => (string?)this.GetPropertyValue("group");
            set => this.SetPropertyValue(value, "group");
        }
        /// <summary>
        /// Gets or sets the package name.
        /// </summary>
        public string? Name
        {
            get => (string?)this.GetPropertyValue("name");
            set => this.SetPropertyValue(value, "name");
        }
        /// <summary>
        /// Gets or sets the package vesion.
        /// </summary>
        public UniversalPackageVersion? Version
        {
            get => UniversalPackageVersion.Parse((string?)this.GetPropertyValue("version"));
            set => this.SetPropertyValue(value?.ToString(), "version");
        }
        /// <summary>
        /// Gets or sets the package title.
        /// </summary>
        public string? Title
        {
            get => (string?)this.GetPropertyValue("title");
            set => this.SetPropertyValue(value, "title");
        }
        /// <summary>
        /// Gets or sets the package description.
        /// </summary>
        public string? Description
        {
            get => (string?)this.GetPropertyValue("description");
            set => this.SetPropertyValue(value, "description");
        }
        /// <summary>
        /// Gets or sets the package short description (summary).
        /// </summary>
        public string? ShortDescription
        {
            get => (string?)this.GetPropertyValue("shortDescription");
            set => this.SetPropertyValue(value, "shortDescription");
        }
        /// <summary>
        /// Gets or sets the package icon URL.
        /// </summary>
        public string? Icon
        {
            get => (string?)this.GetPropertyValue("icon");
            set => this.SetPropertyValue(value, "icon");
        }

        /// <summary>
        /// Gets or sets date when the package was first created.
        /// </summary>
        public DateTimeOffset? CreatedDate
        {
            get => this.TryGetDateTime("createdDate");
            set => this.SetPropertyValue(value?.ToString("o"), "createdDate");
        }
        /// <summary>
        /// Gets or sets a string describing the reason or purpose of the creation.
        /// </summary>
        public string? CreatedReason
        {
            get => (string?)this.GetPropertyValue("createdReason");
            set => this.SetPropertyValue(value, "createdReason");
        }
        /// <summary>
        /// Gets or sets a string describing the mechanism the package was created with.
        /// </summary>
        public string? CreatedUsing
        {
            get => (string?)this.GetPropertyValue("createdUsing");
            set => this.SetPropertyValue(value, "createdUsing");
        }
        /// <summary>
        /// Gets or sets a string describing the person or service that performed the installation.
        /// </summary>
        public string? CreatedBy
        {
            get => (string?)this.GetPropertyValue("createdBy");
            set => this.SetPropertyValue(value, "createdBy");
        }

        /// <summary>
        /// Gets the package dependency list.
        /// </summary>
        public DependencyList Dependencies { get; }
        /// <summary>
        /// Gets the repackaging history.
        /// </summary>
        public RepackageEntryList RepackageHistory { get; }
        /// <summary>
        /// Gets the tags.
        /// </summary>
        public TagList Tags { get; }
        /// <summary>
        /// Gets a collection of all property names.
        /// </summary>
        public ICollection<string> Keys => this.properties.Keys;

        ICollection<object?> IDictionary<string, object?>.Values => this.properties.Values;
        int ICollection<KeyValuePair<string, object?>>.Count => this.properties.Count;
        bool ICollection<KeyValuePair<string, object?>>.IsReadOnly => false;

        /// <summary>
        /// Reads upack.json metadata from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="upackJsonStream"><see cref="Stream"/> containing upack.json metadata.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="upackJsonStream"/> is null.</exception>
        /// <exception cref="FormatException">JSON is invalid.</exception>
        public static UniversalPackageMetadata Parse(Stream upackJsonStream)
        {
            if (upackJsonStream == null)
                throw new ArgumentNullException(nameof(upackJsonStream));

            using var doc = JsonDocument.Parse(upackJsonStream);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                throw new FormatException("Expected JSON object in upack.json.");

            return new UniversalPackageMetadata(doc.RootElement);
        }

        /// <summary>
        /// Returns a shallow copy of all package metadata.
        /// </summary>
        /// <returns>Shallow copy of this instance.</returns>
        public UniversalPackageMetadata Clone()
        {
            var other = new UniversalPackageMetadata();
            foreach (var p in this.properties)
                other[p.Key] = p.Value;

            return other;
        }
        /// <summary>
        /// Returns a value indicating whether the specified property is defined.
        /// </summary>
        /// <param name="key">Name of the property.</param>
        /// <returns>True if <paramref name="key"/> is defined; otherwise false.</returns>
        public bool ContainsKey(string key) => this.properties.ContainsKey(key);
        /// <summary>
        /// Removes the property if it is defined.
        /// </summary>
        /// <param name="key">Name of the property.</param>
        /// <returns>True if property was removed; otherwise false.</returns>
        public bool Remove(string key) => this.properties.Remove(key);
        /// <summary>
        /// Gets a key/value pair enumerator for all properties.
        /// </summary>
        /// <returns>Key/value pair enumerator for all properties.</returns>
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => this.properties.GetEnumerator();

        internal void WriteJson(Utf8JsonWriter writer) => AH.WriteObject(writer, this.properties);

        private void AddInternal(string key, object? value) => this.properties[key] = value;
        private object? GetInternal(string propertyName) => this.properties.TryGetValue(propertyName, out var value) ? value : null;
        private object? GetPropertyValue(string propertyName) => this.GetInternal(propertyName);
        private void SetPropertyValue(object? value, string propertyName)
        {
            if (value != null)
                this.properties[propertyName] = value;
            else
                this.properties.Remove(propertyName);
        }
        private DateTimeOffset? TryGetDateTime(string propertyName)
        {
            if (this.GetInternal(propertyName) is string s && DateTimeOffset.TryParse(s, out var d))
                return d;
            else
                return null;
        }

        void IDictionary<string, object?>.Add(string key, object? value) => this.AddInternal(key, value);
        bool IDictionary<string, object?>.TryGetValue(string key, out object? value) => this.properties.TryGetValue(key, out value);
        void ICollection<KeyValuePair<string, object?>>.Add(KeyValuePair<string, object?> item) => this.AddInternal(item.Key, item.Value);
        void ICollection<KeyValuePair<string, object?>>.Clear() => this.properties.Clear();
        bool ICollection<KeyValuePair<string, object?>>.Contains(KeyValuePair<string, object?> item) => ((ICollection<KeyValuePair<string, object?>>)this.properties).Contains(item);
        void ICollection<KeyValuePair<string, object?>>.CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, object?>>)this.properties).CopyTo(array, arrayIndex);
        bool ICollection<KeyValuePair<string, object?>>.Remove(KeyValuePair<string, object?> item) => ((ICollection<KeyValuePair<string, object?>>)this.properties).Remove(item);
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
