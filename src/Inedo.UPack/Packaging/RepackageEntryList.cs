using System;
using System.Collections.Generic;
using System.Linq;

namespace Inedo.UPack.Packaging
{
    partial class UniversalPackageMetadata
    {
        /// <summary>
        /// Represents a parsed list of repackaging events.
        /// </summary>
        [Serializable]
        public sealed class RepackageEntryList : WrappedList<RepackageHistoryEntry>
        {
            internal RepackageEntryList(UniversalPackageMetadata owner)
                : base(owner)
            {
            }

            private protected override string PropertyName => "repackageHistory";

            private protected override List<RepackageHistoryEntry> GetList()
            {
                var inst = this.Owner[this.PropertyName];

                if (inst is Array a)
                {
                    var list = new List<RepackageHistoryEntry>(a.Length + 1);
                    foreach (var item in a)
                    {
                        if (item is string s)
                        {
                            list.Add(new RepackageHistoryEntry { Id = s });
                        }
                        else if (item is Dictionary<string, object> d)
                        {
                            list.Add(
                                new RepackageHistoryEntry
                                {
                                    Id = getString("id"),
                                    Date = getDate("date"),
                                    Reason = getString("reason"),
                                    Using = getString("using"),
                                    By = getString("by"),
                                    Url = getString("url")
                                }
                            );

                            string getString(string key)
                            {
                                if (d.TryGetValue(key, out var o) && o is string s2)
                                    return s2;
                                else
                                    return null;
                            }

                            DateTimeOffset? getDate(string key)
                            {
                                if (DateTimeOffset.TryParse(getString(key), out var date))
                                    return date;
                                else
                                    return null;
                            }
                        }
                    }

                    return list;
                }

                return new List<RepackageHistoryEntry>();
            }
            private protected override void SetList(List<RepackageHistoryEntry> list)
            {
                if ((list?.Count ?? 0) == 0)
                {
                    this.Owner.Remove(this.PropertyName);
                    return;
                }

                this.Owner[this.PropertyName] = list.Select(getItem).ToArray();

                object getItem(RepackageHistoryEntry entry)
                {
                    var dict = new Dictionary<string, object>();
                    if (!string.IsNullOrEmpty(entry.Id))
                        dict["id"] = entry.Id;
                    if (entry.Date != null)
                        dict["date"] = entry.Date?.ToString("o");
                    if (!string.IsNullOrEmpty(entry.Reason))
                        dict["reason"] = entry.Reason;
                    if (!string.IsNullOrEmpty(entry.Using))
                        dict["using"] = entry.Using;
                    if (!string.IsNullOrEmpty(entry.By))
                        dict["by"] = entry.By;
                    if (!string.IsNullOrEmpty(entry.Url))
                        dict["url"] = entry.Url;

                    if (dict.Count == 1 && dict.ContainsKey("id"))
                        return dict["id"];

                    return dict;
                }
            }
        }
    }
}
