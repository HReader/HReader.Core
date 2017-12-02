using System;
using System.IO;
using System.Threading.Tasks;

namespace HReader.Core.Caching
{
    public interface IReaderCache : IDisposable
    {
        bool HasPrevious { get; }
        bool HasNext { get; }
        Task<Stream> NavigateNextAsync();
        Task<Stream> NavigatePreviousAsync();
        Task<Stream> NavigateToAsync(int index);
    }
}