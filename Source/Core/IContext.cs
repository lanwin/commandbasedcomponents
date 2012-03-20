using System;

namespace CommandBasedComponents.Core
{
    public interface IContext : IDisposable
    {
        T Get<T>(IContextReader<T> reader);
        void Put<T>(IContextWriter<T> writer, T value, CleanupStrategy cleanup = CleanupStrategy.Dispose);
        void Remove<T>(IContextWriter<T> writer);
        bool HasValue(object key);
        bool TryGet<T>(IContextReader<T> reader, out T value);
    }
}