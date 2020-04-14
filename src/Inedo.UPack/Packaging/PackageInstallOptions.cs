using System;
using System.Collections.Generic;
using System.Text;

namespace Inedo.UPack.Packaging
{
    /// <summary>
    /// Input arguments supplied to methods on <see cref="PackageInstaller"/>.
    /// </summary>
    public sealed class PackageInstallOptions
    {
        /// <summary>
        /// Gets or sets the target directory to install the package to.
        /// </summary>
        public string TargetPath { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether files should be overwritten when the package is extracted to the target.
        /// </summary>
        public bool Overwrite { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to use the user-level registry (true) or the system-level registry (false).
        /// </summary>
        public bool UserRegistry { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the package should be registered.
        /// </summary>
        public bool DoNotRegister { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to add the package to the registry's cache.
        /// </summary>
        public bool AddToCache { get; set; }
        /// <summary>
        /// Gets or sets the recorded install reason to be written to the package registry.
        /// </summary>
        public string InstallReason { get; set; }
        /// <summary>
        /// Gets or sets the recorded install user to be written to the package registry.
        /// </summary>
        public string InstalledByUser { get; set; }
        /// <summary>
        /// Gets or sets the recorded installed using app to be written to the package registry.
        /// </summary>
        public string InstalledUsing { get; set; }
    }
}
