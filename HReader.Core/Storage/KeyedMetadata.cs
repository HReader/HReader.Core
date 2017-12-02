using System;
using System.Collections.Generic;
using HReader.Base.Data;

namespace HReader.Core.Storage
{
    internal class KeyedMetadata : DefaultMetadata
    {
        /// <inheritdoc />
        public KeyedMetadata(
            long key,
            Kind kind,
            Language language,
            IReadOnlyList<Series> series,
            IReadOnlyList<Character> characters,
            string title,
            IReadOnlyList<Artist> artists,
            IReadOnlyList<Tag> tags,
            IReadOnlyList<Uri> pages, Uri cover
        ) 
            : base(kind, language, series, characters, title, artists, tags, pages, cover)
        {
            Key = key;
        }
        
        public long Key { get; }
    }
}
