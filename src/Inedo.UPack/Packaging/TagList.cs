using System;
using System.Collections.Generic;
using System.Linq;

namespace Inedo.UPack.Packaging
{
    partial class UniversalPackageMetadata
    {
        /// <summary>
        /// Represents a list of package tags.
        /// </summary>
        [Serializable]
        public sealed class TagList : WrappedList<string>
        {
            internal TagList(UniversalPackageMetadata owner)
                : base(owner)
            {
            }

            private protected override string PropertyName => "tags";

            private protected override List<string> GetList()
            {
                var inst = this.Owner[this.PropertyName];

                if (inst is string s)
                    return new List<string>(1) { s };

                if (inst is Array a)
                    return new List<string>(a.Cast<object>().Select(i => i?.ToString()));

                return new List<string>();
            }
            private protected override void SetList(List<string> list)
            {
                if ((list?.Count ?? 0) == 0)
                {
                    this.Owner.Remove(this.PropertyName);
                    return;
                }

                this.Owner[this.PropertyName] = list.ToArray();
            }
        }
    }
}
