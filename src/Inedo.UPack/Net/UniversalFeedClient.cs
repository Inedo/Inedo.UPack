using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Inedo.UPack.Net
{
    /// <summary>
    /// Provides access to the universal feed API.
    /// </summary>
    public sealed class UniversalFeedClient
    {
        private readonly ApiTransport transport;
        private readonly Lazy<LocalPackageRepository> localRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalFeedClient"/> class.
        /// </summary>
        /// <param name="uri">The uri of the feed API endpoint.</param>
        /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null or empty.</exception>
        /// <exception cref="ArgumentException"><paramref name="uri"/> is invalid.</exception>
        public UniversalFeedClient(string uri)
            : this(new UniversalFeedEndpoint(uri))
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalFeedClient"/> class.
        /// </summary>
        /// <param name="endpoint">Connection information for the remote endpoint.</param>
        /// <param name="transport">Transport layer to use for requests. The default is <see cref="DefaultApiTransport"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="endpoint"/> is null.</exception>
        public UniversalFeedClient(UniversalFeedEndpoint endpoint, ApiTransport? transport = null)
        {
            this.Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            this.transport = transport ?? new DefaultApiTransport();
            this.localRepository = new Lazy<LocalPackageRepository>(initLocalRepository);

            LocalPackageRepository initLocalRepository()
            {
                if (this.Endpoint.IsLocalDirectory)
                    return new LocalPackageRepository(this.Endpoint.Uri.LocalPath);
                else
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets the endpoint connection information.
        /// </summary>
        public UniversalFeedEndpoint Endpoint { get; }

        /// <summary>
        /// Returns a list of packages with the specified group.
        /// </summary>
        /// <param name="group">Group of packages to return. Null indicates all packages are desired.</param>
        /// <param name="maxCount">Maximum number of packages to return. Null indicates no limit.</param>
        /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
        /// <returns>List of packages in the specified group.</returns>
        public async IAsyncEnumerable<RemoteUniversalPackage> EnumeratePackagesAsync(string? group, int? maxCount, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (this.Endpoint.IsLocalDirectory)
            {
                foreach (var p in this.localRepository.Value.ListPackages(group).Take(maxCount ?? int.MaxValue))
                    yield return p;

                yield break;
            }

            var url = FormatUrl("packages", ("group", group), ("count", maxCount));

            var request = new ApiRequest(this.Endpoint, url);
            using var response = await this.transport.GetResponseAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) != true)
                throw new InvalidDataException($"Server returned {response.ContentType} content type; expected application/json.");

            using var responseStream = await response.GetResponseStreamAsync(cancellationToken).ConfigureAwait(false);
            using var jdoc = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (jdoc.RootElement.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException($"Server returned {jdoc.RootElement.ValueKind}; expected Array.");

            foreach (var item in jdoc.RootElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    throw new InvalidDataException("Unexpected token in JSON array.");

                yield return new RemoteUniversalPackage(JsonObject.Create(item)!);
            }
        }
        /// <summary>
        /// Returns a list of all package versions optionally filtered by group.
        /// </summary>
        /// <param name="group">Group of the package.</param>
        /// <param name="maxCount">Maximum number of versions to return. Null indicates no limit.</param>
        /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
        /// <returns>List of all package versions.</returns>
        public IAsyncEnumerable<RemoteUniversalPackageVersion> EnumeratePackageVersionsAsync(string? group = null, int? maxCount = null, CancellationToken cancellationToken = default)
        {
            return this.EnumerateVersionsInternalAsync(group, null, null, false, maxCount, cancellationToken);
        }

        /// <summary>
        /// Returns a list of packages with the specified group.
        /// </summary>
        /// <param name="group">Group of packages to return. Null indicates all packages are desired.</param>
        /// <param name="maxCount">Maximum number of packages to return. Null indicates no limit.</param>
        /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
        /// <returns>List of packages in the specified group.</returns>
        public async Task<IReadOnlyList<RemoteUniversalPackage>> ListPackagesAsync(string? group, int? maxCount, CancellationToken cancellationToken = default)
        {
            if (this.Endpoint.IsLocalDirectory)
                return this.localRepository.Value.ListPackages(group).ToList();

            var url = FormatUrl("packages", ("group", group), ("count", maxCount));

            var request = new ApiRequest(this.Endpoint, url);
            using var response = await this.transport.GetResponseAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) != true)
                throw new InvalidDataException($"Server returned {response.ContentType} content type; expected application/json.");

            using var responseStream = await response.GetResponseStreamAsync(cancellationToken).ConfigureAwait(false);

            var arr = (JsonArray)JsonNode.Parse(responseStream)!;
            var results = new List<RemoteUniversalPackage>(arr.Count);

            foreach (var token in arr)
            {
                if (token is not JsonObject obj)
                    throw new InvalidDataException("Unexpected token in JSON array.");

                results.Add(new RemoteUniversalPackage(obj));
            }

            return results.AsReadOnly();
        }
        /// <summary>
        /// Returns a list of packages that contain the specified search term.
        /// </summary>
        /// <param name="searchTerm">Text to search package metadata for.</param>
        /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
        /// <returns>List of packages that match the specified search term.</returns>
        public async Task<IReadOnlyList<RemoteUniversalPackage>> SearchPackagesAsync(string? searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return await this.ListPackagesAsync(null, null, cancellationToken).ConfigureAwait(false);

            if (this.Endpoint.IsLocalDirectory)
                return this.localRepository.Value.SearchPackages(searchTerm).ToList();

            var url = FormatUrl("search", ("term", searchTerm));

            var request = new ApiRequest(this.Endpoint, url);
            using var response = await this.transport.GetResponseAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) != true)
                throw new InvalidDataException($"Server returned {response.ContentType} content type; expected application/json.");

            using var responseStream = await response.GetResponseStreamAsync(cancellationToken).ConfigureAwait(false);
            var arr = (JsonArray)JsonNode.Parse(responseStream)!;
            var results = new List<RemoteUniversalPackage>(arr.Count);

            foreach (var token in arr)
            {
                if (token is not JsonObject obj)
                    throw new InvalidDataException("Unexpected token in JSON array.");

                results.Add(new RemoteUniversalPackage(obj));
            }

            return results.AsReadOnly();
        }
        /// <summary>
        /// Returns a list of all package versions with the specified package ID.
        /// </summary>
        /// <param name="id">Full name of the package.</param>
        /// <param name="includeFileList">Value indicating whether a list of files inside the package should be included. This will incur additional overhead.</param>
        /// <param name="maxCount">Maximum number of versions to return. Null indicates no limit.</param>
        /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
        /// <exception cref="ArgumentNullException"><paramref name="id"/> is null.</exception>
        /// <returns>List of all package versions.</returns>
        public Task<IReadOnlyList<RemoteUniversalPackageVersion>> ListPackageVersionsAsync(UniversalPackageId? id, bool includeFileList = false, int? maxCount = null, CancellationToken cancellationToken = default) => this.ListVersionsInternalAsync(id, null, includeFileList, maxCount, cancellationToken);
        /// <summary>
        /// Returns metadata for a specific version of a package, or null if the package was not found.
        /// </summary>
        /// <param name="id">Full name of the package.</param>
        /// <param name="version">Version of the package.</param>
        /// <param name="includeFileList">Value indicating whether a list of files inside the package should be included. This will incur additional overhead.</param>
        /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
        /// <exception cref="ArgumentNullException"><paramref name="id"/> is null or <paramref name="version"/> is null.</exception>
        /// <returns>Package vesion metadata if it was found; otherwise null.</returns>
        public async Task<RemoteUniversalPackageVersion?> GetPackageVersionAsync(UniversalPackageId id, UniversalPackageVersion version, bool includeFileList = false, CancellationToken cancellationToken = default)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            var results = await this.ListVersionsInternalAsync(id, version, includeFileList, null, cancellationToken).ConfigureAwait(false);
            return results.Count > 0 ? results[0] : null;
        }
        /// <summary>
        /// Returns a stream containing the specified package if it is available; otherwise null.
        /// </summary>
        /// <param name="id">Full name of the package.</param>
        /// <param name="version">Version of the package. Specify null for the latest version.</param>
        /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
        /// <returns>Stream containing the specified package if it is available; otherwise null.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="id"/> is null.</exception>
        /// <remarks>
        /// The stream returned by this method is not buffered at all. If random access is required, the caller must
        /// first copy it to another stream.
        /// </remarks>
        public async Task<Stream?> GetPackageStreamAsync(UniversalPackageId id, UniversalPackageVersion? version, CancellationToken cancellationToken = default)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            if (this.Endpoint.IsLocalDirectory)
                return this.localRepository.Value.GetPackageStream(id, version);

            var url = "download/" + Uri.EscapeUriString(id.ToString());
            if (version != null)
                url += "/" + Uri.EscapeUriString(version.ToString());
            else
                url += "?latest";

            var request = new ApiRequest(this.Endpoint, url);
            var response = await this.transport.GetResponseAsync(request, cancellationToken).ConfigureAwait(false);
            try
            {
                if (response.StatusCode == 404)
                {
                    response.Dispose();
                    return null;
                }

                response.ThrowIfNotSuccessful();

                return await response.GetResponseStreamAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                response?.Dispose();
                throw;
            }
        }
        /// <summary>
        /// Returns a stream containing the file at the specified path in the specified package if it is available; otherwise null.
        /// </summary>
        /// <param name="id">Full name of the package.</param>
        /// <param name="version">Version of the package. Specify null for the latest version.</param>
        /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
        /// <param name="filePath">Path of the file inside the package.</param>
        /// <returns>Stream containing the specified package if it is available; otherwise null.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="id"/> is null.</exception>
        /// <remarks>
        /// The stream returned by this method is not buffered at all. If random access is required, the caller must
        /// first copy it to another stream.
        /// </remarks>
        public async Task<Stream?> GetPackageFileStreamAsync(UniversalPackageId id, UniversalPackageVersion? version, string filePath, CancellationToken cancellationToken = default)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (this.Endpoint.IsLocalDirectory)
                return this.localRepository.Value.GetPackageFileStream(id, version, filePath);

            var url = "download-file/" + Uri.EscapeUriString(id.ToString());
            if (version != null)
                url += "/" + Uri.EscapeUriString(version.ToString()) + "?path=" + Uri.EscapeDataString(filePath);
            else
                url += "?latest&path=" + Uri.EscapeDataString(filePath);

            var request = new ApiRequest(this.Endpoint, url);
            var response = await this.transport.GetResponseAsync(request, cancellationToken).ConfigureAwait(false);
            try
            {
                return await response.GetResponseStreamAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                response?.Dispose();
                throw;
            }
        }
        /// <summary>
        /// Uploads the package in the specified stream to the feed.
        /// </summary>
        /// <param name="stream">Stream containing a universal package.</param>
        /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        public async Task UploadPackageAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (this.Endpoint.IsLocalDirectory)
                throw new NotSupportedException();

            var request = new ApiRequest(this.Endpoint, "upload", method: "PUT", contentType: "application/zip", requestBody: stream);
            using var response = await this.transport.GetResponseAsync(request, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Deletes the specified package from the remote feed.
        /// </summary>
        /// <param name="id">Full name of the package.</param>
        /// <param name="version">Version of the package.</param>
        /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
        /// <exception cref="ArgumentNullException"><paramref name="id"/> is null or <paramref name="version"/> is null.</exception>
        public async Task DeletePackageAsync(UniversalPackageId id, UniversalPackageVersion version, CancellationToken cancellationToken = default)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            if (this.Endpoint.IsLocalDirectory)
                throw new NotSupportedException();

            var url = "delete/" + Uri.EscapeUriString(id.ToString()) + "/" + Uri.EscapeUriString(version.ToString());
            var request = new ApiRequest(this.Endpoint, url, method: "DELETE");

            using var response = await this.transport.GetResponseAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private async Task<IReadOnlyList<RemoteUniversalPackageVersion>> ListVersionsInternalAsync(UniversalPackageId? id, UniversalPackageVersion? version, bool includeFileList, int? maxCount, CancellationToken cancellationToken)
        {
            if (this.Endpoint.IsLocalDirectory)
            {
                if (id == null || version == null)
                {
                    return this.localRepository.Value.ListPackageVersions(id?.Group, id?.Name).ToList();
                }
                else
                {
                    var v = this.localRepository.Value.GetPackageVersion(id, version);
                    return v != null ? new[] { v } : Array.Empty<RemoteUniversalPackageVersion>();
                }
            }

            var url = FormatUrl("versions", ("group", id?.Group), ("name", id?.Name), ("version", version?.ToString()), ("includeFileList", includeFileList), ("count", maxCount));
            var request = new ApiRequest(this.Endpoint, url);
            using var response = await this.transport.GetResponseAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) != true)
                throw new InvalidDataException($"Server returned {response.ContentType} content type; expected application/json.");

            using var responseStream = await response.GetResponseStreamAsync(cancellationToken).ConfigureAwait(false);
            if (version == null)
            {
                var arr = (JsonArray)JsonNode.Parse(responseStream)!;
                var results = new List<RemoteUniversalPackageVersion>(arr.Count);

                foreach (var token in arr)
                {
                    if (token is not JsonObject obj)
                        throw new InvalidDataException("Unexpected token in JSON array.");

                    results.Add(new RemoteUniversalPackageVersion(obj));
                }

                return results.AsReadOnly();
            }
            else
            {
                var obj = (JsonObject)JsonNode.Parse(responseStream)!;
                return new[] { new RemoteUniversalPackageVersion(obj) };
            }
        }

        private async IAsyncEnumerable<RemoteUniversalPackageVersion> EnumerateVersionsInternalAsync(string? group, string? name, UniversalPackageVersion? version, bool includeFileList, int? maxCount, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (this.Endpoint.IsLocalDirectory)
            {
                IEnumerable<RemoteUniversalPackageVersion> localPackages;
                if (name == null || version == null)
                {
                    localPackages = this.localRepository.Value.ListPackageVersions(group, name);
                }
                else
                {
                    var v = this.localRepository.Value.GetPackageVersion(new UniversalPackageId(group, name), version);
                    localPackages = v != null ? new[] { v } : Enumerable.Empty<RemoteUniversalPackageVersion>();
                }

                foreach (var p in localPackages)
                    yield return p;

                yield break;
            }

            var url = FormatUrl("versions", ("group", group), ("name", name), ("version", version?.ToString()), ("includeFileList", includeFileList), ("count", maxCount));
            var request = new ApiRequest(this.Endpoint, url);
            using var response = await this.transport.GetResponseAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) != true)
                throw new InvalidDataException($"Server returned {response.ContentType} content type; expected application/json.");

            using var responseStream = await response.GetResponseStreamAsync(cancellationToken).ConfigureAwait(false);
            if (version == null)
            {
                using var doc = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false);
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object)
                        throw new InvalidDataException("Unexpected token in JSON array.");

                    yield return new RemoteUniversalPackageVersion(JsonObject.Create(item)!);
                }
            }
            else
            {
                var obj = (JsonObject)JsonNode.Parse(responseStream)!;
                yield return new RemoteUniversalPackageVersion(obj);
            }
        }

        private static string FormatUrl(string url, params (string key, object? value)[] query)
        {
            if (query.Length == 0)
                return url;

            var items = query.Where(i => !string.IsNullOrEmpty(i.value?.ToString())).ToList();
            if (items.Count == 0)
                return url;

            return url + "?" + string.Join("&", items.Select(i => Uri.EscapeDataString(i.key) + "=" + Uri.EscapeDataString(i.value!.ToString()!)));
        }
    }
}
