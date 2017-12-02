using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace HReader.Core.Storage.Queries
{
    internal partial class QueryManager
    {
        private static Lazy<string> ReadLazy(string name)
        {
            return new Lazy<string>(() => ReadString(name), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private static string ReadString(string name)
        {
            using (var stream = Assembly.GetManifestResourceStream("HReader.Core.Storage.Queries." + name))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
        
        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<QueryManager> instance = new Lazy<QueryManager>(() => new QueryManager(), LazyThreadSafetyMode.PublicationOnly);
        public static QueryManager Instance => instance.Value;

        private QueryManager() { }
    }
}
