using System.Text.Json;

namespace Inedo.UPack
{
    internal static class InternalExtensions
    {
        public static string? GetStringOrDefault(this JsonElement obj, string propertyName)
        {
            if (obj.TryGetProperty(propertyName, out var value))
                return value.ToString();
            else
                return null;
        }
        public static int? GetInt32OrDefault(this JsonElement obj, string propertyName)
        {
            if (obj.TryGetProperty(propertyName, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int intValue))
                    return intValue;
                if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out intValue))
                    return intValue;
            }

            return null;
        }
        public static long? GetInt64OrDefault(this JsonElement obj, string propertyName)
        {
            if (obj.TryGetProperty(propertyName, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out long longValue))
                    return longValue;
                if (value.ValueKind == JsonValueKind.String && long.TryParse(value.GetString(), out longValue))
                    return longValue;
            }

            return null;
        }
        public static DateTimeOffset? GetDateTimeOffsetOrDefault(this JsonElement obj, string propertyName)
        {
            if (obj.TryGetProperty(propertyName, out var value) && value.TryGetDateTimeOffset(out var dtoValue))
                return dtoValue;
            else
                return null;
        }
    }
}
