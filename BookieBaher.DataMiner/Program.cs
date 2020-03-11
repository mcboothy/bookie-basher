using System;

namespace BookieBaher.DataMiner
{
    class Program
    {
        static void Main(string[] args)
        {
            DataMinerService service = new DataMinerService();
            service.Start();
            Console.WriteLine("Task Dispatcher Running.. Press any to close");
            Console.ReadKey();
            service.Stop();
            Environment.Exit(0);
        }
    }
}
