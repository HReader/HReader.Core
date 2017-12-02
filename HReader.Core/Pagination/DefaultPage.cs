using System;
using System.Collections;
using System.Collections.Generic;

namespace HReader.Core.Pagination
{
    /// <summary>
    /// A dafault implementation of <see cref="IPage{T}"/> that simply wraps an <see cref="IReadOnlyList{T}"/> instance.
    /// The dispose method will dispose any contained items aswell.
    /// </summary>
    public class DefaultPage<T> : IPage<T>
    {
        private readonly IReadOnlyList<T> items;

        public DefaultPage(IReadOnlyList<T> items)
        {
            this.items = items;
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)items).GetEnumerator();
        }
        /// <inheritdoc />
        public int Count => items.Count;
        /// <inheritdoc />
        public T this[int index] => items[index];

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var item in items)
            {
                (item as IDisposable)?.Dispose();
            }
        }

    }
}