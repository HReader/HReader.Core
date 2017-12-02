using System;
using System.Threading.Tasks;

namespace HReader.Core.Pagination
{
    public interface IPagination<out T> : IDisposable
    {
        int Count { get; }
        int PageSize { get; }
        int CurrentIndex { get; }
        T Current { get; }

        Task SetPageSizeAsync(int value);
        Task NavigateToAsync(int index);

        bool CanSetPageSize { get; }
    }
}