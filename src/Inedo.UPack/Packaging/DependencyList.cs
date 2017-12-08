using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Inedo.UPack.Packaging
{
    partial class UniversalPackageMetadata
    {
        /// <summary>
        /// Represents a parsed list of package dependencies.
        /// </summary>
        [Serializable]
        public sealed class DependencyList : IList<UniversalPackageDependency>
        {
            private const string Key = "dependencies";
            private readonly UniversalPackageMetadata owner;

            internal DependencyList(UniversalPackageMetadata owner)
            {
                this.owner = owner;
            }

            /// <summary>
            /// Gets or sets the dependency with the specified index.
            /// </summary>
            /// <param name="index">Index of the dependency.</param>
            /// <returns>Dependency at the specified index.</returns>
            public UniversalPackageDependency this[int index]
            {
                get
                {
                    if (index < 0)
                        throw new IndexOutOfRangeException();

                    var inst = this.owner[Key];
                    if (inst == null)
                        throw new IndexOutOfRangeException();

                    if (inst is string s)
                        return index == 0 ? UniversalPackageDependency.Parse(s) : throw new FormatException();

                    if (inst is Array a)
                    {
                        if (index >= a.Length)
                            throw new IndexOutOfRangeException();

                        if (a.GetValue(index) is string v)
                            return UniversalPackageDependency.Parse(v);
                        else
                            throw new FormatException();
                    }

                    throw new FormatException();
                }
                set
                {
                    if (index < 0)
                        throw new IndexOutOfRangeException();
                    if (value == null)
                        throw new ArgumentNullException();

                    var inst = this.owner[Key];
                    if (inst == null)
                        throw new IndexOutOfRangeException();

                    if (inst is string s)
                    {
                        if (index != 0)
                            throw new IndexOutOfRangeException();

                        this.owner[Key] = value.ToString();
                        return;
                    }

                    if (inst is Array a)
                    {
                        if (index >= a.Length)
                            throw new IndexOutOfRangeException();

                        a.SetValue(value.ToString(), index);
                        return;
                    }

                    throw new InvalidOperationException();
                }
            }

            /// <summary>
            /// Gets the number of dependencies.
            /// </summary>
            public int Count
            {
                get
                {
                    var inst = this.owner[Key];
                    if (inst is string)
                        return 1;

                    if (inst is Array a)
                        return a.Length;

                    return 0;
                }
            }

            bool ICollection<UniversalPackageDependency>.IsReadOnly => false;

            /// <summary>
            /// Adds a dependency to the end of the list.
            /// </summary>
            /// <param name="item">Dependency to add.</param>
            public void Add(UniversalPackageDependency item)
            {
                var list = this.GetList();
                list.Add(item.ToString());
                this.SetList(list);
            }
            /// <summary>
            /// Returns a value indicating whether the specified dependency is contained in the list.
            /// </summary>
            /// <param name="item">Dependency to search for.</param>
            /// <returns>True if the dependency is in the list; otherwise false.</returns>
            public bool Contains(UniversalPackageDependency item) => this.IndexOf(item) >= 0;
            /// <summary>
            /// Returns the index of the specified dependency if it was found; otherwise returns -1.
            /// </summary>
            /// <param name="item">Dependency to search for.</param>
            /// <returns>Index of the dependency if it was found; otherwise -1.</returns>
            public int IndexOf(UniversalPackageDependency item)
            {
                int index = 0;
                foreach (var d in this)
                {
                    if (d.Equals(item))
                        return index;
                    index++;
                }

                return -1;
            }
            /// <summary>
            /// Removes the specified dependency from the list if it was found.
            /// </summary>
            /// <param name="item">Dependency to remove.</param>
            /// <returns>True of the dependency was removed; false if it was not found.</returns>
            public bool Remove(UniversalPackageDependency item)
            {
                var list = this.GetList();
                bool removed = list.Remove(item.ToString());
                this.SetList(list);
                return removed;
            }
            /// <summary>
            /// Returns an enumerator for all dependencies.
            /// </summary>
            /// <returns>Enumerator for all dependencies.</returns>
            public IEnumerator<UniversalPackageDependency> GetEnumerator()
            {
                var inst = this.owner[Key];
                if (inst == null)
                    yield break;

                if (inst is string s)
                {
                    yield return UniversalPackageDependency.Parse(s);
                    yield break;
                }

                if (inst is Array a)
                {
                    foreach (var item in a)
                    {
                        if (item is string d)
                            yield return UniversalPackageDependency.Parse(d);
                    }
                }
            }

            void ICollection<UniversalPackageDependency>.Clear() => this.owner.Remove(Key);
            void ICollection<UniversalPackageDependency>.CopyTo(UniversalPackageDependency[] array, int arrayIndex)
            {
                this.GetList().Select(UniversalPackageDependency.Parse).ToList().CopyTo(array, arrayIndex);
            }
            void IList<UniversalPackageDependency>.Insert(int index, UniversalPackageDependency item)
            {
                var list = this.GetList();
                list.Insert(index, item.ToString());
                this.SetList(list);
            }
            void IList<UniversalPackageDependency>.RemoveAt(int index)
            {
                var list = this.GetList();
                list.RemoveAt(index);
                this.SetList(list);
            }
            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

            private List<string> GetList()
            {
                var inst = this.owner[Key];
                if (inst == null)
                    return new List<string>();

                if (inst is string s)
                    return new List<string> { s };

                if (inst is Array a)
                {
                    var list = new List<string>(a.Length + 1);
                    foreach (var item in a)
                        list.Add((string)item);
                }

                return new List<string>();
            }
            private void SetList(List<string> list)
            {
                if ((list?.Count ?? 0) == 0)
                {
                    this.owner[Key] = null;
                    return;
                }

                if (list.Count == 1)
                    this.owner[Key] = list[0];

                this.owner[Key] = list.ToArray();
            }
        }
    }
}
