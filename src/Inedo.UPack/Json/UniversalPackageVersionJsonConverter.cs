using System.Text.Json.Serialization;
using System.Text.Json;

namespace Inedo.UPack.Json
{
    public class UniversalPackageVersionJsonConverter : JsonConverter<UniversalPackageVersion>
    {
        public override UniversalPackageVersion? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return UniversalPackageVersion.Parse(reader.GetString());
        }

        public override void Write(
            Utf8JsonWriter writer,
            UniversalPackageVersion value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
