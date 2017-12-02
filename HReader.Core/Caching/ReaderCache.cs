using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HReader.Core.Sources;

namespace HReader.Core.Caching
{
    public sealed partial class MemoryReaderCache : IReaderCache
    {
        private const int CacheBehindOnForwardCount = 5;
        private const int CacheBehindOnBackwardCount = 10;
        private const int CacheInReadingDirectionCount = 5;

        private const int CacheForwardOnJumpAheadCount = 5;
        private const int CacheBackwardOnJumpAheadCount = 3;

        private const int CacheForwardOnJumpBehindCount = 3;
        private const int CacheBackwardOnJumpBehindCount = 5;

        private readonly IReadOnlyList<CacheItem> items;
        private int currentIndex;

        private bool isDisposed;

        public MemoryReaderCache(IReadOnlyList<Uri> pages, ISourceManager sourceManager)
        {
            items = pages.Select(p => new CacheItem(sourceManager, p)).ToList();
            currentIndex = 0;

            // always start the first 3 pages on construction
            // this allows for smooth navigation right off the bat
            Start(0);
            Start(1);
            Start(2);
        }

        public bool HasPrevious => currentIndex > 0;
        public bool HasNext => currentIndex < items.Count - 1;

        private bool InRange(int index)
        {
            return !(index < 0 || index >= items.Count);
        }

        private void Start(int index)
        {
            if (!InRange(index)) return;
            items[index].Start();
        }

        private void Invalidate(int index)
        {
            if (!InRange(index)) return;
            items[index].Invalidate();
        }
        
        public async Task<Stream> NavigateNextAsync()
        {
            if (isDisposed) return null;
            if (!InRange(currentIndex + 1)) return null;

            currentIndex++;
            
            // since the user is navigating forward they will probably continue
            // going forward, we want to cache a few items ahead for smoother navigation
            // even if the user decides to skip a few pages
            // this also starts the next page immediately to ensure it is ready asap
            CacheForward(currentIndex, CacheInReadingDirectionCount);

            // we invalidate pages that are far behind the current page
            // since the user probably won't navigate back to them anymore
            // since the pages advance one by one all old pages will be
            // invalidated as the user navigates forward
            InvalidateBackward(currentIndex, CacheBehindOnForwardCount);
            
            return await items[currentIndex].GetData();
        }

        public async Task<Stream> NavigatePreviousAsync()
        {
            if (isDisposed) return null;
            if (!InRange(currentIndex - 1)) return null;
            currentIndex--;

            // behaves largely the same as navigating forward
            // but leaves a much bigger cache in the forward direction
            // because the user probably wants to go forward at some point
            // and will then skip through pages they already saw
            CacheBackward(currentIndex, CacheInReadingDirectionCount);
            InvalidateForard(currentIndex, CacheBehindOnBackwardCount);
            
            return await items[currentIndex].GetData();
        }

        public async Task<Stream> NavigateToAsync(int index)
        {
            if (isDisposed) return null;
            if (!InRange(index)) return null;

            if (index == currentIndex) return await items[currentIndex].GetData();

            Start(index);

            // how exactly we cache ahead or behind depends on the direction of the jump
            if (index > currentIndex)
            {
                NavigateDirectForward(index);
            }
            else
            {
                NavigateDirectBackward(index);
            }

            return await items[index].GetData();
        }

        private void NavigateDirectForward(int index)
        {
            CacheForward(index, CacheForwardOnJumpAheadCount);
            CacheBackward(index, CacheBackwardOnJumpAheadCount);
            CleanUp(
                currentIndex,
                index - CacheBackwardOnJumpAheadCount,
                index + CacheForwardOnJumpAheadCount
            );
        }

        private void NavigateDirectBackward(int index)
        {
            CacheBackward(index, CacheBackwardOnJumpBehindCount);
            CacheForward(index, CacheForwardOnJumpBehindCount);
            CleanUp(
                currentIndex,
                index - CacheBackwardOnJumpBehindCount,
                index + CacheForwardOnJumpBehindCount
            );
        }

        private void CleanUp(int oldIndex,int lowerBound, int upperBound)
        {
            // clean up all possibly cached items around the old position
            // unless they are in caching range of the new index
            for (var i = oldIndex - CacheBehindOnForwardCount; i < oldIndex + CacheBehindOnBackwardCount; i++)
            {
                if (!(i <= upperBound && oldIndex >= lowerBound))
                {
                    Invalidate(i);
                }
            }
        }

        private void CacheForward(int index, int count)
        {
            for (var i = index; i < index + count + 1; i++)
            {
                Start(i);
            }
        }

        private void CacheBackward(int index, int count)
        {
            for (var i = index; i > index - count - 1; i--)
            {
                Start(i);
            }
        }

        private void InvalidateForard(int index, int offset)
        {
            Invalidate(index + 1 + offset);
        }

        private void InvalidateBackward(int index, int offset)
        {
            Invalidate(index - 1 - offset);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            isDisposed = true;
            foreach (var cacheItem in items)
            {
                cacheItem.Dispose();
            }
        }
    }
}