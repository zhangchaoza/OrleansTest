namespace MyClient.TestMethods
{
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Orleans;
    using Orleans.Runtime;

    internal static class ExternalTasksTests
    {
        static IClusterClient _client;

        public static Task Run(IClusterClient client)
        {
            _client = client;
            return Task.Run(async () =>
            {
                await Run1();
                await Run2();
                await Run3();
            });
        }

        private static async Task Run1()
        {
            var testGrain = _client.GetGrain<IExternalTasksGrains>(1);
            await testGrain.RunExternalTask();
        }

        private static async Task Run2()
        {
            var testGrain = _client.GetGrain<IExternalTasksGrains>(1);
            await testGrain.RunExternalTask2();
        }

        private static async Task Run3()
        {
            var testGrain = _client.GetGrain<IExternalTasksGrains>(1);
            await testGrain.WaitGrainMethod();
        }

    }
}