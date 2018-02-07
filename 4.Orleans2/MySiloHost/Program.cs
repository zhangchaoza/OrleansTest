using Orleans.Runtime.Configuration;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using GrainImplement;
using Orleans.ApplicationParts;

namespace MySiloHost
{
    class Program
    {
        static int Main(string[] args) => RunAsync().Result;

        static async Task<int> RunAsync()
        {
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
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            var config = new ClusterConfiguration();
            config.LoadFromFile("HostConfig.xml");

            // define the cluster configuration
            // var config = ClusterConfiguration.LocalhostPrimarySilo();
            config.AddMemoryStorageProvider();

            var builder = new SiloHostBuilder()
                .UseConfiguration(config)
                .ConfigureApplicationParts(parts => parts
                    // .AddApplicationPart(typeof(HelloGrain).Assembly)
                    .AddFromApplicationBaseDirectory()
                    .WithCodeGeneration()
                    // .AddFromApplicationBaseDirectory()
                    //.WithReferences()
                    )
                .ConfigureLogging(logging => logging
                    .AddFilter("Orleans", LogLevel.Information)
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddConsole());

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }

    }
}
