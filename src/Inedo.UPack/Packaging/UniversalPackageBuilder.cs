using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

        /// <summary>
        /// Adds the files and directories from the specified source path to the specified target path in the package.
        /// </summary>
        /// <param name="sourcePath">Full source path of files and directories to include. This must be an absolute path.</param>
        /// <param name="targetPath">Target prefix path inside the package.</param>
        /// <param name="recursive">When true, subdirectories will be recursively added to the package.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sourcePath"/> is null or empty.</exception>
        /// <exception cref="ArgumentException"><paramref name="sourcePath"/> is not an absolute path.</exception>
        public Task AddContentsAsync(string sourcePath, string targetPath, bool recursive) => this.AddContentsAsync(sourcePath, targetPath, recursive, null);
        /// <summary>
        /// Adds the files and directories from the specified source path to the specified target path in the package.
        /// </summary>
        /// <param name="sourcePath">Full source path of files and directories to include. This must be an absolute path.</param>
        /// <param name="targetPath">Target prefix path inside the package.</param>
        /// <param name="recursive">When true, subdirectories will be recursively added to the package.</param>
        /// <param name="shouldInclude">Method invoked for each file to determine if it should be added to the package. The full source path is the argument supplied to the method.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sourcePath"/> is null or empty.</exception>
        /// <exception cref="ArgumentException"><paramref name="sourcePath"/> is not an absolute path.</exception>
        public Task AddContentsAsync(string sourcePath, string targetPath, bool recursive, Predicate<string> shouldInclude) => this.AddContentsAsync(sourcePath, targetPath, recursive, shouldInclude, new CancellationToken());
        /// <summary>
        /// Adds the files and directories from the specified source path to the specified target path in the package.
        /// </summary>
        /// <param name="sourcePath">Full source path of files and directories to include. This must be an absolute path.</param>
        /// <param name="targetPath">Target prefix path inside the package.</param>
        /// <param name="recursive">When true, subdirectories will be recursively added to the package.</param>
        /// <param name="shouldInclude">Method invoked for each file to determine if it should be added to the package. The full source path is the argument supplied to the method.</param>
        /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sourcePath"/> is null or empty.</exception>
        /// <exception cref="ArgumentException"><paramref name="sourcePath"/> is not an absolute path.</exception>
        public async Task AddContentsAsync(string sourcePath, string targetPath, bool recursive, Predicate<string> shouldInclude, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentNullException(nameof(sourcePath));
            if (!Path.IsPathRooted(sourcePath))
                throw new ArgumentException("Source path must be an absolute path.");

            var root = targetPath?.Trim('/', '\\')?.Replace('\\', '/') ?? string.Empty;

            // keep track of directories implicitly added
            var addedDirs = new HashSet<string>();

            // first add all of the files
            foreach (var sourceFileName in Directory.EnumerateFiles(sourcePath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                if (shouldInclude?.Invoke(sourceFileName) == false)
                    continue;

                cancellationToken.ThrowIfCancellationRequested();

                using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    var itemPath = getFullTargetPath(sourceFileName);
                    var pathParts = itemPath.Split('/');
                    for (int i = 1; i < pathParts.Length - 1; i++)
                        addedDirs.Add(string.Join("/", pathParts.Take(i)));

                    await this.AddFileAsync(sourceStream, itemPath, File.GetLastWriteTimeUtc(sourceFileName), cancellationToken).ConfigureAwait(false);
                }
            }

            // don't ever add any subdirectories if not in recursive mode
            if (recursive)
            {
                // now look for any empty directories
                foreach (var sourceDirName in Directory.EnumerateDirectories(sourcePath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    var itemPath = getFullTargetPath(sourceDirName);
                    if (addedDirs.Add(itemPath))
                        this.AddEmptyDirectory(itemPath);
                }
            }

            string getFullTargetPath(string fullSourcePath)
            {
                var path = fullSourcePath.Substring(sourcePath.Length).Trim('/', '\\').Replace('\\', '/');
                return string.IsNullOrEmpty(targetPath) ? path : (root + "/" + path);
            }
        }

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
