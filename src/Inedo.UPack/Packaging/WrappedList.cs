using System;
using System.Collections;
using System.Collections.Generic;

namespace Inedo.UPack.Packaging
{
    partial class UniversalPackageMetadata
    {
        /// <summary>
        /// Represents a collection of items stored as package properties.
        /// </summary>
        /// <typeparam name="T">Type of the item.</typeparam>
        public abstract class WrappedList<T> : IList<T>
        {
            private protected WrappedList(UniversalPackageMetadata owner) => this.Owner = owner;

            /// <summary>
            /// Gets or sets the item with the specified index.
            /// </summary>
            /// <param name="index">Index of the item.</param>
            /// <returns>Item at the specified index.</returns>
            public T this[int index]
            {
                get => this.GetList()[index];
                set
                {
                    var list = this.GetList();
                    list[index] = value;
                    this.SetList(list);
                }
            }

            /// <summary>
            /// Gets the number of items.
            /// </summary>
            public int Count
            {
                get
                {
                    var inst = this.Owner[this.PropertyName];

                    if (inst is Array a)
                        return a.Length;

                    if (inst is null)
                        return 0;

                    return 1;
                }
            }

            private protected UniversalPackageMetadata Owner { get; }
            private protected abstract string PropertyName { get; }

            bool ICollection<T>.IsReadOnly => false;

            /// <summary>
            /// Adds an item to the end of the list.
            /// </summary>
            /// <param name="item">Item to add.</param>
            public void Add(T item)
            {
                var list = this.GetList();
                list.Add(item);
                this.SetList(list);
            }
            /// <summary>
            /// Returns a value indicating whether the specified item is contained in the list.
            /// </summary>
            /// <param name="item">Item to search for.</param>
            /// <returns>True if the item is in the list; otherwise false.</returns>
            public bool Contains(T item) => this.IndexOf(item) >= 0;
            /// <summary>
            /// Returns the index of the specified item if it was found; otherwise returns -1.
            /// </summary>
            /// <param name="item">Item to search for.</param>
            /// <returns>Index of the item if it was found; otherwise -1.</returns>
            public int IndexOf(T item)
            {
                int index = 0;
                foreach (var d in this)
                {
                    if (d!.Equals(item))
                        return index;
                    index++;
                }

                return -1;
            }
            /// <summary>
            /// Removes the specified item from the list if it was found.
            /// </summary>
            /// <param name="item">Item to remove.</param>
            /// <returns>True if the item was removed; false if it was not found.</returns>
            public bool Remove(T item)
            {
                var list = this.GetList();
                bool removed = list.Remove(item);
                this.SetList(list);
                return removed;
            }
            /// <summary>
            /// Returns an enumerator for all items.
            /// </summary>
            /// <returns>Enumerator for all items.</returns>
            public IEnumerator<T> GetEnumerator() => this.GetList().GetEnumerator();

            void ICollection<T>.Clear() => this.Owner.Remove(this.PropertyName);
            void ICollection<T>.CopyTo(T[] array, int arrayIndex) => this.GetList().CopyTo(array, arrayIndex);
            void IList<T>.Insert(int index, T item)
            {
                var list = this.GetList();
                list.Insert(index, item);
                this.SetList(list);
            }
            void IList<T>.RemoveAt(int index)
            {
                var list = this.GetList();
                list.RemoveAt(index);
                this.SetList(list);
            }
            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

            private protected abstract List<T> GetList();
            private protected abstract void SetList(List<T> list);
        }
    }
}
