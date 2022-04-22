using System.Diagnostics;

namespace Inedo.UPack.Net
{
    /// <summary>
    /// Default <see cref="HttpClient"/> factory that reuses handlers for five minutes.
    /// </summary>
    internal static class InternalHttpClientFactory
    {
        private const int CleanupMilliseconds = 60 * 2 * 1000;
        private static readonly long StaleTicks = Stopwatch.Frequency * 60 * 5;
        private static readonly List<UsedHandler> current = new();
        private static readonly List<UsedHandler> staleHandlers = new();
        private static readonly object syncLock = new();
        private static Timer? cleanupTimer;

        public static HttpClient GetClient(ApiRequest r)
        {
            lock (syncLock)
            {
                if (cleanupTimer == null)
                    cleanupTimer = new Timer(Cleanup_Tick, null, CleanupMilliseconds, CleanupMilliseconds);

                foreach (var h in current)
                {
                    if (h.Handler.UseDefaultCredentials == r.Endpoint.UseDefaultCredentials)
                        return h.CreateClient();
                }

                var handler = new UsedHandler(new HttpClientHandler { UseDefaultCredentials = r.Endpoint.UseDefaultCredentials });
                current.Add(handler);
                return handler.CreateClient();
            }
        }

        private static void Cleanup_Tick(object? _)
        {
            lock (syncLock)
            {
                var currentTime = Stopwatch.GetTimestamp();
                List<UsedHandler>? removeList = null;

                foreach (var h in current)
                {
                    if (currentTime - h.Created >= StaleTicks)
                    {
                        removeList ??= new();
                        removeList.Add(h);
                    }
                }

                if (removeList != null)
                {
                    foreach (var h in removeList)
                        staleHandlers.Add(h);

                    removeList.Clear();
                }

                foreach (var h in staleHandlers)
                {
                    if (h.CanDispose)
                    {
                        removeList ??= new();
                        removeList.Add(h);
                        h.Handler.Dispose();
                    }
                }

                if (removeList != null)
                {
                    foreach (var h in removeList)
                        staleHandlers.Remove(h);
                }

                if (current.Count == 0 && staleHandlers.Count == 0)
                {
                    // dispose the timer if we don't need to clean anything else up
                    cleanupTimer?.Dispose();
                    cleanupTimer = null;
                }
            }
        }

        private sealed class UsedHandler
        {
            public UsedHandler(HttpClientHandler handler)
            {
                this.Handler = handler;
            }

            public HttpClientHandler Handler { get; }
            public List<WeakReference> Clients { get; } = new();
            public long Created { get; } = Stopwatch.GetTimestamp();
            public bool CanDispose
            {
                get
                {
                    foreach (var r in this.Clients)
                    {
                        if (r.IsAlive)
                            return false;
                    }

                    return true;
                }
            }

            public HttpClient CreateClient()
            {
                var client = new HttpClient(this.Handler, false);
                this.Clients.Add(new WeakReference(client));
                return client;
            }
        }
    }
}
