using System;
using System.IO;
using System.Threading.Tasks;
using HReader.Core.Sources;

namespace HReader.Core.Caching
{
    public sealed partial class MemoryReaderCache
    {
        private sealed class CacheItem : IDisposable
        {
            private readonly ISourceManager sourceManager;

            private readonly Uri uri;
            private Task progress;
            private MemoryStream stream;

            private bool isDisposed;

            public CacheItem(ISourceManager sourceManager, Uri uri)
            {
                this.sourceManager = sourceManager;
                this.uri = uri;
            }

            private async Task ConsumeStream(Stream s)
            {
                if (isDisposed) return;
                stream = new MemoryStream();
                await s.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }

            public void Start()
            {
                if (isDisposed) return;
                if (progress != null) return;
                progress = sourceManager.ConsumeAsync(uri, ConsumeStream);
            }

            public void Invalidate()
            {
                if (isDisposed) return;
                if (progress == null) return;

                if (progress.IsCompleted)
                {
                    stream?.Dispose();
                    stream = null;
                }
                else
                {
                    // TODO: figure out what we're going to do if we're trying to invalidate an item that is currently loading
                }
            }

            public async Task<MemoryStream> GetData()
            {
                if (isDisposed) return null;
                Start();
                await progress;
                stream?.Seek(0, SeekOrigin.Begin);
                return stream;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                isDisposed = true;
                stream?.Dispose();
            }
        }
    }
}