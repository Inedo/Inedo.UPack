using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Inedo.UPack.Packaging
{
    /// <summary>
    /// Provides write-only access to a package to allow for universal package creation.
    /// </summary>
    public sealed class UniversalPackageBuilder : IDisposable
    {
        private ZipArchive zip;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageBuilder"/> class.
        /// </summary>
        /// <param name="stream">Stream to write package to.</param>
        /// <param name="metadata">Metadata to write as the upack.json file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null or <paramref name="metadata"/> is null.</exception>
        public UniversalPackageBuilder(Stream stream, UniversalPackageMetadata metadata)
            : this(stream, metadata, false)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageBuilder"/> class.
        /// </summary>
        /// <param name="stream">Stream to write package to.</param>
        /// <param name="metadata">Metadata to write as the upack.json file.</param>
        /// <param name="leaveOpen">Value indicating whether to leave the underlying stream open when the instance is disposed. The default is false.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null or <paramref name="metadata"/> is null.</exception>
        public UniversalPackageBuilder(Stream stream, UniversalPackageMetadata metadata, bool leaveOpen)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            try
            {
                this.zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen);
                this.WriteMetadata(metadata);
            }
            catch
            {
                this.zip?.Dispose();
                throw;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalPackageBuilder"/> class.
        /// </summary>
        /// <param name="fileName">Full path of the .upack file to create.</param>
        /// <param name="metadata">Metadata to write as the upack.json file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is null or empty or <paramref name="metadata"/> is null.</exception>
        public UniversalPackageBuilder(string fileName, UniversalPackageMetadata metadata)
            : this(CreateFile(fileName), metadata)
        {
        }

        /// <summary>
        /// Copies the data in the specified stream to the package using the specified raw path (relative to archive root) and timestamp.
        /// </summary>
        /// <param name="stream">Source stream to copy from.</param>
        /// <param name="path">Raw path of entry to create in the package (relative to archive root).</param>
        /// <param name="timestamp">Timestamp to record for the entry.</param>
        /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null or <paramref name="path"/> is null or empty.</exception>
        public async Task AddFileRawAsync(Stream stream, string path, DateTimeOffset timestamp, CancellationToken cancellationToken)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            var p = path.Replace('\\', '/').Trim('/');
            var entry = this.zip.CreateEntry(p);
            entry.LastWriteTime = timestamp;
            using (var entryStream = entry.Open())
            {
                await stream.CopyToAsync(entryStream, 81920, cancellationToken).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// Copies the data in the specified stream to the package using the specified content path (relative to package directory) and timestamp.
        /// </summary>
        /// <param name="stream">Source stream to copy from.</param>
        /// <param name="path">Path of entry to create in the package (relative to package directory).</param>
        /// <param name="timestamp">Timestamp to record for the entry.</param>
        /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null or <paramref name="path"/> is null or empty.</exception>
        public Task AddFileAsync(Stream stream, string path, DateTimeOffset timestamp, CancellationToken cancellationToken)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            return this.AddFileRawAsync(stream, "package/" + path.Trim('/', '\\'), timestamp, cancellationToken);
        }
        /// <summary>
        /// Creates an entry for an empty directory in the package using the specified raw path (relative to archive root).
        /// </summary>
        /// <param name="path">Path of the empty directory in the package (relative to archive root).</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null or empty.</exception>
        public void AddEmptyDirectoryRaw(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            var p = path.Replace('\\', '/').Trim('/') + "/";
            this.zip.CreateEntry(p);
        }
        /// <summary>
        /// Creates an entry for an empty directory in the package using the specified content path (relative to package directory).
        /// </summary>
        /// <param name="path">Path of the empty directory in the package (relative to package directory).</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null or empty.</exception>
        public void AddEmptyDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            var p = path.Replace('\\', '/').Trim('/') + "/";
            this.zip.CreateEntry("package/" + p);
        }
        /// <summary>
        /// Completes the package file and releases resources.
        /// </summary>
        public void Dispose() => this.zip.Dispose();

        private static Stream CreateFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            return new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        }
        private void WriteMetadata(UniversalPackageMetadata metadata)
        {
            var entry = this.zip.CreateEntry("upack.json");
            using (var entryStream = entry.Open())
            using (var writer = new StreamWriter(entryStream, new UTF8Encoding(false)))
            using (var jsonWriter = new JsonTextWriter(writer) { Formatting = Formatting.Indented })
            {
                metadata.WriteJson(jsonWriter);
            }
        }
    }
}
