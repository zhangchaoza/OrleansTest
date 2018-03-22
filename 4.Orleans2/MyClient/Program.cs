using Common;
using GrainInterfaces;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using System.IO;

namespace MyClient
{
    public class Program
    {
        static int Main(string[] args) => RunMainAsync().Result;

        private static async Task<int> RunMainAsync()
        {
            await InitAsync();

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
                    IConfiguration clientConfig = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddIniFile("init\\ClientConfig.ini", optional: false, reloadOnChange: false)
                        .Build();

                    IConfiguration servicesConfig = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddIniFile("init\\ServicesConfig.ini", optional: false, reloadOnChange: false)
                        .Build();

                    // var ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 40000);
                    // var gateUri = ip.ToGatewayUri();
                    // var a = ConfigurationBinder.Get<StaticGatewayListProviderOptions>(servicesConfig.GetSection("StaticGatewayListProviderOptions"));
                    // var aaa = TypeDescriptor.GetConverter(typeof(Uri));
                    // var u = aaa.ConvertTo(gateUri, typeof(string));

                    client = new ClientBuilder()
                        .UseStaticClustering()
                        .Configure<ClusterOptions>(clientConfig.GetSection("ClusterOptions"))
                        .Configure<StaticGatewayListProviderOptions>(servicesConfig.GetSection("StaticGatewayListProviderOptions"))
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

        private static async Task InitAsync()
        {
            TypeDescriptor.AddAttributes(typeof(IPEndPoint), new TypeConverterAttribute(typeof(IdEndPointConverter)));
            await Task.CompletedTask;
        }
    }
}
