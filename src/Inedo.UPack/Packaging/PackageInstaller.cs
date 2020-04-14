using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Inedo.UPack.Net;

namespace Inedo.UPack.Packaging
{
    /// <summary>
    /// Contains utility methods for common package installation tasks.
    /// </summary>
    public static class PackageInstaller
    {
        public static Task InstallFromFileAsync(Stream packageStream, PackageInstallOptions options, CancellationToken cancellationToken = default)
        {
            if (packageStream == null)
                throw new ArgumentNullException(nameof(packageStream));
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrEmpty(options.TargetPath))
                throw new ArgumentNullException(nameof(options.TargetPath));

            return InstallInternalAsync(packageStream, options, null, options.AddToCache, cancellationToken);
        }
        public static async Task InstallFromFileAsync(string packageFileName, PackageInstallOptions options, CancellationToken cancellationToken = default)
        {
            using var stream = new FileStream(packageFileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
            await InstallFromFileAsync(stream, options, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<bool> InstallFromCacheAsync(UniversalPackageId id, UniversalPackageVersion version, PackageInstallOptions options, CancellationToken cancellationToken = default)
        {
            using var registry = PackageRegistry.GetRegistry(options.UserRegistry);
            await registry.LockAsync(cancellationToken).ConfigureAwait(false);
            using var stream = await registry.TryOpenFromCacheAsync(id, version, cancellationToken).ConfigureAwait(false);
            if (stream == null)
                return false;

            await InstallInternalAsync(stream, options, registry, false, cancellationToken).ConfigureAwait(false);
            return true;
        }

        public static async Task InstallFromFeedAsync(UniversalPackageId id, UniversalPackageVersion version, UniversalFeedEndpoint feedEndpoint, PackageInstallOptions options, CancellationToken cancellationToken = default)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (version == null)
                throw new ArgumentNullException(nameof(version));
            if (feedEndpoint == null)
                throw new ArgumentNullException(nameof(feedEndpoint));

            var client = new UniversalFeedClient(feedEndpoint);
            using var downloadStream = await GetSeekableStreamAsync(await client.GetPackageStreamAsync(id, version, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
            await InstallFromFileAsync(downloadStream, options, cancellationToken).ConfigureAwait(false);
        }

        private static async Task InstallInternalAsync(Stream packageStream, PackageInstallOptions options, PackageRegistry registry, bool addToCache, CancellationToken cancellationToken)
        {
            UniversalPackageId id;
            UniversalPackageVersion version;

            var reg = registry;
            try
            {
                using (var package = new UniversalPackage(packageStream, true))
                {
                    id = new UniversalPackageId(package.Group, package.Name);
                    version = package.Version;
                    await package.ExtractContentItemsAsync(options.TargetPath, options.Overwrite, cancellationToken).ConfigureAwait(false);
                }

                if (!options.DoNotRegister)
                {
                    reg ??= PackageRegistry.GetRegistry(options.UserRegistry);
                    await reg.LockAsync(cancellationToken).ConfigureAwait(false);
                    await reg.RegisterPackageAsync(
                        new RegisteredPackage
                        {
                            Group = id.Group,
                            Name = id.Name,
                            Version = version.ToString(),
                            InstallPath = options.TargetPath,
                            InstallationDate = DateTimeOffset.Now.ToString("o"),
                            InstallationReason = options.InstallReason,
                            InstalledBy = options.InstalledByUser,
                            InstalledUsing = options.InstalledUsing ?? ("UPackLib/" + typeof(PackageInstaller).Assembly.GetName().Version.ToString())
                        },
                        cancellationToken
                    ).ConfigureAwait(false);
                }

                if (addToCache)
                {
                    reg ??= PackageRegistry.GetRegistry(options.UserRegistry);
                    await reg.LockAsync(cancellationToken).ConfigureAwait(false);
                    packageStream.Position = 0;
                    await reg.WriteToCacheAsync(id, version, packageStream, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                if (reg != registry && reg != null)
                    await reg.UnlockAsync().ConfigureAwait(false);
            }
        }
        private static async Task<Stream> GetSeekableStreamAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (stream.CanSeek)
                return stream;

            var tempPath = Path.GetTempFileName();
            var tempStream = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read | FileShare.Delete, 4096, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
            try
            {
                await stream.CopyToAsync(tempStream, 81920, cancellationToken).ConfigureAwait(false);
                stream.Dispose();
                return tempStream;
            }
            catch
            {
                tempStream.Dispose();
                throw;
            }
        }
    }
}
