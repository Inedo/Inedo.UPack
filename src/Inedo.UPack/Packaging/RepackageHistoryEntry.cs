using System;

namespace Inedo.UPack.Packaging
{
    /// <summary>
    /// Contains information for a repackage event.
    /// </summary>
    [Serializable]
    public sealed class RepackageHistoryEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepackageHistoryEntry"/> class.
        /// </summary>
        public RepackageHistoryEntry()
        {
        }

        /// <summary>
        /// Gets or sets the package identification string.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets date when the package was first created.
        /// </summary>
        public DateTimeOffset? CreatedDate { get; set; }
        /// <summary>
        /// Gets or sets a string describing the reason or purpose of the creation.
        /// </summary>
        public string CreatedReason { get; set; }
        /// <summary>
        /// Gets or sets a string describing the mechanism the package was created with.
        /// </summary>
        public string CreatedUsing { get; set; }
        /// <summary>
        /// Gets or sets a string describing the person or service that performed the installation.
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// Gets or sets a URL describing where more information about the repackaging can be found.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Returns the <see cref="Id"/> property value.
        /// </summary>
        /// <returns>The Id.</returns>
        public override string ToString() => this.Id;
    }
}
