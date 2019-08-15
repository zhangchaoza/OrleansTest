namespace MyClient.TestMethods
{
    using GrainInterfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// RequestContext is thread-static
    /// </summary>
    internal static class CallingTransactionsTests
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
            IATMGrain atm = _client.GetGrain<IATMGrain>(0);
            Guid from = Guid.Parse("{aaaf5c35-c257-4ac1-96eb-796a5a84ba06}");
            Guid to = Guid.Parse("{a57239f5-3bfc-4065-b4b2-b10e90a36b84}");
            await atm.Transfer(from, to, 100);
            uint fromBalance = await _client.GetGrain<IAccountGrain>(from).GetBalance();
            uint toBalance = await _client.GetGrain<IAccountGrain>(to).GetBalance();
            _logger.LogInformation("fromBalance:{0},toBalance{1}", fromBalance, toBalance);
        }

    }
}