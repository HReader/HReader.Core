using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HReader.Base;
using HReader.Base.Data;

namespace HReader.Core.Sources
{
    public class DefaultSourceManager : ISourceManager
    {
        private readonly ObservableCollection<IMetadataSource> metadata;
        private readonly ObservableCollection<IContentSource> content;
        private readonly HashSet<Assembly> initializedAssemblies = new HashSet<Assembly>();
        private FileSystemWatcher watcher;

        public DefaultSourceManager(string sourcesDirectory,string nativeDataDirectory, ISynchronizeInvoke synchronizationProvider)
        {
            if (sourcesDirectory is null)
                throw new ArgumentNullException(nameof(sourcesDirectory));
            if (string.IsNullOrWhiteSpace(sourcesDirectory))
                throw new ArgumentException("Value cannot be empty or whitespace.", nameof(sourcesDirectory));

            content = new ObservableCollection<IContentSource>();
            metadata = new ObservableCollection<IMetadataSource>();

            Content = new ReadOnlyObservableCollection<IContentSource>(content);
            Metadata = new ReadOnlyObservableCollection<IMetadataSource>(metadata);
            
            InitializeData(sourcesDirectory, nativeDataDirectory);
            InitializeWatcher(sourcesDirectory, synchronizationProvider);
        }

        private void InitializeData(string directory, string nativeDataDirectory)
        {
            // load builtins
            content.Add(new Builtin.Native.ContentSource(nativeDataDirectory));
            content.Add(new Builtin.Web.ContentSource());

            // ensure the directory exists
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                return;
            }
            
            foreach (var file in Directory.EnumerateFiles(directory, "*.dll"))
            {
                AttemptAssemblyLoad(file);
            }  
        }

        private void InitializeWatcher(string directory, ISynchronizeInvoke synchronizationProvider)
        {
            watcher = new FileSystemWatcher(directory, "*.dll");
            watcher.BeginInit();
            watcher.SynchronizingObject = synchronizationProvider;
            watcher.Created += OnFileCreated;
            watcher.EnableRaisingEvents = true;
            watcher.EndInit();
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if(File.Exists(e.FullPath))
            {
                AttemptAssemblyLoad(e.FullPath);
            }
        }
        
        private void AttemptAssemblyLoad(string fullPath)
        {
            Assembly asm;
            try
            {
                asm = Assembly.LoadFile(fullPath);
            }
            catch (Exception)
            {
                return;
            }

            if (initializedAssemblies.Contains(asm)) return;
            initializedAssemblies.Add(asm);

            LoadMetadataSources(asm);
            LoadContentSources(asm);
        }

        private static IEnumerable<TInterface> CreateFrom<TInterface>(Assembly asm)
        {
            return asm.GetExportedTypes()
                      .Where(t => typeof(TInterface).IsAssignableFrom(t))
                      .Where(t => (t.GetConstructor(new Type[0])?.GetParameters().Length ?? -1) == 0)
                      .Select(t => (TInterface) Activator.CreateInstance(t));
        }

        private void LoadMetadataSources(Assembly asm)
        {
            foreach (var source in CreateFrom<IMetadataSource>(asm))
            {
                metadata.Add(source);
            }
        }

        private void LoadContentSources(Assembly asm)
        {
            foreach (var source in CreateFrom<IContentSource>(asm))
            {
                content.Add(source);
            }
        }

        public ReadOnlyObservableCollection<IMetadataSource> Metadata { get; }
        public ReadOnlyObservableCollection<IContentSource> Content { get; }

        /// <inheritdoc />
        public async Task<IMetadata> ResolveMetadataAsync(Uri uri)
        {
            foreach (var source in Metadata)
            {
                if (source.CanHandle(uri))
                {
                    return await source.HandleAsync(uri);
                }
            }
            throw new InvalidOperationException("Metadata could not be resolved.");
        }

        /// <inheritdoc />
        public async Task<bool> ConsumeAsync(Uri uri, Func<Stream, Task> consumer)
        {
            foreach (var source in Content)
            {
                if (source.CanHandle(uri))
                {
                    await source.HandleAsync(uri, consumer);
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            watcher.Created -= OnFileCreated;
            watcher?.Dispose();
        }
    }
}
