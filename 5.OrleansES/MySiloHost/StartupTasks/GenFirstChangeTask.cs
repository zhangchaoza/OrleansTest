namespace MySiloHost.StartupTasks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Orleans;
    using Orleans.Runtime;

    public class GenFirstChangeTask : IStartupTask
    {

        private readonly IGrainFactory grainFactory;

        public GenFirstChangeTask(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            var logEventGrain = grainFactory.GetGrain<ILogStorageBasedEventGrain>(0);
            // await logEventGrain.Update(null);
            await logEventGrain.Update(new Change
            {
                Name = "GenFirstChangeTask",
                Value = -1,
                When = DateTimeOffset.UtcNow
            });

            var stateEventGrain = grainFactory.GetGrain<IStateStorageBasedEventGrain>(0);
            // await stateEventGrain.Update(null);
            await stateEventGrain.Update(new Change
            {
                Name = "GenFirstChangeTask",
                Value = -1,
                When = DateTimeOffset.UtcNow
            });

            var customEventGrain = grainFactory.GetGrain<ICustomStorageBasedEventGrain>(0);
            // await stateEventGrain.Update(null);
            await customEventGrain.Update(new Change
            {
                Name = "GenFirstChangeTask",
                Value = -1,
                When = DateTimeOffset.UtcNow
            });
        }
    }
}