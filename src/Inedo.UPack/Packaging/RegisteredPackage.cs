using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Inedo.UPack.Packaging
{
    /// <summary>
    /// Represents a package registration entry.
    /// </summary>
    public sealed class RegisteredPackage : IDictionary<string, object?>
    {
        private readonly Dictionary<string, object?> properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisteredPackage"/> class.
        /// </summary>
        public RegisteredPackage()
        {
            this.properties = new Dictionary<string, object?>();
        }
        internal RegisteredPackage(JObject obj)
        {
            this.properties = (Dictionary<string, object?>?)AH.CanonicalizeJsonToken(obj) ?? new Dictionary<string, object?>();
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
        /// Gets or sets the group of the package.
        /// </summary>
        public string? Group
        {
            get => (string?)this.GetPropertyValue("group");
            set => this.SetPropertyValue(value, "group");
        }
        /// <summary>
        /// Gets or sets the name of the package.
        /// </summary>
        public string? Name
        {
            get => (string?)this.GetPropertyValue("name");
            set => this.SetPropertyValue(value, "name");
        }
        /// <summary>
        /// Gets or sets the version of the package.
        /// </summary>
        public string? Version
        {
            get => (string?)this.GetPropertyValue("version");
            set => this.SetPropertyValue(value, "version");
        }
        /// <summary>
        /// Gets or sets the path where the package is installed.
        /// </summary>
        public string? InstallPath
        {
            get => (string?)this.GetPropertyValue("path");
            set => this.SetPropertyValue(value, "path");
        }
        /// <summary>
        /// Gets or sets the URL of feed where the package was installed from.
        /// </summary>
        public string? FeedUrl
        {
            get => (string?)this.GetPropertyValue("feedUrl");
            set => this.SetPropertyValue(value, "feedUrl");
        }
        /// <summary>
        /// Gets or sets the date of the package installation.
        /// </summary>
        public string? InstallationDate
        {
            get => (string?)this.GetPropertyValue("installationDate");
            set => this.SetPropertyValue(value, "installationDate");
        }
        /// <summary>
        /// Gets or sets the documented reason for the package installation.
        /// </summary>
        public string? InstallationReason
        {
            get => (string?)this.GetPropertyValue("installationReason");
            set => this.SetPropertyValue(value, "installationReason");
        }
        /// <summary>
        /// Gets or sets the name of the tool used to install the package.
        /// </summary>
        public string? InstalledUsing
        {
            get => (string?)this.GetPropertyValue("installedUsing");
            set => this.SetPropertyValue(value, "installedUsing");
        }
        /// <summary>
        /// Gets or sets the name of the user that installed the package.
        /// </summary>
        public string? InstalledBy
        {
            get => (string?)this.GetPropertyValue("installedBy");
            set => this.SetPropertyValue(value, "installedBy");
        }
        /// <summary>
        /// Gets a collection of all property names.
        /// </summary>
        public ICollection<string> Keys => this.properties.Keys;

        ICollection<object?> IDictionary<string, object?>.Values => this.properties.Values;
        int ICollection<KeyValuePair<string, object?>>.Count => this.properties.Count;
        bool ICollection<KeyValuePair<string, object?>>.IsReadOnly => false;

        /// <summary>
        /// Returns the full name of the package.
        /// </summary>
        /// <returns>Full name of the package.</returns>
        public override string ToString() => AH.FormatName(this.Group, this.Name ?? string.Empty);
        /// <summary>
        /// Gets a key/value pair enumerator for all properties.
        /// </summary>
        /// <returns>Key/value pair enumerator for all properties.</returns>
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => this.properties.GetEnumerator();
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

        internal Dictionary<string, object?> GetInternalDictionary() => this.properties;

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
