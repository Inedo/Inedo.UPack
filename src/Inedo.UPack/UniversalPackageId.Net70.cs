#if NET7_0_OR_GREATER

using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Inedo.UPack;

public sealed partial class UniversalPackageId : IEqualityOperators<UniversalPackageId, UniversalPackageId, bool>, IParsable<UniversalPackageId>
{
    static UniversalPackageId IParsable<UniversalPackageId>.Parse(string s, IFormatProvider? provider) => Parse(s);
    static bool IParsable<UniversalPackageId>.TryParse(string? s, IFormatProvider? provider, [NotNullWhen(true)] out UniversalPackageId? result) => TryParse(s, out result);
}
#endif
