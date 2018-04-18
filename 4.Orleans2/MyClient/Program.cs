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
using System.Diagnostics;

namespace MyClient
{
    public class Program
    {
        static readonly Random random = new Random();

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
#if DEBUG

                throw e;
#else

                Console.WriteLine(e);
                Console.ReadLine();
                return 1;
#endif
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
                    // var b = aaa.CanConvertFrom(typeof(string));
                    // var c = (Uri)aaa.ConvertFrom("gwy.tcp://localhost:40000/0");
                    // var c2 = (Uri)aaa.ConvertFrom("gwy.tcp://10.0.113.30:40000/0");

                    client = new ClientBuilder()
                        // .UseLocalhostClustering(40000)
                        .UseStaticClustering()
                        // .UseStaticClustering(options =>
                        // {
                        //    var biubhi=options.Gateways;
                        // })
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
                catch (SiloUnavailableException ex)
                {
                    attempt++;
                    Console.WriteLine($"Attempt {attempt} of {initializeAttemptsBeforeFailing} failed to initialize the Orleans client.");
                    if (initializeAttemptsBeforeFailing > 0 && (attempt > initializeAttemptsBeforeFailing))
                    {
                        throw ex;
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

                var caculator = client.GetGrain<ICaculator>(Guid.Empty);
                var num = await caculator.Add(random.Next(10000), random.Next(10000));
                var response = await friend.SayHello($"{message},{num}");
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
