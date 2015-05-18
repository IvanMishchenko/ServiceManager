using System;
using System.Threading.Tasks;

namespace ServicesManager
{
    class Program
    {
        private static readonly string[] MachineNames = new[] { "localhost", "localhost1", "localhost2" };
        private static readonly string[] ServiceName = new[] { "MySQL56", "Fax", "W32Time" };
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        private static async Task MainAsync(string[] args)
        {
            Console.WriteLine("1 -> Run");
            Console.WriteLine("2 -> Stop");
            bool isNeedToRun = Console.ReadLine() == "1";

            var logger = new ServiceLogger();

            var serviceManager = new ServiceManager(MachineNames, ServiceName, logger);
            serviceManager.ProccessMachines(isNeedToRun).Wait();

            logger.ToConsole();

            Console.ReadLine();
        }
    }
}
