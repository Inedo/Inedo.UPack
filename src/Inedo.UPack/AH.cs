using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
    }
}
