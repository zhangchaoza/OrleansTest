namespace MyClient.TestMethods
{
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Runtime;
    using Microsoft.Extensions.DependencyInjection;

    internal static class CallFilterTests
    {
        static IClusterClient _client;
        static ILogger _logger;

        public static Task Run(IClusterClient client)
        {
            _client = client;
            _logger = _client.ServiceProvider.GetRequiredService<ILogger<Program>>();

            return Task.Run(async () =>
            {
                await Run1();
                await Run2();
            });
        }

        private static async Task Run1()
        {
            var testGrain = _client.GetGrain<IGrainCallFiltersGrain>(1);
            var result = await testGrain.Call();
            _logger.LogInformation("result={0}", result);
        }

        private static async Task Run2()
        {
            var testGrain = _client.GetGrain<IPer_grain_GrainCallFiltersGrain>(1);
            var result = await testGrain.Call();
            _logger.LogInformation("result={0}", result);
        }

    }
}