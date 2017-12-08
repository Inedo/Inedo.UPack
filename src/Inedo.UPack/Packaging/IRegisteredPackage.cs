using Newtonsoft.Json;

namespace Inedo.UPack.Packaging
{
    [JsonObject]
    internal interface IRegisteredPackage
    {
        [JsonProperty("group", NullValueHandling = NullValueHandling.Ignore)]
        string Group { get; set; }
        [JsonProperty("name")]
        string Name { get; set; }
        [JsonProperty("version")]
        string Version { get; set; }
        [JsonProperty("path")]
        string InstallPath { get; set; }
        [JsonProperty("feedUrl", NullValueHandling = NullValueHandling.Ignore)]
        string FeedUrl { get; set; }
        [JsonProperty("installationDate", NullValueHandling = NullValueHandling.Ignore)]
        string InstallationDate { get; set; }
        [JsonProperty("installationReason", NullValueHandling = NullValueHandling.Ignore)]
        string InstallationReason { get; set; }
        [JsonProperty("installedUsing", NullValueHandling = NullValueHandling.Ignore)]
        string InstalledUsing { get; set; }
        [JsonProperty("installedBy", NullValueHandling = NullValueHandling.Ignore)]
        string InstalledBy { get; set; }
    }
}
