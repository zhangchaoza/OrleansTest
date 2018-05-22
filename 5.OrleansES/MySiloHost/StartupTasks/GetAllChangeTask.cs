namespace MySiloHost.StartupTasks
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Runtime;

    public class GetAllChangeTask : IStartupTask
    {
        private readonly IGrainFactory grainFactory;
        private readonly ILogger logger;

        public GetAllChangeTask(IGrainFactory grainFactory, ILogger<GetAllChangeTask> logger)
        {
            this.grainFactory = grainFactory;
            this.logger = logger;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {

#if LOG_BASED

            var logEventGrain = grainFactory.GetGrain<ILogStorageBasedEventGrain>(0);
            DisplayAll(nameof(logEventGrain), await logEventGrain.GetAllEvents());

#endif

#if STATE_BASED

            var stateEventGrain = grainFactory.GetGrain<IStateStorageBasedEventGrain>(0);
            DisplayAll(nameof(stateEventGrain), await stateEventGrain.GetAllEvents());

#endif

#if CUSTOM_BASED

            var customEventGrain = grainFactory.GetGrain<ICustomStorageBasedEventGrain>(0);
            DisplayAll(nameof(customEventGrain), await customEventGrain.GetAllEvents());

#endif

        }

        private void DisplayAll(string name, IReadOnlyList<Change> allEvents)
        {
            if (null == allEvents || allEvents.Count == 0)
            {
                logger.LogInformation("{0}没有更新", name);
            }
            else
            {
                foreach (var e in allEvents)
                {
                    logger.LogInformation("{0}历史更新事件：{1},{2},{3}", name, e.Name, e.Value, e.When);
                }
            }
        }
    }
}