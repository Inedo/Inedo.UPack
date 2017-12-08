namespace Inedo.UPack.Packaging
{
    /// <summary>
    /// Represents a package registration entry.
    /// </summary>
    public sealed class RegisteredPackage : IRegisteredPackage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisteredPackage"/> class.
        /// </summary>
        public RegisteredPackage()
        {
        }

        /// <summary>
        /// Gets or sets the group of the package.
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// Gets or sets the name of the package.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the version of the package.
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Gets or sets the path where the package is installed.
        /// </summary>
        public string InstallPath { get; set; }
        /// <summary>
        /// Gets or sets the URL of feed where the package was installed from.
        /// </summary>
        public string FeedUrl { get; set; }
        /// <summary>
        /// Gets or sets the date of the package installation.
        /// </summary>
        public string InstallationDate { get; set; }
        /// <summary>
        /// Gets or sets the documented reason for the package installation.
        /// </summary>
        public string InstallationReason { get; set; }
        /// <summary>
        /// Gets or sets the name of the tool used to install the package.
        /// </summary>
        public string InstalledUsing { get; set; }
        /// <summary>
        /// Gets or sets the name of the user that installed the package.
        /// </summary>
        public string InstalledBy { get; set; }

        /// <summary>
        /// Returns the full name of the package.
        /// </summary>
        /// <returns>Full name of the package.</returns>
        public override string ToString() => AH.FormatName(this.Group, this.Name);
    }
}
