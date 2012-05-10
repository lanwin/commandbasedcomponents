using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace CommandBasedComponents.Infrastructure
{
    public class WindowsServiceFacadeReal : IWindowsServiceFacade
    {
        bool IWindowsServiceFacade.IsStarted(string name)
        {
            return GetService(name).Status == ServiceControllerStatus.Running;
        }

        bool IWindowsServiceFacade.IsStopped(string name)
        {
            return GetService(name).Status == ServiceControllerStatus.Stopped;
        }

        public bool Exists(string name)
        {
            return GetService(name) != null;
        }

        public Task<bool> Kill(string name)
        {
            return Task.Factory.StartNew(() =>
            {
                var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services");
                if(key == null)
                    return true;
                var controller = GetService(name);
                key = key.OpenSubKey(controller.ServiceName);
                if(key == null)
                    return true;
                var imagePath = (string)key.GetValue("ImagePath", null);
                // find process & kill
                return true;
            });
        }

        public Task<bool> Start(string name)
        {
            return Task.Factory.StartNew(() =>
            {
                var controller = GetService(name);
                controller.Start();
                controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(10));
                return controller.Status == ServiceControllerStatus.Running;
            });
        }

        public Task<bool> Stop(string name)
        {
            return Task.Factory.StartNew(() =>
            {
                var controller = GetService(name);
                controller.Stop();
                controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(10));
                return controller.Status == ServiceControllerStatus.Stopped;
            });
        }

        static ServiceController GetService(string name)
        {
            return ServiceController
                .GetServices()
                .FirstOrDefault(s => s.ServiceName == name);
        }
    }
}