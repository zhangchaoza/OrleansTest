using Orleans.Runtime.Configuration;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using GrainImplement;
using Orleans.ApplicationParts;
using Orleans.Configuration;
using System.IO;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Common;
using OrleansDashboard;
using MySiloHost.StartupTasks;

namespace MySiloHost
{
    class Program
    {
        static int Main(string[] args) => RunAsync().Result;

        static async Task<int> RunAsync()
        {
            await InitAsync();

            bool isError = false;
            try
            {
                var host = await StartSilo();
                Console.WriteLine("\nPress 已启动，任意键退出。");
                Console.ReadLine();

                await host.StopAsync();
                return 0;
            }
            catch (Exception ex)
            {
                isError = true;
                Console.WriteLine(ex);
                return 1;
            }
            finally
            {
                if (isError)
                {
                    Console.WriteLine("\nPress 已启动，任意键退出。");
                    Console.ReadLine();
                }
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            IConfiguration hostConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddIniFile(Path.Combine("init", "HostConfig.ini"), optional: false, reloadOnChange: false)
                .AddIniFile(Path.Combine("init", "DashboardConfig.ini"), optional: false, reloadOnChange: false)
                .Build();

            var builder = new SiloHostBuilder()
                .UseDashboard()
                // .UseDashboard(options =>
                // {
                //     options.Username = "zack";
                //     options.Password = "123";
                //     options.Host = "*";
                //     options.Port = 20020;
                //     options.HostSelf = true;
                // })
                // .UseLocalhostClustering()
                .UseConsulClustering(op =>
                {
                    op.Address = new Uri("http://127.0.0.1:8500");
                })
                .AddMemoryGrainStorage("DevStore")
                //.AddMemoryGrainStorageAsDefault()
                .UseInMemoryReminderService()
                .AddSimpleMessageStreamProvider("SMSProvider", op =>
                {
                    op.FireAndForgetDelivery = SimpleMessageStreamProviderOptions.DEFAULT_VALUE_FIRE_AND_FORGET_DELIVERY;
                    op.OptimizeForImmutableData = SimpleMessageStreamProviderOptions.DEFAULT_VALUE_OPTIMIZE_FOR_IMMUTABLE_DATA;
                    op.PubSubType = SimpleMessageStreamProviderOptions.DEFAULT_PUBSUB_TYPE;
                })
                // .AddPersistentStreams("SMSProvider_Persistent", (services, name) =>
                // {
                //     return null;
                // }, streamConfigurator =>
                // {

                // })
                .AddStartupTask<FirstStartupTask>()
                .AddMemoryGrainStorage("PubSubStore")
                .Configure<DashboardOptions>(hostConfig.GetSection("DashboardOptions"))
                .Configure<SiloOptions>(opt => opt.SiloName = Dns.GetHostName())
                .Configure<ClusterOptions>(hostConfig.GetSection("ClusterOptions"))
                .Configure<EndpointOptions>(hostConfig.GetSection("EndpointOptions"))
                .Configure<DevelopmentClusterMembershipOptions>(hostConfig.GetSection("DevelopmentClusterMembershipOptions"))
                .Configure<ConsulClusteringSiloOptions>(hostConfig.GetSection("ConsulClusteringSiloOptions"))
                // .ConfigureServices((ctx, ss) =>
                // {
                //     ss.Configure<DevelopmentClusterMembershipOptions>(hostConfig.GetSection("DevelopmentClusterMembershipOptions"));
                // })
                // .Configure<MemoryGrainStorageOptions>(hostConfig)
                .ConfigureApplicationParts(parts => parts
                    // .AddApplicationPart(typeof(HelloGrain).Assembly)
                    .AddFromApplicationBaseDirectory()
                    .WithCodeGeneration()
                    // .AddFromApplicationBaseDirectory()
                    //.WithReferences()
                    )
                .ConfigureLogging(logging => logging
                    .AddFilter("Orleans", LogLevel.Warning)
                    // .AddFilter("Orleans.Runtime.Management", LogLevel.Warning)
                    // .AddFilter("Orleans.Runtime.SiloControl", LogLevel.Warning)
                    // .AddFilter("Runtime", LogLevel.Warning)
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddConsole());

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }

        private static async Task InitAsync()
        {
            TypeDescriptor.AddAttributes(typeof(IPEndPoint), new TypeConverterAttribute(typeof(IdEndPointConverter)));
            TypeDescriptor.AddAttributes(typeof(IPAddress), new TypeConverterAttribute(typeof(IPAddressConverter)));
            await Task.CompletedTask;
        }

    }
}
