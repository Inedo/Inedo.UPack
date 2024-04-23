#if NET6_0_OR_GREATER

using System.Diagnostics.CodeAnalysis;

namespace Inedo.UPack;

public sealed partial class UniversalPackageId : ISpanFormattable
{
    public static bool TryParse(string? s, [NotNullWhen(true)] out UniversalPackageId? id)
    {
        id = null;

        if (string.IsNullOrEmpty(s))
            return false;

        int i = s.IndexOf(':');
        if (i >= 0)
        {
            id = new UniversalPackageId(s[..i], s[(i + 1)..]);
        }
        else
        {
            i = s.LastIndexOf('/');
            if (i < 0)
                id = new UniversalPackageId(s);
            else
                id = new UniversalPackageId(s[..i], s[(i + 1)..]);
        }

        return true;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (this.Group == null)
        {
            if (this.Name.Length > destination.Length)
            {
                charsWritten = 0;
                return false;
            }

            this.Name.CopyTo(destination);
            charsWritten = this.Name.Length;
        }
        else
        {
            if (this.Group.Length + this.Name.Length + 1 > destination.Length)
            {
                charsWritten = 0;
                return false;
            }

            this.Group.CopyTo(destination);
            destination[this.Group.Length + 1] = '/';
            this.Name.CopyTo(destination[(this.Group.Length + 1)..]);
            charsWritten = this.Group.Length + this.Name.Length + 1;
        }

        return true;
    }

    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => this.ToString();
}
#endif
