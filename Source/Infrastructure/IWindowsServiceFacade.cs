using System;

namespace CommandBasedComponents.Infrastructure
{
    public interface IWindowsServiceFacade
    {
        bool IsStarted { get; }
        bool IsStopped { get; }
        void StopAndWait(TimeSpan timeout);
        void Kill();
        void Start();
    }
}