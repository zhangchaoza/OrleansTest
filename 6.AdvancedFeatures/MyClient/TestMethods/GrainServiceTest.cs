namespace MyClient.TestMethods
{
    using GrainInterfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using System.Threading.Tasks;

    internal static class GrainServiceTest
    {
        private static IClusterClient _client;
        private static ILogger _logger;

        public static Task Run(IClusterClient client)
        {
            _client = client;
            _logger = _client.ServiceProvider.GetRequiredService<ILogger<Program>>();

            return Task.Run(async () =>
            {
                await Run1();
            });
        }

        private static async Task Run1()
        {
            var testGrain = _client.GetGrain<IGrainServiceTestGrain>(1);
            await testGrain.Call();
        }
    }
}