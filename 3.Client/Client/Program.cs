using GrainInterfaces;
using HelloGrains;
using Orleans;
using Orleans.Runtime.Configuration;
using System;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Client start.");

            //while (true)
            //{
            //    Console.WriteLine("Your Id");
            //    if (!int.TryParse(Console.ReadLine(), out int id))
            //    {
            //        continue;
            //    }

            //    Console.WriteLine("Repeat [y/n]");
            //    var y = Console.ReadLine();
            //    if (y == "y" || y == "Y")
            //    {
            //        for (int i = id; i < id + 10; i++)
            //        {
            //            test(id, $"hello {i}");
            //        }
            //    }
            //    else
            //    {
            //        test(id);
            //    }
            //}

            TaskTest().Wait();
        }

        private static void test(int id, string msg = null)
        {
            // Then configure and connect a client.
            //var clientConfig = ClientConfiguration.LoadFromFile("ClientConfiguration.xml");
            var clientConfig = ClientConfiguration.LocalhostSilo();

            IClusterClient client = new ClientBuilder()
                .UseConfiguration(clientConfig).Build();
            client.Connect().Wait();

            Console.WriteLine("Client connected.");

            if (string.IsNullOrEmpty(msg))
            {
                Console.WriteLine("Write some msg.");
                msg = Console.ReadLine();
            }

            GrainClient.Initialize(clientConfig);
            var friend = GrainClient.GrainFactory.GetGrain<IHello>(id);
            var result = friend.SayHello(msg).Result;

            Console.WriteLine(result);

            Chat c = new Chat();
            var obj = GrainClient.GrainFactory.CreateObjectReference<IChat>(c).Result;
            friend.Subscribe(obj);

            friend.SendUpdateMessage("456");

            client.Close();
        }

        private async static Task TaskTest()
        {
            var clientConfig = ClientConfiguration.LocalhostSilo();
            IClusterClient client = new ClientBuilder()
                .UseConfiguration(clientConfig).Build();
            await client.Connect();
            Console.WriteLine("Client connected.");

            await client.GetGrain<ITaskGrain>(0).MyGrainMethod();

            await client.Close();
        }
    }
}