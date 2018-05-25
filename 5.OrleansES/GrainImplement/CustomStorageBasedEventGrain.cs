namespace GrainImplement
{
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Providers;
    using Orleans.EventSourcing;
    using System.Reflection;
    using Orleans.EventSourcing.CustomStorage;
    using System.Collections.Generic;
    using EventSourcing;
    using EventSourcing.EventModels;
    using EventSourcing.EventStorages.Redis;
    using GrainImplement.EventStates;

    [LogConsistencyProvider(ProviderName = "CustomStorage")]
    public class CustomStorageBasedEventGrain :
        JournaledGrain<ChangeEventState, Change>,
        ICustomStorageBasedEventGrain,
        ICustomStorageInterface<ChangeEventState, Change>
    {
        private readonly ILogger logger;

        public CustomStorageBasedEventGrain(ILogger<CustomStorageBasedEventGrain> logger)
        {
            this.logger = logger;
        }

        protected string EventName => $"CustomStorageBasedEventGrain<{this.GetPrimaryKeyLong()}>";

        public async override Task OnActivateAsync()
        {
            await InitStorageInterface();
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

        protected override void TransitionState(ChangeEventState state, Change change)
        {
            logger.LogInformation($"{EventName}_{MethodBase.GetCurrentMethod().Name} 版本: {Version}");
            state.Apply(change, Version);
        }

        #endregion

        #region IEventGrain

        Task<Change> IEventGrain.GetTop()
        {
            return State.GetNewestEvent();
        }

        Task<IReadOnlyList<Change>> IEventGrain.GetAllEvents()
        {
            return State.GetAllEvents();
        }

        async Task IEventGrain.Update(Change change)
        {
            if (null == change)
            {
                return;
            }
            logger.LogInformation($"{EventName} update:{{0}},{{1}},{{2}}", change.Name, change.Value, change.When);
            await RaiseConditionalEvent(change);
            // RaiseEvent(change);
            // await ConfirmEvents();
        }

        Task<double> IEventGrain.GetCurrentValue()
        {
            return Task.FromResult(State.GetCurrent());
        }

        #endregion

        #region ICustomStorageInterface

        private IEventTable table;

        Task InitStorageInterface()
        {
            table = new RedisEventTable(logger);
            State.Init(EventName, table);
            return Task.CompletedTask;
        }

        async Task<KeyValuePair<int, ChangeEventState>> ICustomStorageInterface<ChangeEventState, Change>.ReadStateFromStorage()
        {
            var version = await State.ReadFromStorage();
            return new KeyValuePair<int, ChangeEventState>(version, State);
        }

        Task<bool> ICustomStorageInterface<ChangeEventState, Change>.ApplyUpdatesToStorage(IReadOnlyList<Change> updates, int expectedversion)
        {
            return table.ApplyNotSavedSnapshotEvents(EventName, expectedversion + 1, updates);
        }

        #endregion
    }
}