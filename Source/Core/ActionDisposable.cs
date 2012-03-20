using System;

namespace CommandBasedComponents.Core
{
    public class ActionDisposable : IDisposable
    {
        readonly Action _action;

        public ActionDisposable(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            _action();
        }
    }
}