using System.Text;

namespace Inedo.UPack
{
    /// <summary>
    /// Represents a binary string that is formatted as a hexadecimal number.
    /// </summary>
    [Serializable]
    public readonly struct HexString : IEquatable<HexString>
    {
        private readonly byte[] bytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="HexString"/> struct.
        /// </summary>
        /// <param name="bytes">Bytes to display in hex.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null or has a length of zero.</exception>
        public HexString(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                throw new ArgumentNullException(nameof(bytes));

            this.bytes = bytes;
        }

        public static bool operator ==(HexString s1, HexString s2) => Equals(s1, s2);
        public static bool operator !=(HexString s1, HexString s2) => !Equals(s1, s2);

        /// <summary>
        /// Converts the specified string to a <see cref="HexString"/> instance.
        /// </summary>
        /// <param name="s">String containing an even number of hexadecimal characters.</param>
        /// <returns><see cref="HexString"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is null or empty.</exception>
        /// <exception cref="ArgumentException"><paramref name="s"/> does not contain an even number of characters.</exception>
        /// <exception cref="FormatException"><paramref name="s"/> contains invalid characters.</exception>
        public static HexString Parse(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException(s);

            if ((s.Length % 2) != 0)
                throw new ArgumentException("String is not an even number of characters.");

            var bytes = new byte[s.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)((GetNibble(s[i * 2]) << 4) | GetNibble(s[(i * 2) + 1]));

            return new HexString(bytes);
        }

        public static bool Equals(HexString s1, HexString s2) => AH.Equals(s1.bytes, s2.bytes);
        public bool Equals(HexString other) => Equals(this, other);
        public override bool Equals(object? obj) => obj is HexString h && this.Equals(h);
        public override int GetHashCode() => AH.GetHashCode(this.bytes);
        /// <summary>
        /// Returns a hexadecimal string representation of the binary value.
        /// </summary>
        /// <returns>Hexadecimal string.</returns>
        public override string ToString()
        {
            if (this.bytes == null)
                return string.Empty;

            var buffer = new StringBuilder(this.bytes.Length * 2);
            foreach (byte b in this.bytes)
            {
                buffer.Append(GetChar((b >> 4) & 0xF));
                buffer.Append(GetChar(b & 0xF));
            }

            return buffer.ToString();
        }
        /// <summary>
        /// Copies the bytes of the <see cref="HexString"/> to a new byte array and returns it.
        /// </summary>
        /// <returns>Byte array containing raw bytes.</returns>
        public byte[] ToByteArray()
        {
            var data = new byte[this.bytes.Length];
            Buffer.BlockCopy(this.bytes, 0, data, 0, this.bytes.Length);
            return data;
        }

        private static char GetChar(int n) => n < 0xA ? (char)('0' + n) : (char)('a' + n - 0xA);
        private static int GetNibble(char c)
        {
            if (c >= '0' && c <= '9')
                return c - '0';
            else if (c >= 'a' && c <= 'f')
                return c - 'a' + 0xA;
            else if (c >= 'A' && c <= 'F')
                return c - 'A' + 0xA;
            else
                throw new FormatException("Invalid character in hex string.");
        }
    }
}
