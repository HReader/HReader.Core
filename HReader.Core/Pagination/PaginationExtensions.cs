using System;
using System.Text;
using System.Threading.Tasks;

namespace HReader.Core.Pagination
{
    /// <summary>
    ///     Contains methods that offer more varied ways of navigating a <see cref="IPagination{T}"/>.
    /// </summary>
    public static class PaginationExtensions
    {
        public static Task NavigateAsync<T>(this IPagination<T> @this, int offset, NavigationOrigin origin = NavigationOrigin.Current, NavigationDirection direction = NavigationDirection.Forward)
        {
            if (@this == null) throw new ArgumentNullException(nameof(@this));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

            int offsetStart;
            switch (origin)
            {
                case NavigationOrigin.Start:
                    offsetStart = 0;
                    break;
                case NavigationOrigin.Current:
                    offsetStart = @this.CurrentIndex;
                    break;
                case NavigationOrigin.End:
                    offsetStart = @this.Count - 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            if (direction == NavigationDirection.Backward)
            {
                offset *= -1;
            }

            return @this.NavigateToAsync(offsetStart + offset);
        }

        public static Task NavigateToStartAsync<T>(this IPagination<T> @this)
        {
            if (@this == null) throw new ArgumentNullException(nameof(@this));
            return @this.NavigateToAsync(0);
        }

        public static Task NavigateToEndAsync<T>(this IPagination<T> @this)
        {
            if (@this == null) throw new ArgumentNullException(nameof(@this));
            return @this.NavigateToAsync(@this.Count - 1);
        }

        public static Task NavigateToNextAsync<T>(this IPagination<T> @this)
        {
            return NavigateAsync(@this, 1);
        }

        public static Task NavigateToPreviousAsync<T>(this IPagination<T> @this)
        {
            return NavigateAsync(@this, 1, direction: NavigationDirection.Backward);
        }
    }
}
