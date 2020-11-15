using System;
using System.Threading;

namespace BookieBaher.LogProcessor
{
    class Program
    {
        static readonly AutoResetEvent waitHandle = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            try
            {
                Service service = new Service();
                service.Start();

                Console.WriteLine($"{Service.Name} is running.. Press any to close");

                // Handle Control+C or Control+Break
                Console.CancelKeyPress += (o, e) =>
                {
                    Console.WriteLine("Exit");
                    waitHandle.Set();
                };

                // Wait
                waitHandle.WaitOne();

                service.Stop();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR - {ex.Message}");
                Environment.Exit(-1);
            }
        }
    }
}
