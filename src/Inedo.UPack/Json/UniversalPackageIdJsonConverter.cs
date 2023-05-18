using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inedo.UPack.Json
{
    public class UniversalPackageIdJsonConverter : JsonConverter<UniversalPackageId>
    {
        public override UniversalPackageId? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return value == null
                ? default
                : UniversalPackageId.Parse(value);
        }

        public override void Write(
            Utf8JsonWriter writer,
            UniversalPackageId value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
