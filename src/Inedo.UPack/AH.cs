using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Inedo.UPack
{
    internal static class AH
    {
        public static string FormatName(string group, string name) => string.IsNullOrEmpty(group) ? name : (group + "/" + name);
        public static string NullIf(string a, string b) => a != b ? a : null;
        public static Task CompletedTask
        {
            get
            {
#if NET45
                return Task.FromResult<object>(null);
#else
                return Task.CompletedTask;
#endif
            }
        }
        public static Encoding UTF8 => new UTF8Encoding(false);

        /// <summary>
        /// Returns a hash code for arbitrary binary data.
        /// </summary>
        /// <param name="array">The array containing the data to hash.</param>
        /// <returns>The hash code.</returns>
        public static int GetHashCode(byte[] array)
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
        public static bool Equals(byte[] array1, byte[] array2)
        {
            if (ReferenceEquals(array1, array2))
                return true;
            if (ReferenceEquals(array1, null) | ReferenceEquals(array2, null))
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

        public static object CanonicalizeJsonToken(JToken token)
        {
            if (token is JValue v)
                return v.ToString();

            if (token is JObject o)
            {
                return o.Properties()
                    .ToDictionary(p => p.Name, p => CanonicalizeJsonToken(p.Value));
            }

            if (token is JArray a)
                return a.Select(CanonicalizeJsonToken).ToArray();

            return null;
        }
    }
}
