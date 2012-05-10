using System;
using CommandBasedComponents.Core;
using CommandBasedComponents.Testing;

namespace CommandBasedComponents.Infrastructure
{
    public class WindowsServiceLocatorReal : IWindowsServiceLocator
    {
        [NotNull]
        public FakeWindowsServiceFacade Find(string name)
        {
            var windowsService = TryFind(name);
            if(windowsService == null)
            {
                throw new InvalidOperationException("Service " + name + " is not installed");
            }
            return windowsService;
        }

        public FakeWindowsServiceFacade TryFind(string name)
        {
            throw new NotImplementedException();/*
            return ServiceController.GetServices()
                .Where(s => s.ServiceName.Equals(name, StringComparison.OrdinalIgnoreCase))
                .Select(s => new WindowsServiceFacade(s.n))
                .FirstOrDefault();*/
        }
    }
}