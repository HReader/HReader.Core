using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using HReader.Base;
using HReader.Base.Data;

namespace HReader.Core.Sources
{
    public interface ISourceManager : IDisposable
    {
        ReadOnlyObservableCollection<IContentSource> Content { get; }
        ReadOnlyObservableCollection<IMetadataSource> Metadata { get; }

        Task<IMetadata> ResolveMetadataAsync(Uri uri);
        Task<bool> ConsumeAsync(Uri uri, Func<Stream, Task> consumer);
    }
}