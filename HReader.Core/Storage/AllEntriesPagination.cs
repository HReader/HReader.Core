using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using HReader.Base.Data;
using HReader.Core.Storage.Queries;
using HReader.Core.Pagination;
using Microsoft.Data.Sqlite;

namespace HReader.Core.Storage
{
    internal class AllEntriesPagination : EntryPagination
    {
        internal AllEntriesPagination(MetadataRepository repository) : base(repository)
        {
            getPageCommand = Repository.Sql
                                       .CreateCommand(QueryManager.Instance.SelectEntryPage)
                                       .WithParameter("$order", "id");

            pageOffsetParameter = getPageCommand.CreateParameter("$pageOffset");
            pageSizeParameter = getPageCommand.CreateParameter("$pageSize");
        }
        
        private readonly SqliteParameter pageOffsetParameter;
        private readonly SqliteParameter pageSizeParameter;
        private readonly SqliteCommand getPageCommand;

        /// <inheritdoc />
        protected override async Task SetPageSizeImplAsync(int value)
        {
            var firstItem = CurrentIndex * PageSize;
            PageSize = value;
            // change the active page to the one the start of the previously active page was on
            await NavigateToAsync(firstItem / PageSize);
        }

        /// <inheritdoc />
        protected override async Task SetItemCountAsync()
        {
            ItemCount = (int) await Repository.Sql
                                              .CreateCommand(QueryManager.Instance.SelectEntryCount)
                                              .ConsumeScalarAsync<long>();
        }

        /// <inheritdoc />
        protected override async Task<IPage<IMetadata>> GetPageAsync(int index)
        {
            pageSizeParameter.Value = PageSize;
            pageOffsetParameter.Value = PageSize * index;

            var builder = ImmutableList.CreateBuilder<IMetadata>();

            using (var reader = await getPageCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var id = reader.GetFieldValue<long>(0);

                    var tags = SelectTagsAsync(id);
                    var chars = SelectCharactersAsync(id);
                    var series = SelectSeriesAsync(id);
                    var artists = SelectArtistsAsync(id);
                    var pages = SelectPagesAsync(id);

                    await Task.WhenAll(tags, chars, series, artists, pages);

                    builder.Add(new KeyedMetadata(
                        id,
                        new Kind(reader.GetString(3)),
                        new Language(reader.GetString(2)),
                        await series,
                        await chars,
                        reader.GetString(1),
                        await artists,
                        await tags,
                        await pages,
                        new Uri(reader.GetString(4), UriKind.Absolute)
                    ));
                }
            }

            return new DefaultPage<IMetadata>(builder.ToImmutable());
        }

        /// <inheritdoc />
        public override bool CanSetPageSize => true;

        public override void Dispose()
        {
            getPageCommand?.Dispose();
            base.Dispose();
        }
    }
}