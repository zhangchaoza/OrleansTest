using GrainInterfaces;
using Orleans;
using Orleans.Runtime.Configuration;
using System;

namespace Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Client start.");

            while (true)
            {
                Console.WriteLine("Your Id");
                if (!int.TryParse(Console.ReadLine(), out int id))
                {
                    continue;
                }

                // Then configure and connect a client.
                var clientConfig = ClientConfiguration.LoadFromFile("ClientConfiguration.xml");
                
                IClusterClient client = new ClientBuilder().UseConfiguration(clientConfig).Build();
                client.Connect().Wait();

                Console.WriteLine("Client connected.");

                Console.WriteLine("Write some msg.");
                var msg = Console.ReadLine();

                GrainClient.Initialize(clientConfig);
                var friend = GrainClient.GrainFactory.GetGrain<IHello>(id);
                var result = friend.SayHello(msg).Result;

                Console.WriteLine(result);
                client.Close();
            }

            Console.ReadKey();
        }
    }
}