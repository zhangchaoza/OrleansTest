namespace GrainImplement
{
    using System;
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Providers;
    using Orleans.Runtime;
    using Orleans.EventSourcing;
    using System.Reflection;
    using System.Linq;

    // [StorageProvider(ProviderName = "Default")]
    [LogConsistencyProvider(ProviderName = "LogStorage")]
    public class LogStorageBasedEventGrain : JournaledGrain<EventState, Change>, ILogStorageBasedEventGrain
    {
        private readonly ILogger logger;

        public LogStorageBasedEventGrain(ILogger<LogStorageBasedEventGrain> logger)
        {
            this.logger = logger;
        }

        protected string EventName => $"LogStorageBasedEventGrain<{this.GetPrimaryKeyLong()}>";

        public override Task OnActivateAsync()
        {
            return Task.CompletedTask;
        }


        #region JournaledGrain

        protected override void OnTentativeStateChanged()
        {
            logger.LogInformation($"{EventName}_{MethodBase.GetCurrentMethod().Name} 版本: {Version}");
        }

        protected override void OnStateChanged()
        {
            logger.LogInformation($"{EventName}_{MethodBase.GetCurrentMethod().Name} 版本: {Version}");
        }

        protected override void TransitionState(EventState state, Change delta)
        {
            logger.LogInformation($"{EventName}_{MethodBase.GetCurrentMethod().Name} 版本: {Version}");
            state.Apply(delta);
        }

        #endregion

        #region IEventGrain

        Task<Change> IEventGrain.GetTop()
        {
            var result = State.Changes
                .OrderByDescending(o => o.Value.When)
                .Select(i => i.Value)
                .FirstOrDefault();
            return Task.FromResult(result);
        }

        async Task IEventGrain.Update(Change change)
        {
            if (null == change)
            {
                return;
            }
            logger.LogInformation($"{EventName} update:{{0}},{{1}},{{2}}", change.Name, change.Value, change.When);
            RaiseEvent(change);
            await ConfirmEvents();
        }

        #endregion
    }
}