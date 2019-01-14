using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inedo.UPack.Packaging
{
    /// <summary>
    /// Represents a read-only universal package.
    /// </summary>
    public sealed class UniversalPackage : IDisposable
    {
        private ZipArchive zip;
        private UniversalPackageMetadata metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackage"/> class.
        /// </summary>
        /// <param name="stream">Stream backed by the universal package. If this stream does not support seeking, a copy will be made.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="InvalidDataException">The stream does not contain a valid universal package.</exception>
        public UniversalPackage(Stream stream)
            : this(stream, false)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackage"/> class.
        /// </summary>
        /// <param name="stream">Stream backed by the universal package. If this stream does not support seeking, a copy will be made.</param>
        /// <param name="leaveOpen">Value indicating whether to leave the underlying stream open when the instance is disposed. The default is false.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="InvalidDataException">The stream does not contain a valid universal package.</exception>
        public UniversalPackage(Stream stream, bool leaveOpen)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanSeek)
            {
                var tempStream = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
                stream.CopyTo(tempStream);
                if (!leaveOpen)
                    stream.Dispose();

                tempStream.Position = 0;
                this.zip = new ZipArchive(tempStream, ZipArchiveMode.Read, false);
            }
            else
            {
                this.zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen);
            }

            this.Entries = new EntryCollection(this);
            this.metadata = this.ReadMetadata();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackage"/> class.
        /// </summary>
        /// <param name="fileName">Full path of the .upack file to open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is null or empty.</exception>
        public UniversalPackage(string fileName)
            : this(OpenFile(fileName))
        {
        }

        /// <summary>
        /// Gets the group of the package.
        /// </summary>
        public string Group => this.metadata.Group;
        /// <summary>
        /// Gets the name of the package.
        /// </summary>
        public string Name => this.metadata.Name;
        /// <summary>
        /// Gets the version of the package.
        /// </summary>
        public UniversalPackageVersion Version => this.metadata.Version;
        /// <summary>
        /// Gets the entries contained in the package.
        /// </summary>
        public EntryCollection Entries { get; }

        /// <summary>
        /// Gets a copy of the full metadata (upack.json) for the package.
        /// </summary>
        /// <returns>Full package metadata.</returns>
        public UniversalPackageMetadata GetFullMetadata() => this.metadata.Clone();
        /// <summary>
        /// Returns an entry by its raw path (relative to the archive root), or null if the item was not found.
        /// </summary>
        /// <param name="rawPath">The full path of the item relative to the archive root.</param>
        /// <returns>Entry with the specified path if it was found; otherwise null.</returns>
        public UniversalPackageEntry? GetRawEntry(string rawPath)
        {
            if (string.IsNullOrEmpty(rawPath))
                return null;

            rawPath = rawPath.Replace('\\', '/').TrimStart('/');
            if (string.IsNullOrEmpty(rawPath))
                return null;

            var zipEntry = this.zip.GetEntry(rawPath);
            if (zipEntry != null)
                return new UniversalPackageEntry(zipEntry);

            // if not exact match, iterate through everything
            List<UniversalPackageEntry> maybeMatches = null;
            foreach (var entry in this.Entries)
            {
                // if casing matches exactly, return this one
                if (entry.RawPath == rawPath)
                {
                    return entry;
                }
                else if (string.Equals(entry.RawPath, rawPath, StringComparison.OrdinalIgnoreCase))
                {
                    if (maybeMatches == null)
                        maybeMatches = new List<UniversalPackageEntry>();
                    maybeMatches.Add(entry);
                }
            }

            if (maybeMatches?.Count > 0)
            {
                // use lexical sort for remaining matches
                maybeMatches.Sort((a, b) => a.RawPath.CompareTo(b.RawPath));
                return maybeMatches[0];
            }

            return null;
        }
        /// <summary>
        /// Returns an entry by its content path (relative to the package root), or null if the item was not found.
        /// </summary>
        /// <param name="contentPath">The full path of the item relative to the content root.</param>
        /// <returns>Entry with the specified path if it was found; otherwise null.</returns>
        public UniversalPackageEntry? GetContentEntry(string contentPath)
        {
            if (string.IsNullOrEmpty(contentPath))
                return null;

            return this.GetRawEntry("package/" + contentPath.TrimStart('/', '\\'));
        }
        /// <summary>
        /// Releases resources used by this instance.
        /// </summary>
        public void Dispose() => this.zip.Dispose();
        /// <summary>
        /// Extracts all items in the package to the specified target path.
        /// </summary>
        /// <param name="targetPath">Root path to extract items to.</param>
        /// <param name="cancellationToken">Cancellation token/</param>
        /// <exception cref="ArgumentNullException"><paramref name="targetPath"/> is null or empty.</exception>
        public Task ExtractAllItemsAsync(string targetPath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(targetPath))
                throw new ArgumentNullException(nameof(targetPath));

            return ExtractItemsInternalAsync(
                this.Entries,
                e => e.RawPath,
                targetPath,
                cancellationToken
            );
        }
        /// <summary>
        /// Extracts all items in the package to the specified target path.
        /// </summary>
        /// <param name="targetPath">Root path to extract items to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="targetPath"/> is null or empty.</exception>
        public Task ExtractAllItemsAsync(string targetPath) => this.ExtractAllItemsAsync(targetPath, default);
        /// <summary>
        /// Extracts the content items in the package to the specified target path.
        /// </summary>
        /// <param name="targetPath">Root path to extract items to.</param>
        /// <param name="cancellationToken">Cancellation token/</param>
        /// <exception cref="ArgumentNullException"><paramref name="targetPath"/> is null or empty.</exception>
        public Task ExtractContentItemsAsync(string targetPath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(targetPath))
                throw new ArgumentNullException(nameof(targetPath));

            return ExtractItemsInternalAsync(
                this.Entries.Where(e => e.IsContent),
                e => e.ContentPath,
                targetPath,
                cancellationToken
            );
        }
        /// <summary>
        /// Extracts the content items in the package to the specified target path.
        /// </summary>
        /// <param name="targetPath">Root path to extract items to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="targetPath"/> is null or empty.</exception>
        public Task ExtractContentItemsAsync(string targetPath) => this.ExtractContentItemsAsync(targetPath, default);

        private UniversalPackageMetadata ReadMetadata()
        {
            var entry = this.zip.GetEntry("upack.json");
            if (entry == null)
                throw new InvalidDataException("upack.json not found in package.");

            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            using (var json = new JsonTextReader(reader))
            {
                var obj = JObject.Load(json);
                return new UniversalPackageMetadata(obj);
            }
        }

        private static async Task ExtractItemsInternalAsync(IEnumerable<UniversalPackageEntry> entries, Func<UniversalPackageEntry, string> getRelativePath, string targetPath, CancellationToken cancellationToken)
        {
            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var relativePath = getRelativePath(entry);
                var destPath = Path.Combine(targetPath.Replace('/', Path.DirectorySeparatorChar), relativePath.Replace('/', Path.DirectorySeparatorChar));

                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(destPath);
                }
                else
                {
                    var containingPath = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(containingPath))
                        Directory.CreateDirectory(containingPath);

                    using (var entryStream = entry.Open())
                    using (var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                    {
                        await entryStream.CopyToAsync(destStream, 81920, cancellationToken).ConfigureAwait(false);
                    }

                    File.SetLastWriteTimeUtc(destPath, entry.Timestamp.UtcDateTime);
                }
            }
        }
        private static Stream OpenFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            return new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <summary>
        /// Represents a collection of entries in a universal package.
        /// </summary>
        public sealed class EntryCollection : IReadOnlyCollection<UniversalPackageEntry>
        {
            private readonly UniversalPackage owner;

            internal EntryCollection(UniversalPackage owner) => this.owner = owner;

            /// <summary>
            /// Gets the number of entries in the package.
            /// </summary>
            public int Count => this.owner.zip.Entries.Count;

            /// <summary>
            /// Gets an enumerator for the package entries.
            /// </summary>
            /// <returns>Package entry enumerator.</returns>
            public IEnumerator<UniversalPackageEntry> GetEnumerator() => this.owner.zip.Entries.Select(e => new UniversalPackageEntry(e)).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }
    }
}
