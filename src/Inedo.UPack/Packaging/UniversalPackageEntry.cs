using System;
using System.IO;
using System.IO.Compression;

namespace Inedo.UPack.Packaging
{
    /// <summary>
    /// Represents a file contained in a universal package.
    /// </summary>
    public struct UniversalPackageEntry
    {
        private readonly ZipArchiveEntry zipEntry;

        internal UniversalPackageEntry(ZipArchiveEntry zipEntry) => this.zipEntry = zipEntry;

        /// <summary>
        /// Gets the full path relative to the package archive root. This includes metadata files as well as package content.
        /// </summary>
        public string RawPath => this.zipEntry?.FullName.Replace('\\', '/')!;
        /// <summary>
        /// Gets the path relative to the package content root. This includes only content files and returns null for metadata files.
        /// </summary>
        public string? ContentPath
        {
            get
            {
                var fullName = this.RawPath;
                if (fullName?.StartsWith("package/", StringComparison.OrdinalIgnoreCase) == true && fullName.Length > "package/".Length)
                    return fullName.Substring("package/".Length);
                else
                    return null;
            }
        }
        /// <summary>
        /// Gets a value indicating whether this is a content item.
        /// </summary>
        public bool IsContent => this.ContentPath != null;
        /// <summary>
        /// Gets a value indicating whether this item represents a directory.
        /// </summary>
        public bool IsDirectory => this.RawPath?.EndsWith("/") == true;
        /// <summary>
        /// Gets the size of the item in bytes.
        /// </summary>
        public long Size => this.zipEntry?.Length ?? 0;
        /// <summary>
        /// Gets the timestamp of the item.
        /// </summary>
        public DateTimeOffset Timestamp => this.zipEntry?.LastWriteTime ?? default;

        /// <summary>
        /// Opens the item for sequential read access.
        /// </summary>
        /// <returns>Stream backed by the item.</returns>
        public Stream Open() => this.zipEntry.Open();
    }
}
