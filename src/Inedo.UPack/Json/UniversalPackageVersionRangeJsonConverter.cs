using System.Text.Json.Serialization;
using System.Text.Json;

namespace Inedo.UPack.Json
{
    public class UniversalPackageVersionRangeJsonConverter : JsonConverter<UniversalPackageVersionRange>
    {
        public override UniversalPackageVersionRange Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return value == null
                ? default
                : UniversalPackageVersionRange.Parse(value);
        }

        public override void Write(
            Utf8JsonWriter writer,
            UniversalPackageVersionRange value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
