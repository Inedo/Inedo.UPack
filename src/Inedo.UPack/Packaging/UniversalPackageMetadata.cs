using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inedo.UPack.Packaging
{
    /// <summary>
    /// Represents metadata contained in a upack.json file.
    /// </summary>
    [Serializable]
    public sealed partial class UniversalPackageMetadata : IDictionary<string, object>
    {
        private Dictionary<string, object> properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageMetadata"/> class.
        /// </summary>
        public UniversalPackageMetadata()
        {
            this.properties = new Dictionary<string, object>();
            this.Dependencies = new DependencyList(this);
        }
        internal UniversalPackageMetadata(JObject obj)
        {
            this.properties = (Dictionary<string, object>)obj.ToObject(typeof(Dictionary<string, object>));
            this.Dependencies = new DependencyList(this);
        }

        /// <summary>
        /// Gets or sets the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Value of the property if defined; otherwise null.</returns>
        public object this[string propertyName]
        {
            get => this.GetPropertyValue(propertyName);
            set => this.SetPropertyValue(value, propertyName);
        }

        /// <summary>
        /// Gets or sets the package group.
        /// </summary>
        public string Group
        {
            get => (string)this.GetPropertyValue();
            set => this.SetPropertyValue(value);
        }
        /// <summary>
        /// Gets or sets the package name.
        /// </summary>
        public string Name
        {
            get => (string)this.GetPropertyValue();
            set => this.SetPropertyValue(value);
        }
        /// <summary>
        /// Gets or sets the package vesion.
        /// </summary>
        public UniversalPackageVersion Version
        {
            get => UniversalPackageVersion.Parse((string)this.GetPropertyValue());
            set => this.SetPropertyValue(value?.ToString());
        }
        /// <summary>
        /// Gets or sets the package title.
        /// </summary>
        public string Title
        {
            get => (string)this.GetPropertyValue();
            set => this.SetPropertyValue(value);
        }
        /// <summary>
        /// Gets or sets the package description.
        /// </summary>
        public string Description
        {
            get => (string)this.GetPropertyValue();
            set => this.SetPropertyValue(value);
        }
        /// <summary>
        /// Gets or sets the package icon URL.
        /// </summary>
        public string Icon
        {
            get => (string)this.GetPropertyValue();
            set => this.SetPropertyValue(value);
        }
        /// <summary>
        /// Gets the package dependency list.
        /// </summary>
        public DependencyList Dependencies { get; }
        /// <summary>
        /// Gets a collection of all property names.
        /// </summary>
        public ICollection<string> Keys => this.properties.Keys;

        ICollection<object> IDictionary<string, object>.Values => this.properties.Values;
        int ICollection<KeyValuePair<string, object>>.Count => this.properties.Count;
        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;

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
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => this.properties.GetEnumerator();

        internal void WriteJson(JsonTextWriter json) => new JsonSerializer().Serialize(json, this.properties);

        private void AddInternal(string key, object value)
        {
        }
        private object GetPropertyValue([CallerMemberName] string propertyName = null) => this.properties.TryGetValue(propertyName.ToLowerInvariant(), out var value) ? value : null;
        private void SetPropertyValue(object value, [CallerMemberName] string propertyName = null)
        {
            if (value != null)
                this.properties[propertyName.ToLowerInvariant()] = value;
            else
                this.properties.Remove(propertyName.ToLowerInvariant());
        }

        void IDictionary<string, object>.Add(string key, object value) => this.AddInternal(key, value);
        bool IDictionary<string, object>.TryGetValue(string key, out object value) => this.properties.TryGetValue(key, out value);
        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) => this.AddInternal(item.Key, item.Value);
        void ICollection<KeyValuePair<string, object>>.Clear() => this.properties.Clear();
        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) => ((ICollection<KeyValuePair<string, object>>)this.properties).Contains(item);
        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, object>>)this.properties).CopyTo(array, arrayIndex);
        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) => ((ICollection<KeyValuePair<string, object>>)this.properties).Remove(item);
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
