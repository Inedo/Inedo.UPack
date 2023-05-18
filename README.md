# UpackLib.NET

[![Build status](https://buildmaster.inedo.com/api/ci-badges/image?API_Key=badges&$ApplicationId=16)](https://buildmaster.inedo.com/api/ci-badges/link?API_Key=badges&$ApplicationId=16)

This library is a work-in-progress set of utilities for working with
[Universal Packages](https://inedo.com/support/documentation/various/universal-packages/universal-feeds-package-ref)
and feeds.

## Installation
Add a reference to Inedo.UPack using NuGet package manager.

## Creating a Package
Use the `UniversalPackageBuilder` class to create a new `.upack` file:

```C#
var metadata = new UniversalPackageMetadata
{
    Group = "my/group",
    Name = "MyPackage",
    Version = UniversalPackageVersion.Parse("1.0.0"),
    Title = "My Test Package",
    Description = "This is where a useful description would go.",
    ["_customProperty"] = "I am a custom, extended property."
};

using (var builder = new UniversalPackageBuilder("MyPackage.upack", metadata))
{
    // Recursively add all files and directories from C:\Test\MyFiles to the package
    await builder.AddContentsAsync(@"C:\Test\MyFiles", "", true);
}
```

## Opening a Package
Create an instance of the `UniversalPackage` class:

```C#
using (var package = new UniversalPackage("MyPackage.upack"))
{
}
```

### Read Package Metadata
Access the properties on a `UniversalPackage` instance:

```C#
Console.WriteLine($"Package name: {package.Name}");
Console.WriteLine($"Package version: {package.Version}");

// The UniversalPackage class only provides basic metadata.
// To get a copy of all metadata, call the GetFullMetadata method.
var fullMetadata = package.GetFullMetadata();
Console.WriteLine($"Package title: {fullMetadata.Title}");
```

### Extract Package Contents
Use the `ExtractContentItemsAsync` method on a `UniversalPackage` instance:

```C#
await package.ExtractContentItemsAsync(@"C:\Test\UnpackedPackage");
```

## Opening the Package Registry
Use the `GetRegistry` method on the `PackageRegistry` class:

```C#
using (var registry = PackageRegistry.GetRegistry(true /*true for user registry*/))
{
}
```

### Registering a Package
Lock the package registry and call the `RegisterPackageAsync` method on a `PackageRegistry` instance:

```C#
await registry.LockAsync();
await registry.RegisterPackageAsync(
    new RegisteredPackage
    {
        Group = "my/group",
        Name = "MyPackage",
        Version = "1.0.0",
        InstallPath = @"C:\Test\UnpackedPackage",
        InstallationDate = DateTimeOffset.Now.ToString("o"),
        InstallationReason = "No reason - just a test!",
        InstalledUsing = "My upack client",
        InstalledBy = "Steve"
    }
);
```

### Getting a List of Registered Packages
Call the `GetInstalledPackagesAsync` method on a `PackageRegistry` instance:

```C#
foreach (var p in await registry.GetInstalledPackagesAsync())
    Console.WriteLine($"{p.Name} {p.Version}");
```

## Connecting to a Universal Package Feed
Create a new instance of the `UniversalFeedClient` class:

```C#
var feed = new UniversalFeedClient("http://upack.example.com/feed");
```

### List Feed Packages in a Group
Call `ListPackagesAsync` on a `UniversalFeedClient` instance:

```C#
var packages = await feed.ListPackagesAsync("my/group", null);
```

### Download a Feed Package to a File
Call `GetPackageStreamAsync` on a `UniversalFeedClient` instance to get a stream:

```C#
using (var packageStream = await feed.GetPackageStreamAsync(UniversalPackageId.Parse("my/group/MyPackage"), UniversalPackageVersion.Parse("1.0.0")))
using (var fileStream = File.Create(@"C:\Test\DownloadedPackage.upack"))
{
    await packageStream.CopyToAsync(fileStream);
}
```

### Upload a Package to a Feed
Call `UploadPackageAsync` on a `UniversalFeedClient` instance:

```C#
using (var fileStream = File.OpenRead(@"C:\Test\SourcePackage.upack"))
{
    await feed.UploadPackageAsync(fileStream);
}
```
