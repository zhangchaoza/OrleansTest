using Common;
using GrainInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySiloHost.StartupTasks;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using OrleansDashboard;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MySiloHost
{
    internal class Program
    {
        private static int Main(string[] args) => RunAsync().Result;

        private static async Task<int> RunAsync()
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
                // .UseDashboard()
                // .UseDashboard(options =>
                // {
                //     options.Username = "zack";
                //     options.Password = "123";
                //     options.Host = "*";
                //     options.Port = 20020;
                //     options.HostSelf = true;
                // })
                .UseLocalhostClustering(siloPort: 111112, gatewayPort: 30000)
                // .UseConsulClustering(op =>
                // {
                //     op.Address = new Uri("http://127.0.0.1:8500");
                // })
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
                // .Configure<DashboardOptions>(hostConfig.GetSection("DashboardOptions"))
                .Configure<SiloOptions>(opt => opt.SiloName = Dns.GetHostName())// 配置silo属性
                .Configure<ClusterOptions>(hostConfig.GetSection("ClusterOptions"))// 配置cluster属性
                .Configure<EndpointOptions>(hostConfig.GetSection("EndpointOptions"))// 配置silo port、gateway
                .Configure<DevelopmentClusterMembershipOptions>(hostConfig.GetSection("DevelopmentClusterMembershipOptions"))// 配置membership
                .Configure<ConsulClusteringSiloOptions>(hostConfig.GetSection("ConsulClusteringSiloOptions")) // 配置consul

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
                    .AddConsole())

                // Silo-wide Grain Call Filters
                .AddIncomingGrainCallFilter(async context =>
                {
                    // If the method being called is 'MyInterceptedMethod', then set a value
                    // on the RequestContext which can then be read by other filters or the grain.
                    if (string.Equals(context.InterfaceMethod.Name, nameof(IGrainCallFiltersGrain.Call)))
                    {
                        RequestContext.Set("intercepted value", "this value was added by the filter");
                    }

                    await context.Invoke();

                    // If the grain method returned an int, set the result to double that value.
                    if (context.Result is int resultValue) context.Result = resultValue * 2;
                })
                // use DI setting Silo-wide Grain Call Filters
                // .ConfigureServices(services => services.AddSingleton<IIncomingGrainCallFilter, LoggingCallFilter>())
                ;

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