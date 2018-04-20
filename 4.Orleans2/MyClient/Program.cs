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
using Orleans.Streams;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace MyClient
{
    public class Program
    {
        static readonly Random random = new Random();

        static async Task<int> Main(string[] args)
        {
            await InitAsync();

            while (true)
            {
                if (RunMainAsync().Result == 0)
                    break;
                await Task.Delay(4000);
            }
            return 0;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                using (var client = await StartClientWithRetries())
                {
                    var subscriptionHandle = await SubscribeStream(client);
                    await DoClientWorkREPL(client);
                    // await DoClientWorkSimple(client);
                    await Task.Delay(5000);
                    await Task.WhenAll(subscriptionHandle.Select(h => h.UnsubscribeAsync()));
                    await client.Close();
                }

                // Console.WriteLine("\nPress 任意键退出。");
                // Console.ReadKey();
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
                        .AddIniFile(Path.Combine("init", "ClientConfig.ini"), optional: false, reloadOnChange: false)
                        .Build();

                    IConfiguration servicesConfig = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddIniFile(Path.Combine("init", "ServicesConfig.ini"), optional: false, reloadOnChange: false)
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
                        .AddClusterConnectionLostHandler((s, e) => Console.WriteLine("已断开"))
                        .AddSimpleMessageStreamProvider("SMSProvider")
                        .Configure<ClusterOptions>(clientConfig.GetSection("ClusterOptions"))
                        .Configure<StaticGatewayListProviderOptions>(servicesConfig.GetSection("StaticGatewayListProviderOptions"))
                        .ConfigureApplicationParts(parts => parts
                            .AddFromAppDomain()
                            .WithReferences())
                        .ConfigureLogging(logging => logging
                            .AddFilter("Orleans", LogLevel.Warning)
                            .AddFilter("Orleans.Runtime.Management", LogLevel.Warning)
                            .AddFilter("Orleans.Runtime.SiloControl", LogLevel.Warning)
                            .AddFilter("Runtime", LogLevel.Warning)
                            .SetMinimumLevel(LogLevel.Debug)
                            .AddConsole())
                        .Build();

                    await client.Connect();

                    Console.WriteLine("Client successfully connect to silo host");
                    break;
                }
                catch (OrleansException ex)
                {
                    attempt++;
                    Console.WriteLine($"重试 {attempt} of {initializeAttemptsBeforeFailing} 链接失败.");
                    if (initializeAttemptsBeforeFailing > 0 && (attempt > initializeAttemptsBeforeFailing))
                    {
                        throw ex;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return client;
        }

        private static async Task<IList<StreamSubscriptionHandle<int>>> SubscribeStream(IClusterClient client)
        {
            var streamProvider = client.GetStreamProvider("SMSProvider");
            var stream = streamProvider.GetStream<int>(Guid.Empty, "RANDOMDATA");
            var subscriptionHandle = await stream.SubscribeAsync<int>(
                    async (data, token) =>
                    {
                        await Task.Run(() => Console.WriteLine($"stream:{data}"));
                    });

            var shs = await stream.GetAllSubscriptionHandles();
            // foreach (var sh in shs)
            // {
            //     sh.ResumeAsync(async (data, token) => await Task.Run(() => Console.WriteLine($"stream:{data}")));
            // }
            return shs;
        }

        /// <summary>
        /// Read–Eval–Print Loop
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private static async Task DoClientWorkREPL(IClusterClient client)
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

        private static async Task DoClientWorkSimple(IClusterClient client)
        {
            var friend = client.GetGrain<IHello>(0);
            var caculator = client.GetGrain<ICaculator>(Guid.Empty);
            var num = await caculator.Add(random.Next(10000), random.Next(10000));
            var response = await friend.SayHello($"{num}");
            Console.WriteLine("\n\n{0}\n\n", response);
        }

        private static async Task InitAsync()
        {
            TypeDescriptor.AddAttributes(typeof(IPEndPoint), new TypeConverterAttribute(typeof(IdEndPointConverter)));
            await Task.CompletedTask;
        }
    }
}
