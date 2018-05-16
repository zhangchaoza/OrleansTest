using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Hosting;
using Orleans.Configuration;
using System.IO;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using Common;
using OrleansDashboard;
using MySiloHost.StartupTasks;
using Orleans.EventSourcing;

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
                Console.WriteLine("\n已启动，任意键退出。");
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
                    Console.WriteLine("\n已启动，任意键退出。");
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
                .AddAdoNetGrainStorageAsDefault(op =>
                {
                    op.ConnectionString = "Data Source=10.0.113.10;Initial Catalog=OrleansStore_ZC;Persist Security Info=True;User ID=sa;Password=admin@2023";
                    op.UseJsonFormat = true;
                })
                .UseLocalhostClustering()
                .AddLogStorageBasedLogConsistencyProvider("LogStorage")
                .AddStateStorageBasedLogConsistencyProvider("StateStorage")
                .AddCustomStorageBasedLogConsistencyProvider("CustomStorage")
                // .AddMemoryGrainStorage("DevStore")
                // .AddMemoryGrainStorageAsDefault()
                .UseInMemoryReminderService()
                .AddStartupTask<GenFirstChangeTask>()
                .AddStartupTask<GetTopChangeTask>(40000)
                .Configure<DashboardOptions>(hostConfig.GetSection("DashboardOptions"))
                .Configure<SiloOptions>(opt => opt.SiloName = Dns.GetHostName())
                .Configure<ClusterOptions>(hostConfig.GetSection("ClusterOptions"))
                .Configure<EndpointOptions>(hostConfig.GetSection("EndpointOptions"))
                .Configure<DevelopmentClusterMembershipOptions>(hostConfig.GetSection("DevelopmentClusterMembershipOptions"))
                // .ConfigureServices((ctx, ss) =>
                // {
                //     ss.Configure<DevelopmentClusterMembershipOptions>(hostConfig.GetSection("DevelopmentClusterMembershipOptions"));
                // })
                .ConfigureApplicationParts(parts => parts
                    // .AddApplicationPart(typeof(HelloGrain).Assembly)
                    .AddFromApplicationBaseDirectory()
                    .WithCodeGeneration()
                    // .AddFromApplicationBaseDirectory()
                    //.WithReferences()
                    )
                .ConfigureLogging(logging => logging
                    .AddFilter("Orleans", LogLevel.Error)
                    .AddFilter("Orleans.Runtime.Management", LogLevel.Error)
                    .AddFilter("Orleans.Runtime.SiloControl", LogLevel.Error)
                    .AddFilter("Runtime", LogLevel.Error)
                    .AddFilter("GrainImplement.LogStorageBasedEventGrain", LogLevel.None)
                    .AddFilter("GrainImplement.StateStorageBasedEventGrain", LogLevel.None)
                    .AddFilter("GrainImplement.CustomStorageBasedEventGrain", LogLevel.Information)
                    .AddFilter("MySiloHost.StartupTasks.GetTopChangeTask", LogLevel.Information)
                    .SetMinimumLevel(LogLevel.None)
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
