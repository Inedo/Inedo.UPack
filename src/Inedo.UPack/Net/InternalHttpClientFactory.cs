using System.Diagnostics;

namespace Inedo.UPack.Net
{
    /// <summary>
    /// Default <see cref="HttpClient"/> factory that reuses handlers for five minutes.
    /// </summary>
    internal sealed class InternalHttpClientFactory
    {
        private readonly List<UsedHandler> current = new();
        private readonly List<UsedHandler> staleHandlers = new();
        private readonly object syncLock = new();
        private Timer? cleanupTimer;

        public static InternalHttpClientFactory Instance { get; } = new();

        public int CleanupMilliseconds { get; set; } = 60 * 2 * 1000;
        public long StaleTicks { get; set; } = Stopwatch.Frequency * 60 * 5;

        public HttpClient GetClient(ApiRequest r)
        {
            lock (syncLock)
            {
                this.cleanupTimer ??= new Timer(this.Cleanup_Tick, null, this.CleanupMilliseconds, this.CleanupMilliseconds);

                foreach (var h in this.current)
                {
                    if (h.Handler.UseDefaultCredentials == r.Endpoint.UseDefaultCredentials)
                        return h.CreateClient();
                }

                var handler = new UsedHandler(new HttpClientHandler { UseDefaultCredentials = r.Endpoint.UseDefaultCredentials });
                this.current.Add(handler);
                return handler.CreateClient();
            }
        }

        public void RunCleanup()
        {
            lock (this.syncLock)
            {
                var currentTime = Stopwatch.GetTimestamp();
                List<UsedHandler>? removeList = null;

                foreach (var h in this.current)
                {
                    if (currentTime - h.Created >= this.StaleTicks)
                    {
                        removeList ??= new();
                        removeList.Add(h);
                    }
                }

                if (removeList != null)
                {
                    foreach (var h in removeList)
                    {
                        this.staleHandlers.Add(h);
                        this.current.Remove(h);
                    }

                    removeList.Clear();
                }

                foreach (var h in this.staleHandlers)
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
                        this.staleHandlers.Remove(h);
                }

                if (this.current.Count == 0 && this.staleHandlers.Count == 0)
                {
                    // dispose the timer if we don't need to clean anything else up
                    this.cleanupTimer?.Dispose();
                    this.cleanupTimer = null;
                }
            }

        }

        private void Cleanup_Tick(object? _) => this.RunCleanup();

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
