using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CommandBasedComponents.Core
{
    [DebuggerStepThrough]
    public class Context : IContext
    {
        readonly IContext _parent;
        readonly Dictionary<object, Value> _values = new Dictionary<object, Value>();

        public Context(IContext parent = null)
        {
            _parent = parent;
        }

        public static IContext Empty
        {
            get { return new Context(); }
        }

        public T Get<T>(IContextReader<T> reader)
        {
            if(reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            T value;
            if(!TryGet(reader, out value))
            {
                throw new InvalidOperationException("Dose not contains key");
            }

            return value;
        }

        public bool TryGet<T>(IContextReader<T> reader, out T value)
        {
            Value innerValue;
            if(_values.TryGetValue(reader, out innerValue))
            {
                value = (T)innerValue.Data;
                return true;
            }

            if(_parent != null && _parent.TryGet(reader, out value))
            {
                return true;
            }

            value = default( T );
            return false;
        }

        public void Remove<T>(IContextWriter<T> writer)
        {
            if(writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            if(_parent != null)
            {
                _parent.Remove(writer);
            }

            _values.Remove(writer);
        }

        public void Put<T>(IContextWriter<T> writer, T value, CleanupStrategy cleanup = CleanupStrategy.Dispose)
        {
            if(writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            if(_parent != null && cleanup != CleanupStrategy.Scope)
            {
                _parent.Put(writer, value, cleanup);
                return;
            }

            _values[writer] = new Value
            {
                Data = value,
                Cleanup = cleanup
            };
        }

        public bool HasValue(object key)
        {
            if(key == null)
            {
                throw new ArgumentNullException("key");
            }

            return _values.ContainsKey(key) || ( _parent != null && _parent.HasValue(key) );
        }

        public void Dispose()
        {
            foreach(var value in _values.ToArray())
            {
                if(value.Value.Cleanup != CleanupStrategy.None)
                {
                    var disposable = value.Value.Data as IDisposable;
                    if(disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
                _values.Remove(value);
            }
        }

        class Value
        {
            public object Data { get; set; }
            public CleanupStrategy Cleanup { get; set; }
        }
    }
}