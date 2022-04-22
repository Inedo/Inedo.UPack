namespace Inedo.UPack.Packaging
{
    partial class UniversalPackageMetadata
    {
        /// <summary>
        /// Represents a parsed list of package dependencies.
        /// </summary>
        [Serializable]
        public sealed class DependencyList : WrappedList<UniversalPackageDependency>
        {
            internal DependencyList(UniversalPackageMetadata owner)
                : base(owner)
            {
            }

            private protected override string PropertyName => "dependencies";

            private protected override List<UniversalPackageDependency> GetList()
            {
                var inst = this.Owner[this.PropertyName];
                if (inst == null)
                    return new List<UniversalPackageDependency>();

                if (inst is string s)
                    return new List<UniversalPackageDependency> { UniversalPackageDependency.Parse(s) };

                if (inst is Array a)
                {
                    var list = new List<UniversalPackageDependency>(a.Length + 1);
                    foreach (var item in a)
                        list.Add(UniversalPackageDependency.Parse((string?)item));

                    return list;
                }

                return new List<UniversalPackageDependency>();
            }
            private protected override void SetList(List<UniversalPackageDependency> list)
            {
                if ((list?.Count ?? 0) == 0)
                {
                    this.Owner[this.PropertyName] = null;
                    return;
                }

                if (list!.Count == 1)
                    this.Owner[this.PropertyName] = list[0].ToString();

                this.Owner[this.PropertyName] = list.Select(d => d.ToString()).ToArray();
            }
        }
    }
}
