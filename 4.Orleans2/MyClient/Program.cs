using System;
using System.Threading.Tasks;
using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;

namespace MyClient
{
    public class Program
    {
        static int Main(string[] args) => RunMainAsync().Result;

        private static async Task<int> RunMainAsync()
        {
            try
            {
                using (var client = await StartClientWithRetries())
                {
                    await DoClientWork(client);

                    Console.WriteLine("\nPress 任意键退出。");
                    Console.ReadKey();
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
                return 1;
            }
        }

        private static async Task<IClusterClient> StartClientWithRetries(int initializeAttemptsBeforeFailing = -1)
        {
            int attempt = 0;
            IClusterClient client;
            while (true)
            {
                try
                {
                    var config = ClientConfiguration.LoadFromFile("ClientConfig.xml");
                    // var config = ClientConfiguration.LocalhostSilo();
                    client = new ClientBuilder()
                        .UseConfiguration(config)
                        .ConfigureApplicationParts(parts => parts
                            .AddFromAppDomain()
                            .WithReferences())
                        .ConfigureLogging(logging => logging
                            .AddFilter("Orleans", LogLevel.Information)
                            .SetMinimumLevel(LogLevel.Debug)
                            .AddConsole())
                        .Build();

                    await client.Connect();
                    Console.WriteLine("Client successfully connect to silo host");
                    break;
                }
                catch (SiloUnavailableException)
                {
                    attempt++;
                    Console.WriteLine($"Attempt {attempt} of {initializeAttemptsBeforeFailing} failed to initialize the Orleans client.");
                    if (initializeAttemptsBeforeFailing > 0 && (attempt > initializeAttemptsBeforeFailing))
                    {
                        throw;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(4));
                }
            }

            return client;
        }

        private static async Task DoClientWork(Orleans.IClusterClient client)
        {
            var friend = client.GetGrain<IHello>(0);
            while (true)
            {
                Console.WriteLine("输入消息(exit 退出)：");
                var message = Console.ReadLine();
                if (message == "exit")
                {
                    await friend.SayHello("bye!");
                    break;
                }
                var response = await friend.SayHello(message);
                Console.WriteLine("\n\n{0}\n\n", response);
            }
        }
    }
}
