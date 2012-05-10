using System;
using System.ServiceProcess;
using Microsoft.Win32;

namespace CommandBasedComponents.Infrastructure
{
    public class WindowsServiceFacadeReal : IWindowsServiceFacade
    {
        readonly ServiceController _controller;

        public WindowsServiceFacadeReal(ServiceController controller)
        {
            _controller = controller;
        }

        public bool IsStarted
        {
            get { return _controller.Status == ServiceControllerStatus.Running; }
        }

        public bool IsStopped
        {
            get { return _controller.Status == ServiceControllerStatus.Stopped; }
        }

        public void StopAndWait(TimeSpan timeout)
        {
            _controller.Stop();
            _controller.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
        }

        public void Kill()
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services");
            if(key == null)
            {
                return;
            }
            key = key.OpenSubKey(_controller.ServiceName);
            if(key == null)
            {
                return;
            }
            var imagePath = (string)key.GetValue("ImagePath", null);

            // find process & kill
        }

        public void Start()
        {
            _controller.Start();
        }
    }
}