using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using HReader.Base.Data;
using HReader.Core.Pagination;

namespace HReader.Core.Storage
{
    public interface ISearchQuery
    {
        //TODO: figure out how searching should be parameterized to allow for most customizable search
    }

    public interface IMetadataRepository : IDisposable, INotifyPropertyChanged
    {
        Task<IPagination<IPage<IMetadata>>> GetAllEntriesAsync();
        Task<IPagination<IPage<IMetadata>>> SearchAsync(ISearchQuery query);
        Task AddAsync(IMetadata entry);

        Task RemoveAsync(IMetadata entry);
        Task RemoveAsync(IEnumerable<IMetadata> entry);
    }
}