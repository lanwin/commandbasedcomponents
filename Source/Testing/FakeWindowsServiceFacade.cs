using System;
using CommandBasedComponents.Infrastructure;

namespace CommandBasedComponents.Testing
{
    public class FakeWindowsServiceFacade : IWindowsServiceFacade
    {
        readonly string _name;

        public FakeWindowsServiceFacade(string name)
        {
            _name = name;
            IsStopped = true;
        }

        public bool IsStarted { get; set; }
        public bool IsStopped { get; set; }

        public void StopAndWait(TimeSpan timeout)
        {
            IsStopped = true;
            IsStarted = false;
            Console.WriteLine("Service " + _name + " stopped");
        }

        public void Kill()
        {
            Console.WriteLine("Service " + _name + " killed");
        }

        public void Start()
        {
            IsStarted = true;
            IsStopped = false;
            Console.WriteLine("Service " + _name + " started");
        }
    }
}