namespace MySiloHost.StartupTasks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Runtime;

    public class GetCurrentTask : IStartupTask
    {
        private readonly IGrainFactory grainFactory;
        private readonly ILogger logger;

        public GetCurrentTask(IGrainFactory grainFactory, ILogger<GetCurrentTask> logger)
        {
            this.grainFactory = grainFactory;
            this.logger = logger;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {

#if LOG_BASED

            var logEventGrain = grainFactory.GetGrain<ILogStorageBasedEventGrain>(0);
            DisplayCurrent(nameof(logEventGrain), await logEventGrain.GetCurrentValue());

#endif

#if STATE_BASED

            var stateEventGrain = grainFactory.GetGrain<IStateStorageBasedEventGrain>(0);
            DisplayCurrent(nameof(stateEventGrain), await stateEventGrain.GetCurrentValue());

#endif

#if CUSTOM_BASED

            var customEventGrain = grainFactory.GetGrain<ICustomStorageBasedEventGrain>(0);
            DisplayCurrent(nameof(customEventGrain), await customEventGrain.GetCurrentValue());

#endif

        }

        private void DisplayCurrent(string name, double top)
        {
            logger.LogInformation("{0}当前值：{1}", name, top);
        }
    }
}