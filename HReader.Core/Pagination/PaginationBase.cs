using System;
using System.Threading.Tasks;

namespace HReader.Core.Pagination
{
    public abstract class PaginationBase<T> : IPagination<T>
    {
        public abstract bool CanSetPageSize { get; }
        protected abstract Task SetPageSizeImplAsync(int value);
        protected abstract Task SetItemCountAsync();
        protected abstract Task<T> GetPageAsync(int index);

        protected int ActivePageFirstItemIndex => CurrentIndex * PageSize;

        protected int ItemCount { get; set; }

        /// <inheritdoc />
        public int Count => ItemCount % PageSize == 0 ? ItemCount / PageSize : ItemCount / PageSize + 1;

        /// <inheritdoc />
        public int CurrentIndex { get; private set; } = -1;

        /// <inheritdoc />
        public T Current { get; private set; }

        /// <inheritdoc />
        public int PageSize { get; protected set; } = 25;

        public async Task InitializeAsync()
        {
            await SetItemCountAsync();
            await NavigateToAsync(0);
        }

        public async Task SetPageSizeAsync(int value)
        {
            if (!CanSetPageSize) throw new NotSupportedException("This Pagination does not support non-default page sizes");
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            await SetPageSizeImplAsync(value);
        }

        protected async Task NavigateToUncheckedAsync(int index)
        {
            var old = Current;
            Current = await GetPageAsync(index);
            CurrentIndex = index;
            (old as IDisposable)?.Dispose();
        }

        /// <inheritdoc />
        public async Task NavigateToAsync(int index)
        {
            if (Count == 0) return;
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (index == CurrentIndex) return;
            await NavigateToUncheckedAsync(index);
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            (Current as IDisposable)?.Dispose();
        }
    }
}