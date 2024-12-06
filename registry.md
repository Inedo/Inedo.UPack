# Universal Package Registry

This document describes the design and specifications for the local package registry that's used by the `Inedo.UPack` library to track installed packages on a machine in a global, user, or custom context.

## Background: Other Local Package Registries
All packages, whether in universal format or not, are essentially zip files containing the contents (i.e., the actual files you want packaged ) and a metadata file describing the contents. Once the contents of a package are unpacked and installed in a directory, there is no easy way to find out what package that content came from, or if it came from a package at all. This is where a local package registry comes into play.

Different package managers use different mechanisms to represent a local registry. Some store the entire package, others store only metadata about the package, and each approach has advantages and disadvantages.

For example:
* The local registry for MSI (Microsoft Installer) is comprised of an installation cache directory (containing the MSI files installed) as well as various entries in the Windows registry.
* The NuGet local registry is comprised of various package cache directories and a packages.config file stored in the root of a project file that describes which packages are used.
* APT (Advanced Package Tool for Debian) maintains a directory of package-related files in /var/lib/dpkg/info, including MD5sums of each of the various installed files and the executable scripts that would be run before and after installation.

However, in all cases, the packages registered vs. packages installed are not enforced by the package manager or operating system. You can consolidate the data in the local registry if needed, or delete files that were originally installed by the package manager.

## Designing the Universal Package Registry
The Universal Package Registry was developed with the realities of other local package registries in mind. 

* Well-defined specification - this allows a variety of tools (not just an API or CLI) to read or write data
* **Data only** - there is no attempt to automatically reconcile registered vs. installed packages
* **Compliance-driven metadata** - provides a common mechanism to indicate what installed a package and why
* **Any additional metadata** - for compliance or other reasons, you can add additional package metadata

A Universal Package Registry may also include a package cache. There are three common uses cases for this:

* **Auditing** - to verify or compare the original contents of the package registered versus the files installed on disk
* **Repairing installation** - if an installed package has been corrupted (a file at the installation location has been modified or deleted), the package can be reinstalled without requiring a connection to a Universal Feed 
* **Staging packages** - package files could be copied to the cache ahead of installation to allow for rapid deployment of newer versions

## Registry Types and Locations

There are three different types of registries (Machine, User, Custom). Each machine can have any number of user and Custom registries:

### Machine-level Registry

A registry with a known location that can be queried by anyone, and by default requires administrator privileges to install/modify.
* Windows: `%ProgramData%\upack`
* Linux: `/var/lib/upack`
 
### User-level Registry
A registry with a known location based on the current username, which can be changed by the current user and administrators.
 * Windows: `%USER%\.upack`
* Linux: `~/.upack`

### Custom Registry
A registry with a different location and possibly different permissions.


## Registry Specifications

A Universal Package Registry consists of two components: a registry file and an ephemeral lock file. A package cache can also be added, but it's optional. The structure on disk is as follows:

```plaintext
 ‹registry-root›\
   .lock
   installedPackages.json 
   packageCache\
     ‹group$packageName›\ 
       ‹packageName.version›.upack
```

As [with universal packages](https://docs.inedo.com/docs/proget/upack/upack-universal-packages/upack-universal-packages-manifest), you can add any number of files or directories outside of these minimum requirements. However, we strongly recommend that you prefix these files and folders with an underscore (\_) to avoid overlap with files or folders that are added in a future version of the specification. 

### Interacting with a Universal Package Registry and the Lock File

The lack of a registry root directory or installedPackages.json file is not an error condition, but implies that no packages are registered (i.e. have been installed). An invalid `installedPackages.json` file (i.e. not readable as JSON or invalid data) is an error condition and should not be automatically remediated.

The .lock file is used to indicate that another process is currently interacting with the registry. It should only be used when atomically reading/writing the metadata file; changing the package cache should not cause the repository to be locked.

If a .lock file exists, its modification date should be checked against the current system time. If the difference is more than ten seconds, the other process is assumed to have crashed, and the lock file should be deleted. Otherwise, the file should be checked again in this way until the lock is removed.

If no .lock file exists, a process should create a lock file with two lines (\\r or \\r\\n): a human-readable description of the lock (generally the process name) and a lock token (generally a GUID). If, when the operation is complete, the lock token matches, the file should be deleted.

No operation should take longer than one second (let alone ten), and the user should be notified of any exceptions (locked registry, unmatched token).

### Installed Packages JSON Format

The registry file (installedPackages.json) is a JSON-based array of objects with the following properties:

<table><thead><tr><th>Property</th><th>Format</th></tr></thead><tbody><tr><td><code>group</code></td><td rowspan="3"><em>see <a href="https://docs.inedo.com/docs/proget/upack/proget-api-universalfeed">package metadata specs</a></em></td></tr><tr><td><code>name</code><sup>R</sup></td></tr><tr><td><code>version</code><sup>R</sup></td></tr><tr><td><code>path</code></td><td>A <em>string</em> of the absolute path on disk where the package was installed to</td></tr><tr><td><code>feedUrl</code></td><td>A <em>string</em> of an absolute URL of the universal feed where the package was installed from</td></tr><tr><td><code>installationDate</code></td><td>A <em>string</em> representing the UTC date when the package was installed, in ISO 8601 format (yyyy-MM-ddThh:mm:ss)</td></tr><tr><td><code>installationReason</code></td><td>A <em>string</em> describing the reason or purpose of the installation<br><br><em>For example, <a href="https://inedo.com/buildmaster">BuildMaster</a> uses</em> <code>{Application Name}</code> <code>v{Release Number}</code> <code>#{Package Number}</code> <code>(ID{Execution-Number})</code></td></tr><tr><td><code>installationUsing</code></td><td>A <em>string</em> describing the mechanism the package was installed with; there are no format restrictions, but we recommend treating it like a User Agent string and including the tool name and version<br><br><em>For example, BuildMaster uses</em> <code>BuildMaster/5.6.11</code></td></tr><tr><td><code>installationBy</code></td><td>A <em>string</em> describing the person or service that performed the installation<br><br><em>For example, BuildMaster uses the user who triggered the deployment or SYSTEM if it was a triggered/scheduled deployment</em></td></tr></tbody></table>

An `R` denotes a required property, and the object may contain additional properties as needed.

We **strongly recommended** that you prefix these properties with an underscore (\_) so as to not clash with property names that may be added to the specification later.

### Package Uniqueness and Data Constraints

Only one version of a package can be registered at a time. Uniqueness is determined by a combination of the group (or lack thereof) and the package name. A future version of this specification may allow multiple versions of a package, but that will be an "opt-in" setting, likely defined in a (yet to be specified) registry configuration file.

### Package Cache

The package cache is simply a directory containing package files that may currently be installed. It must be named `packageCache`, and contain package files (`packageName.version.upack`) stored in subdirectories comprised of the group name (with $ replacing the /), a $, and the package name. For example:

```plaintext
‹registry-root›\
   packageCache\ 
      $hdars\
         hdars.1.2.3.upack
         hdars.1.2.4.upack
         hdars.2.0.0.upack
      accounting$apps$accounts\ 
         accounts.1.0.2-beta.upack
```

We strongly recommend that the use of caching be explicitly enabled (both when downloading and installing packages). Caching packages by default generally causes more problems than it helps, and can take up a lot of memory if not monitored.
