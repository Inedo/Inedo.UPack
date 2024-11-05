using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Inedo.UPack
{
    internal static class AH
    {
        public static string FormatName(string? group, string name) => string.IsNullOrEmpty(group) ? name : (group + "/" + name);
        public static string? NullIf(string? a, string? b) => a != b ? a : null;
        public static Encoding UTF8 => new UTF8Encoding(false);

        /// <summary>
        /// Returns a hash code for arbitrary binary data.
        /// </summary>
        /// <param name="array">The array containing the data to hash.</param>
        /// <returns>The hash code.</returns>
        public static int GetHashCode(byte[]? array)
        {
            if (array == null || array.Length == 0)
                return 0;

            int length = array.Length;

            unsafe
            {
                unchecked
                {
                    fixed (byte* ptr = array)
                    {
                        byte* p = ptr;
                        uint h = 2166136261u;
                        int i;

                        for (i = 0; i < length; i++)
                            h = (h * 16777619u) ^ p[i];

                        return (int)h;
                    }
                }
            }
        }
        /// <summary>
        /// Returns a value indicating whether two byte arrays contain the same data.
        /// </summary>
        /// <param name="array1">The first array.</param>
        /// <param name="array2">The second array.</param>
        /// <returns>True if arrays contain the same data; otherwise false.</returns>
        public static bool Equals(byte[]? array1, byte[]? array2)
        {
            if (ReferenceEquals(array1, array2))
                return true;
            if (array1 is null || array2 is null)
                return false;
            if (array1.Length != array2.Length)
                return false;

            int length = array1.Length;

            unsafe
            {
                fixed (byte* ptr1 = array1)
                fixed (byte* ptr2 = array2)
                {
                    for (int i = 0; i < length; i++)
                    {
                        if (ptr1[i] != ptr2[i])
                            return false;
                    }
                }
            }

            return true;
        }

        public static object? CanonicalizeJsonToken(JsonElement token)
        {
            switch (token.ValueKind)
            {
                case JsonValueKind.String:
                    return token.GetString();

                case JsonValueKind.True:
                    return true;

                case JsonValueKind.False:
                    return false;

                case JsonValueKind.Number:
                    if (token.TryGetInt32(out int intValue))
                        return intValue;
                    else if (token.TryGetInt64(out long longValue))
                        return longValue;
                    else if (token.TryGetDouble(out double doubleValue))
                        return doubleValue;
                    else
                        return token.ToString();

                case JsonValueKind.Array:
                    {
                        var arr = new object?[token.GetArrayLength()];
                        int i = 0;
                        foreach (var v in token.EnumerateArray())
                            arr[i++] = CanonicalizeJsonToken(v);
                        return arr;
                    }

                case JsonValueKind.Object:
                    {
                        var dict = new Dictionary<string, object?>();
                        foreach (var p in token.EnumerateObject())
                            dict[p.Name] = CanonicalizeJsonToken(p.Value);
                        return dict;
                    }

                default:
                    return null;
            }
        }
        public static object? CanonicalizeJsonToken(JsonNode? token)
        {
            if (token is JsonValue value)
                return value.ToString();

            if (token is JsonObject obj)
            {
                var dict = new Dictionary<string, object?>();
                foreach (var p in obj)
                    dict[p.Key] = CanonicalizeJsonToken(p.Value);

                return dict;
            }

            if (token is JsonArray array)
            {
                var arr = new List<object?>();
                foreach (var v in array)
                    arr.Add(CanonicalizeJsonToken(v));

                return arr.ToArray();
            }

            return null;
        }

        public static void WriteObject(Utf8JsonWriter writer, Dictionary<string, object?> properties)
        {
            writer.WriteStartObject();

            foreach (var p in properties)
            {
                writer.WritePropertyName(p.Key);
                writeValue(writer, p.Value);
            }

            writer.WriteEndObject();

            static void writeValue(Utf8JsonWriter writer, object? value)
            {
                switch (value)
                {
                    case JsonElement el:
                        writer.WriteRawValue(el.GetRawText());
                        break;
                    case bool b:
                        writer.WriteBooleanValue(b);
                        break;

                    case int i:
                        writer.WriteNumberValue(i);
                        break;

                    case long l:
                        writer.WriteNumberValue(l);
                        break;

                    case string s:
                        writer.WriteStringValue(s);
                        break;

                    case object?[] a:
                        writer.WriteStartArray();
                        foreach (var v in a)
                            writeValue(writer, v);
                        writer.WriteEndArray();
                        break;

                    case Dictionary<string, object?> o:
                        WriteObject(writer, o);
                        break;

                    case null:
                        writer.WriteNullValue();
                        break;
                }
            }
        }
    }
}
