using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Inedo.UPack.Packaging
{
    /// <summary>
    /// Provides synchronized access to the user or machine level upack package registry.
    /// </summary>
    public sealed class PackageRegistry : IDisposable
    {
        private bool disposed;

        private PackageRegistry(string registryRoot) => this.RegistryRoot = registryRoot;

        /// <summary>
        /// Gets the root directory of the package registry.
        /// </summary>
        public string RegistryRoot { get; }
        /// <summary>
        /// Gets the current lock token if a lock is taken; otherwise null.
        /// </summary>
        public string LockToken { get; private set; }

        /// <summary>
        /// Returns an instance of the <see cref="PackageRegistry"/> class that represents a package registry on the system.
        /// </summary>
        /// <param name="openUserRegistry">Value indicating whether to open the current user's registry (true) or the machine registry (false).</param>
        /// <returns>Instance of the <see cref="PackageRegistry"/> class.</returns>
        public static PackageRegistry GetRegistry(bool openUserRegistry)
        {
            var root = openUserRegistry ? GetCurrentUserRegistryRoot() : GetMachineRegistryRoot();
            return new PackageRegistry(root);
        }
        /// <summary>
        /// Returns the directory where the machine registry is stored on the current system.
        /// </summary>
        /// <returns>Directory where the machine registry is stored on the current system.</returns>
        public static string GetMachineRegistryRoot() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "upack");
        /// <summary>
        /// Returns the directory where the current user's registry is stored on the current system.
        /// </summary>
        /// <returns>Directory where the current user's registry is stored on the current system.</returns>
        public static string GetCurrentUserRegistryRoot() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".upack");

        /// <summary>
        /// Attempts to acquire an exclusive lock on the package registry.
        /// </summary>
        public Task LockAsync() => this.LockRegistryAsync(null, default);
        /// <summary>
        /// Attempts to acquire an exclusive lock on the package registry.
        /// </summary>
        /// <param name="reason">Reason for the lock. This will be generated if not specified.</param>
        public Task LockAsync(string reason) => this.LockRegistryAsync(null, default);
        /// <summary>
        /// Attempts to acquire an exclusive lock on the package registry.
        /// </summary>
        /// <param name="reason">Reason for the lock. This will be generated if not specified.</param>
        /// <param name="cancellationToken">Token used to cancel the lock acquisition.</param>
        public Task LockAsync(string reason, CancellationToken cancellationToken) => this.LockRegistryAsync(null, cancellationToken);
        /// <summary>
        /// Attempts to acquire an exclusive lock on the package registry.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the lock acquisition.</param>
        public Task LockAsync(CancellationToken cancellationToken) => this.LockRegistryAsync(null, cancellationToken);
        /// <summary>
        /// Releases an exclusive lock previously acquired on the package registry.
        /// </summary>
        public Task UnlockAsync()
        {
            this.UnlockRegistry();
            return AH.CompletedTask;
        }
        /// <summary>
        /// Returns a list of packages in the registry.
        /// </summary>
        /// <returns>List of all packages in the registry.</returns>
        public Task<IReadOnlyList<RegisteredPackage>> GetInstalledPackagesAsync() => Task.FromResult<IReadOnlyList<RegisteredPackage>>(GetInstalledPackages(this.RegistryRoot));
        /// <summary>
        /// Adds or replaces a package registration entry in the registry.
        /// </summary>
        /// <param name="package">The package to register.</param>
        /// <exception cref="ArgumentNullException"><paramref name="package"/> is null.</exception>
        public Task RegisterPackageAsync(RegisteredPackage package) => this.RegisterPackageAsync(package, default);
        /// <summary>
        /// Adds or replaces a package registration entry in the registry.
        /// </summary>
        /// <param name="package">The package to register.</param>
        /// <param name="cancellationToken">Token used to cancel the package registration.</param>
        /// <exception cref="ArgumentNullException"><paramref name="package"/> is null.</exception>
        public Task RegisterPackageAsync(RegisteredPackage package, CancellationToken cancellationToken)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            var packages = GetInstalledPackages(this.RegistryRoot);

            packages.RemoveAll(p => PackageNameAndGroupEquals(p, package));
            packages.Add(package);

            WriteInstalledPackages(this.RegistryRoot, packages);
            return AH.CompletedTask;
        }
        /// <summary>
        /// Removes a package registration entry from the registry.
        /// </summary>
        /// <param name="package">THe package to unregister.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>True if package was unregistered; false if it was not in the registry.</returns>
        public Task<bool> UnregisterPackageAsync(RegisteredPackage package, CancellationToken cancellationToken)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            var packages = GetInstalledPackages(this.RegistryRoot);

            bool removed = packages.RemoveAll(p => PackageNameAndGroupEquals(p, package)) > 0;
            if (removed)
                WriteInstalledPackages(this.RegistryRoot, packages);

            return Task.FromResult(removed);
        }
        /// <summary>
        /// Removes a package registration entry from the registry.
        /// </summary>
        /// <param name="package">THe package to unregister.</param>
        /// <returns>True if package was unregistered; false if it was not in the registry.</returns>
        public Task<bool> UnregisterPackageAsync(RegisteredPackage package) => this.UnregisterPackageAsync(package, default);

        void IDisposable.Dispose()
        {
            if (!this.disposed)
            {
                if (this.LockToken != null)
                {
                    try
                    {
                        this.UnlockRegistry();
                    }
                    catch
                    {
                    }
                }

                this.disposed = true;
            }
        }

        private async Task LockRegistryAsync(string reason, CancellationToken cancellationToken)
        {
            var fileName = Path.Combine(this.RegistryRoot, ".lock");

            var lockDescription = GetLockReason(reason);
            var lockToken = Guid.NewGuid().ToString();

            TryAgain:
            var fileInfo = getFileInfo();
            while (fileInfo != null && DateTime.UtcNow - fileInfo.LastWriteTimeUtc <= new TimeSpan(0, 0, 10))
            {
                await Task.Delay(500, cancellationToken).ConfigureAwait(false);
                fileInfo = getFileInfo();
            }

            // ensure registry root exists
            Directory.CreateDirectory(this.RegistryRoot);

            try
            {
                // write out the lock info
                using (var lockStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                using (var writer = new StreamWriter(lockStream, AH.UTF8))
                {
                    writer.WriteLine(lockDescription);
                    writer.WriteLine(lockToken.ToString());
                }

                // verify that we acquired the lock
                using (var lockStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(lockStream, AH.UTF8))
                {
                    if (reader.ReadLine() != lockDescription)
                        goto TryAgain;

                    if (reader.ReadLine() != lockToken)
                        goto TryAgain;
                }
            }
            catch (IOException)
            {
                // file may be in use by other process
                goto TryAgain;
            }

            // at this point, lock is acquired provided everyone is following the rules
            this.LockToken = lockToken;

            FileInfo getFileInfo()
            {
                try
                {
                    var info = new FileInfo(fileName);
                    if (!info.Exists)
                        return null;
                    return info;
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
                catch (DirectoryNotFoundException)
                {
                    return null;
                }
            }
        }
        private void UnlockRegistry()
        {
            if (this.LockToken == null)
                return;

            var fileName = Path.Combine(this.RegistryRoot, ".lock");
            if (!File.Exists(fileName))
                return;

            string token;
            using (var lockStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(lockStream, AH.UTF8))
            {
                reader.ReadLine();
                token = reader.ReadLine();
            }

            if (token == this.LockToken)
                File.Delete(fileName);

            this.LockToken = null;
        }
        private static List<RegisteredPackage> GetInstalledPackages(string registryRoot)
        {
            var fileName = Path.Combine(registryRoot, "installedPackages.json");

            if (!File.Exists(fileName))
                return new List<RegisteredPackage>();

            using (var configStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var streamReader = new StreamReader(configStream, AH.UTF8))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                return (new JsonSerializer().Deserialize<RegisteredPackage[]>(jsonReader) ?? new RegisteredPackage[0])
                    .ToList();
            }
        }
        private static void WriteInstalledPackages(string registryRoot, IEnumerable<IRegisteredPackage> packages)
        {
            Directory.CreateDirectory(registryRoot);
            var fileName = Path.Combine(registryRoot, "installedPackages.json");

            using (var configStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (var streamWriter = new StreamWriter(configStream, AH.UTF8))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                new JsonSerializer { Formatting = Formatting.Indented }.Serialize(jsonWriter, packages.ToArray());
            }
        }
        private static bool PackageNameAndGroupEquals(IRegisteredPackage p1, IRegisteredPackage p2)
        {
            if (ReferenceEquals(p1, p2))
                return true;
            if (ReferenceEquals(p1, null) | ReferenceEquals(p2, null))
                return false;

            return string.Equals(p1.Group ?? string.Empty, p2.Group ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                && string.Equals(p1.Name, p2.Name, StringComparison.OrdinalIgnoreCase);
        }
        private static string GetLockReason(string reason)
        {
            if (!string.IsNullOrWhiteSpace(reason))
                return reason;

            var asm = Assembly.GetEntryAssembly();
            if (asm != null)
            {
                var title = asm.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
                if (!string.IsNullOrWhiteSpace(title))
                    return "Locked by " + title;

                return "Locked by " + asm.GetName().Name;
            }

            return "Locked for update";
        }
    }
}
