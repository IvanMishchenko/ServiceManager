using System;
using System.Collections.Generic;

namespace ServicesManager
{
    class ServiceLogger
    {
        private readonly Dictionary<string, Dictionary<string, string>> _statuses;

        public ServiceLogger()
        {
            _statuses = new Dictionary<string, Dictionary<string, string>>();
        }

        public void Log(string machineName, string serviceName, string result)
        {
            if (_statuses.ContainsKey(machineName))
                _statuses[machineName].Add(serviceName, result);
            else
                _statuses.Add(machineName, new Dictionary<string, string>
                {
                    {serviceName, result}
                });
        }

        public void ToConsole()
        {
            foreach (var status in _statuses)
            {
                Console.WriteLine("Status for machine \"{0}\"", status.Key);

                foreach (var serviceStatus in status.Value)
                {
                    Console.WriteLine("\t {0} - {1} ", serviceStatus.Key, serviceStatus.Value);
                }
            }
        }
    }
}
