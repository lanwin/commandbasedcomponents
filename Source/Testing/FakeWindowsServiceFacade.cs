using System;
using System.Threading.Tasks;
using CommandBasedComponents.Infrastructure;

namespace CommandBasedComponents.Testing
{
    public class FakeWindowsServiceFacade : IWindowsServiceFacade
    {
        public FakeWindowsServiceFacade()
        {
            IsStopped = true;
        }

        public bool IsStarted { get; set; }
        public bool IsStopped { get; set; }

        bool IWindowsServiceFacade.IsStarted(string name)
        {
            return IsStarted;
        }

        bool IWindowsServiceFacade.IsStopped(string name)
        {
            return IsStopped;
        }

        public bool Exists(string name)
        {
            return true;
        }

        public Task<bool> Kill(string name)
        {
            Console.WriteLine("Service " + name + " killed");
            var source = new TaskCompletionSource<bool>();
            source.SetResult(true);
            return source.Task;
        }

        public Task<bool> Start(string name)
        {
            IsStarted = true;
            IsStopped = false;
            Console.WriteLine("Service " + name + " started");
            var source = new TaskCompletionSource<bool>();
            source.SetResult(true);
            return source.Task;
        }

        public Task<bool> Stop(string name)
        {
            IsStopped = true;
            IsStarted = false;
            Console.WriteLine("Service " + name + " stopped");
            var source = new TaskCompletionSource<bool>();
            source.SetResult(true);
            return source.Task;
        }
    }
}