namespace MySiloHost.StartupTasks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Orleans;
    using Orleans.Runtime;

    public class GenChangeTask : IStartupTask
    {

        private readonly IGrainFactory grainFactory;

        public GenChangeTask(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {

#if LOG_BASED

            var logEventGrain = grainFactory.GetGrain<ILogStorageBasedEventGrain>(0);
            // await logEventGrain.Update(null);
           await GenChange( logEventGrain, 10);

#endif

#if STATE_BASED

            var stateEventGrain = grainFactory.GetGrain<IStateStorageBasedEventGrain>(0);
            //await stateEventGrain.Update(null);
            await GenChange( stateEventGrain, 10);

#endif

#if CUSTOM_BASED

            var customEventGrain = grainFactory.GetGrain<ICustomStorageBasedEventGrain>(0);
            //await customEventGrain.Update(null);
            await GenChange(customEventGrain, 13);

#endif

        }

        private async Task GenChange(IEventGrain eventGrain, int count)
        {
            for (int i = 0; i < count; i++)
            {
                await eventGrain.Update(new Change
                {
                    Name = "GenChangeTask",
                    Value = i,
                    When = DateTimeOffset.UtcNow
                });
            }
        }
    }
}