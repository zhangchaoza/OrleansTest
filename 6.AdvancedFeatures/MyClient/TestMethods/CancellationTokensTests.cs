namespace MyClient.TestMethods
{
    using GrainInterfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using System;
    using System.Threading.Tasks;

    internal static class CancellationTokensTests
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
            var tcs = new GrainCancellationTokenSource();
            var testGrain = _client.GetGrain<ICancellationTokensGrain>(1);

            await Task.WhenAny(
                testGrain.LongIoWork(tcs.Token, TimeSpan.FromSeconds(5)),
                Task.Delay(TimeSpan.FromSeconds(30)).ContinueWith(t => tcs.Cancel())
            );
            tcs.Dispose();
        }

    }
}