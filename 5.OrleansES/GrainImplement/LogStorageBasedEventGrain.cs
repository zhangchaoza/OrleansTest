namespace GrainImplement
{
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Providers;
    using Orleans.EventSourcing;
    using System.Reflection;
    using System.Linq;
    using System.Collections.Generic;
    using EventSourcing.EventStates;
    using GrainImplement.EventStates;

    [StorageProvider(ProviderName = "Default")]
    [LogConsistencyProvider(ProviderName = "LogStorage")]
    public class LogStorageBasedEventGrain : JournaledGrain<SimpleChangeEventState, Change>, ILogStorageBasedEventGrain
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
            // logger.LogInformation($"{EventName}_{MethodBase.GetCurrentMethod().Name} 版本: {Version}");
        }

        protected override void OnStateChanged()
        {
            // logger.LogInformation($"{EventName}_{MethodBase.GetCurrentMethod().Name} 版本: {Version}");
        }

        protected override void TransitionState(SimpleChangeEventState state, Change delta)
        {
            logger.LogInformation($"{EventName}_{MethodBase.GetCurrentMethod().Name} 版本: {Version}");
            state.Apply(delta);
        }

        #endregion

        #region IEventGrain

        Task<Change> IEventGrain.GetTop()
        {
            var newestEvent = State.GetNewestEvent();
            return Task.FromResult(newestEvent);
        }

        async Task<IReadOnlyList<Change>> IEventGrain.GetAllEvents()
        {
            var allEvents = await RetrieveConfirmedEvents(0, Version);
            return allEvents;
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

        Task<double> IEventGrain.GetCurrentValue()
        {
            return Task.FromResult(State.GetCurrent());
        }

        #endregion

    }
}