using System;
using CommandBasedComponents.Core;
using CommandBasedComponents.Infrastructure;

namespace CommandBasedComponents.Testing
{
    public class FakeWindowsServiceLocator : IWindowsServiceLocator
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
            return new FakeWindowsServiceFacade(name);
        }
    }
}