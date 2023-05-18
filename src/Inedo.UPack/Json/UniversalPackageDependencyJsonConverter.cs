using System.Text.Json.Serialization;
using System.Text.Json;

namespace Inedo.UPack.Json
{
    public class UniversalPackageDependencyJsonConverter : JsonConverter<UniversalPackageDependency>
    {
        public override UniversalPackageDependency Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return UniversalPackageDependency.Parse(reader.GetString());
        }

        public override void Write(
            Utf8JsonWriter writer,
            UniversalPackageDependency value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
