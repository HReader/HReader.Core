using System;
using System.Collections.Generic;

namespace HReader.Core.Pagination
{
    /// <summary>
    /// Wraps a list of items that may be disposable.
    /// <see cref="IPage{T}"/> instances should dispose contained disposable items when they are disposed.
    /// </summary>
    /// <typeparam name="T">The type of items</typeparam>
    public interface IPage<out T> : IReadOnlyList<T>, IDisposable { }
}