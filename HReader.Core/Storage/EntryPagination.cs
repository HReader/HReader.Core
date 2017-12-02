using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using HReader.Base.Data;
using HReader.Core.Storage.Queries;
using HReader.Core.Pagination;
using Microsoft.Data.Sqlite;

namespace HReader.Core.Storage
{
    internal abstract class EntryPagination : PaginationBase<IPage<IMetadata>>
    {
        protected EntryPagination(MetadataRepository repository)
        {
            Repository = repository;

            SelectEntryArtists = repository.Sql.CreateCommand(QueryManager.Instance.SelectEntryArtists);
            SelectEntryArtistsEntry = SelectEntryArtists.CreateParameter("$entry");

            SelectEntryCharacters = repository.Sql.CreateCommand(QueryManager.Instance.SelectEntryCharacters);
            SelectEntryCharactersEntry = SelectEntryCharacters.CreateParameter("$entry");

            SelectEntrySeries = repository.Sql.CreateCommand(QueryManager.Instance.SelectEntrySeries);
            SelectEntrySeriesEntry = SelectEntrySeries.CreateParameter("$entry");

            SelectEntryTags = repository.Sql.CreateCommand(QueryManager.Instance.SelectEntryTags);
            SelectEntryTagsEntry = SelectEntryTags.CreateParameter("$entry");

            SelectEntryPages = repository.Sql.CreateCommand(QueryManager.Instance.SelectEntryPages);
            SelectEntryPagesEntry = SelectEntryPages.CreateParameter("$entry");
        }

        protected MetadataRepository Repository { get; private set; }

        private SqliteCommand SelectEntryArtists    { get; }
        private SqliteCommand SelectEntryCharacters { get; }
        private SqliteCommand SelectEntrySeries     { get; }
        private SqliteCommand SelectEntryTags       { get; }

        private SqliteCommand SelectEntryPages      { get; }

        private SqliteParameter SelectEntryArtistsEntry    { get; }
        private SqliteParameter SelectEntryCharactersEntry { get; }
        private SqliteParameter SelectEntrySeriesEntry     { get; }
        private SqliteParameter SelectEntryTagsEntry       { get; }

        private SqliteParameter SelectEntryPagesEntry      { get; }

        protected async Task<IReadOnlyList<Artist>> SelectArtistsAsync(long entry)
        {
            return await SelectForEntryAsync(
                SelectEntryArtists,
                SelectEntryArtistsEntry,
                entry,
                s => new Artist(s));
        }

        protected async Task<IReadOnlyList<Character>> SelectCharactersAsync(long entry)
        {
            return await SelectForEntryAsync(
                SelectEntryTags,
                SelectEntryTagsEntry,
                entry,
                s => new Character(s));
        }

        protected async Task<IReadOnlyList<Series>> SelectSeriesAsync(long entry)
        {
            return await SelectForEntryAsync(
                SelectEntrySeries,
                SelectEntrySeriesEntry,
                entry,
                s => new Series(s));
        }

        protected async Task<IReadOnlyList<Tag>> SelectTagsAsync(long entry)
        {
            return await SelectForEntryAsync(
                SelectEntryTags,
                SelectEntryTagsEntry,
                entry,
                s => new Tag(s));
        }

        protected async Task<IReadOnlyList<Uri>> SelectPagesAsync(long entry)
        {
            return await SelectForEntryAsync(
                SelectEntryPages,
                SelectEntryPagesEntry,
                entry,
                s => new Uri(s, UriKind.Absolute));
        }

        private static async Task<IReadOnlyList<T>> SelectForEntryAsync<T>(SqliteCommand cmd, SqliteParameter param, long entry, Func<string, T> factory)
        {
            param.Value = entry;
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (!reader.HasRows) return ImmutableList<T>.Empty;

                var builder = ImmutableList.CreateBuilder<T>();

                while (await reader.ReadAsync())
                {
                    builder.Add(factory(reader.GetString(0)));
                }

                return builder.ToImmutable();
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            SelectEntryArtists.Dispose();
            SelectEntryCharacters.Dispose();
            SelectEntrySeries.Dispose();
            SelectEntryTags.Dispose();
            Repository = null;
            base.Dispose();
        }
    }
}