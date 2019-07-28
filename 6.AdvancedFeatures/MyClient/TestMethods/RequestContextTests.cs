namespace MyClient.TestMethods
{
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Orleans;
    using Orleans.Runtime;

    /// <summary>
    /// RequestContext is thread-static
    /// </summary>
    internal static class RequestContextTests
    {
        static IClusterClient _client;

        public static Task Run(IClusterClient client)
        {
            _client = client;
            return Task.Run(async () =>
            {
                await Run1();
                await Run1_2();
                await Run2();
            });
        }

        private static async Task Run1()
        {
            var testGrain = _client.GetGrain<IRequestContextTestGrain>(1);
            RequestContext.Set("TraceId", "client DoTaskTest");
            await testGrain.DisplayRequestContext();
        }

        private static async Task Run1_2()
        {
            var testGrain = _client.GetGrain<IRequestContextTestGrain>(1);
            await testGrain.DisplayRequestContext();
        }

        private static async Task Run2()
        {
            var testGrain = _client.GetGrain<IRequestContextTestGrain>(2);
            await testGrain.DisplayRequestContext();
        }
    }
}