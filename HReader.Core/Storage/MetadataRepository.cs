using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HReader.Base.Data;
using HReader.Core.Storage.Queries;
using HReader.Core.Pagination;
using Microsoft.Data.Sqlite;

namespace HReader.Core.Storage
{
    public class MetadataRepository : IMetadataRepository
    {
        public MetadataRepository(string dbPath)
        {
            isNew = !File.Exists(dbPath);
            Sql = new SqliteConnection(new SqliteConnectionStringBuilder()
            {
                BrowsableConnectionString = false,
                Mode =  SqliteOpenMode.ReadWriteCreate,
                DataSource = dbPath,
            }.ConnectionString);
        }

        public async Task InitializeAsync()
        {
            await Sql.OpenAsync();
            if (isNew)
            {
                await CreateDatabaseAsync();
            }
        }

        private async Task CreateDatabaseAsync()
        {
            using (var cmd = Sql.CreateCommand())
            {
                cmd.CommandText = QueryManager.Instance.Schema;
                await cmd.ExecuteNonQueryAsync();
            }
            isNew = false;
        }

        private bool isNew;

        public async Task<IPagination<IPage<IMetadata>>> GetAllEntriesAsync()
        {
            var entries = new AllEntriesPagination(this);
            await entries.InitializeAsync();
            return entries;
        }
        
        /// <inheritdoc />
        public Task<IPagination<IPage<IMetadata>>> SearchAsync(ISearchQuery query)
        {
            // This cannot be implemented before search parameters have been specified
            throw new NotImplementedException(); 
        }

        /// <inheritdoc />
        public async Task AddAsync(IMetadata entry)
        {
            using (var tr = Sql.BeginTransaction())
            {
                var id = await InsertEntry(entry);

                var pages = InsertPages(entry.Pages, id);
                var artists = InsertArtists(entry.Artists, id);
                var characters = InsertCharacters(entry.Characters, id);
                var series =  InsertSeries(entry.Series, id);
                var tags = InsertTags(entry.Tags, id);

                await Task.WhenAll(pages, artists, characters, series, tags);

                tr.Commit();
            }
        }

        private async Task InsertArtists(IReadOnlyList<Artist> artists, long id)
        {
            using (var insertArtist = Sql.CreateCommand(QueryManager.Instance.InsertArtist))
            using (var insertEntry = Sql.CreateCommand(QueryManager.Instance.InsertArtistEntry)
                                        .WithParameter("$entry", id))
            {
                var name = insertArtist.CreateParameter("$value");
                var artistId = insertEntry.CreateParameter("$artist");

                foreach (var artist in artists)
                {
                    name.Value = artist.Value;
                    artistId.Value = await insertArtist.ExecuteScalarAsync();
                    await insertEntry.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task InsertCharacters(IReadOnlyList<Character> characters, long id)
        {
            using (var insert = Sql.CreateCommand(QueryManager.Instance.InsertCharacter))
            using (var insertEntry = Sql.CreateCommand(QueryManager.Instance.InsertCharacterEntry)
                                        .WithParameter("$entry", id))
            {
                var name = insert.CreateParameter("$value");
                var characterId = insertEntry.CreateParameter("$character");

                foreach (var character in characters)
                {
                    name.Value = character.Value;
                    characterId.Value = await insert.ExecuteScalarAsync();
                    await insertEntry.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task InsertSeries(IReadOnlyList<Series> series, long id)
        {
            using (var insert = Sql.CreateCommand(QueryManager.Instance.InsertSeries))
            using (var insertEntry = Sql.CreateCommand(QueryManager.Instance.InsertSeriesEntry)
                                        .WithParameter("$entry", id))
            {
                var name = insert.CreateParameter("$value");
                var insertId = insertEntry.CreateParameter("$series");

                foreach (var se in series)
                {
                    name.Value = se.Value;
                    insertId.Value = await insert.ExecuteScalarAsync();
                    await insertEntry.ExecuteNonQueryAsync();
                }
            }
        }
        private async Task InsertTags(IReadOnlyList<Tag> tags, long id)
        {
            using (var insert = Sql.CreateCommand(QueryManager.Instance.InsertTag))
            using (var insertEntry = Sql.CreateCommand(QueryManager.Instance.InsertTagEntry)
                                        .WithParameter("$entry", id))
            {
                var name = insert.CreateParameter("$value");
                var tagId = insertEntry.CreateParameter("$tag");

                foreach (var tag in tags)
                {
                    name.Value = tag.Value;
                    tagId.Value = await insert.ExecuteScalarAsync();
                    await insertEntry.ExecuteNonQueryAsync();
                }
            }
        }
        
        private Task<long> InsertEntry(IMetadata entry)
        {
            return Sql.CreateCommand(QueryManager.Instance.InsertEntry)
                      .WithParameter("$title", entry.Title)
                      .WithParameter("$lang", entry.Language.Value)
                      .WithParameter("$kind", entry.Kind.Value)
                      .WithParameter("$cover", entry.Cover.AbsoluteUri)
                      .ConsumeScalarAsync<long>();
        }

        private async Task InsertPages(IReadOnlyList<Uri> pages, long entry)
        {
            using (var cmd = Sql.CreateCommand(QueryManager.Instance.InsertPage)
                                .WithParameter("$entry", entry))
            {
                var uri = cmd.CreateParameter("$uri");
                var idx = cmd.CreateParameter("$idx");

                for (var i = 0; i < pages.Count; i++)
                {
                    idx.Value = i;
                    uri.Value = pages[i].AbsoluteUri;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <inheritdoc />
        public async Task RemoveAsync(IMetadata entry)
        {
            if (!(entry is KeyedMetadata km))
                throw new NotSupportedException("Only Metadata returned by this repository can be removed from it.");

            await Sql.CreateCommand(QueryManager.Instance.DeleteEntry)
                     .WithParameter("$entry", km.Key)
                     .ConsumeNonQueryAsync();
        }

        /// <inheritdoc />
        public async Task RemoveAsync(IEnumerable<IMetadata> entry)
        {
            List<KeyedMetadata> entries;
            try
            {
                entries = entry.Cast<KeyedMetadata>().ToList();
            }
            catch (InvalidCastException e)
            {
                throw new NotSupportedException("Only Metadata returned by this repository can be removed from it.", e);
            }

            var list = Enumerable.Range(0, entries.Count - 1).Select(i => "$e" + i.ToString()).ToList();
            var query = string.Format(QueryManager.Instance.DeleteEntries, string.Join(", ", list));

            using (var cmd = Sql.CreateCommand(query))
            {
                for (var i = 0; i < entries.Count; i++)
                {
                    cmd.WithParameter(list[i], entries[i].Key);
                }
                await cmd.ConsumeNonQueryAsync();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Sql?.Dispose();
        }

        internal SqliteConnection Sql { get; }


        protected virtual void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(value, field)) return;
            field = value;
            NotifyOfPropertyChange(propertyName);
        }

        protected virtual void NotifyOfPropertyChange([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
