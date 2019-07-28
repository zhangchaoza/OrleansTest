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
using Orleans.Concurrency;
using MyClient.TestMethods;

namespace MyClient
{
    public class Program
    {
        static readonly Random random = new Random();
        private static IList<StreamSubscriptionHandle<int>> subscriptionHandle;

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
                    subscriptionHandle = await SubscribeStream(client);

                    try
                    {
                        await RequestContextTests.Run(client);
                        await ExternalTasksTests.Run(client);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    }

                    await Task.WhenAll(subscriptionHandle.Select(h => h.UnsubscribeAsync()));
                    await client.Close();
                }

                // Console.WriteLine("\nPress 任意键退出。");
                // Console.ReadKey();
                return 0;
            }
            catch (Exception)
            {
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
                        // .UseLocalhostClustering(gatewayPort: 30000)
                        .UseStaticClustering()
                        // .UseConsulClustering(op =>
                        // {
                        //     op.Address = new Uri("http://127.0.0.1:8500");
                        // })
                        // .UseStaticClustering(options =>
                        // {
                        //    var biubhi=options.Gateways;
                        // })
                        .AddClusterConnectionLostHandler(OnLost)
                        .AddSimpleMessageStreamProvider("SMSProvider")
                        .Configure<ClusterOptions>(clientConfig.GetSection("ClusterOptions"))// 配置cluster属性
                        .Configure<StaticGatewayListProviderOptions>(servicesConfig.GetSection("StaticGatewayListProviderOptions"))// 配置silo gateway
                        .Configure<ConsulClusteringClientOptions>(servicesConfig.GetSection("ConsulClusteringClientOptions"))// 配置consul
                        .ConfigureApplicationParts(parts => parts
                            .AddFromAppDomain()
                            .WithReferences())
                        .ConfigureLogging(logging => logging
                            .AddFilter("Orleans", LogLevel.Warning)
                            .AddFilter("Orleans.Runtime.Management", LogLevel.Warning)
                            .AddFilter("Orleans.Runtime.SiloControl", LogLevel.Warning)
                            .AddFilter("Runtime", LogLevel.Warning)
                            .SetMinimumLevel(LogLevel.None)
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
            var subscriptionHandle = await stream.SubscribeAsync<int>((data, token) =>
            {
                Console.WriteLine($"SMSProvider-RANDOMDATA-RECEIVED:{data}");
                return Task.CompletedTask;
            });

            var shs = await stream.GetAllSubscriptionHandles();
            // foreach (var sh in shs)
            // {
            //     sh.ResumeAsync(async (data, token) => await Task.Run(() => Console.WriteLine($"stream:{data}")));
            // }
            return shs;
        }

        private static void OnLost(object sender, EventArgs e)
        {
            Console.WriteLine("已断开");
            // Task.WhenAll(subscriptionHandle.Select(h => h.UnsubscribeAsync()));
            // subscriptionHandle = SubscribeStream((IClusterClient)sender).Result;
        }

        private static async Task InitAsync()
        {
            TypeDescriptor.AddAttributes(typeof(IPEndPoint), new TypeConverterAttribute(typeof(IdEndPointConverter)));
            await Task.CompletedTask;
        }
    }
}
