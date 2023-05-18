using Inedo.UPack.Packaging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inedo.UPack.Json
{
    public class UniversalPackageMetadataJsonConverter : JsonConverter<UniversalPackageMetadata>
    {
        public override UniversalPackageMetadata Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var element = JsonElement.ParseValue(ref reader);
            return new UniversalPackageMetadata(element);
        }

        public override void Write(
            Utf8JsonWriter writer,
            UniversalPackageMetadata value,
            JsonSerializerOptions options)
        {
            value.WriteJson(writer);
        }
    }
}
