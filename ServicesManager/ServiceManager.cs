using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace ServicesManager
{
    class ServiceManager
    {
        private readonly string[] _machineNames;
        private readonly string[] _serviceName;
        private readonly ServiceLogger _logger;

        public ServiceManager(string[] machineNames, string[] serviceName, ServiceLogger logger)
        {
            _machineNames = machineNames;
            _serviceName = serviceName;
            _logger = logger;
        }


        public async Task ProccessMachines(bool isNeedToStart)
        {
            Console.WriteLine("Started Proccessing");

            var allMachinesProccessors = _machineNames.Select(m => Task.Run(() => ProccessMachineAsync(m, isNeedToStart)));
            await Task.WhenAll(allMachinesProccessors);
        }

        private async Task ProccessMachineAsync(string machineName, bool isNeedToStart)
        {
            var services = GetServicesFromMachine(machineName);

            var tasks =
                    services.Select(
                        x => Task.Run(() =>
                            ProcessServiceAsync(x, isNeedToStart,
                                new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token)));

            await Task.WhenAll(tasks);
        }

        private IEnumerable<ServiceController> GetServicesFromMachine(string machineName)
        {
            Console.WriteLine("\tRetriving services from {0}", machineName);

            List<ServiceController> servicesList;

            try
            {
                servicesList = ServiceController.GetServices(machineName).Where(s => _serviceName.Any(n => string.Equals(n, s.ServiceName, StringComparison.InvariantCultureIgnoreCase))).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\tUnable to retrieve services from {0}", machineName);
                Console.WriteLine("Exception: {0}", ex);
                return new List<ServiceController>();
            }

            Console.WriteLine("\tRetrieved services from {0}", machineName);

            return servicesList;
        }

        private async Task ProcessServiceAsync(ServiceController service, bool isNeedToStart, CancellationToken token)
        {
            Console.WriteLine("\t\tTrying to {0} {1} service from {2}", isNeedToStart ? "start" : "stop", service.DisplayName, service.MachineName);

            try
            {
                if (isNeedToStart && service.Status != ServiceControllerStatus.Running)
                    service.Start();
                else if (service.Status != ServiceControllerStatus.Stopped) service.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\t\tUnable to {0} {1} service from {2}", isNeedToStart ? "start" : "stop", service.DisplayName, service.MachineName);
                Console.WriteLine("Exception: {0}", ex);
                _logger.Log(service.MachineName, service.ServiceName, "Failure");
                return;
            }

            while (true)
            {
                service.Refresh();

                if (token.IsCancellationRequested)
                {
                    if ((isNeedToStart && service.Status != ServiceControllerStatus.Running) ||
                        (!isNeedToStart && service.Status != ServiceControllerStatus.Stopped))
                    {
                        Console.WriteLine("\t\tUnable to {0} {1} service from {2}", isNeedToStart ? "start" : "stop", service.DisplayName, service.MachineName);
                        _logger.Log(service.MachineName, service.ServiceName, "Failure");
                        return;
                    }
                }
                else
                {
                    if ((isNeedToStart && service.Status == ServiceControllerStatus.Running) ||
                        (!isNeedToStart && service.Status == ServiceControllerStatus.Stopped))
                    {
                        Console.WriteLine("\t\t{0} service from {1} {2}", service.DisplayName, service.MachineName, isNeedToStart ? "started" : "stopped");
                        _logger.Log(service.MachineName, service.ServiceName, "Success");
                        return;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
        }
    }
}
