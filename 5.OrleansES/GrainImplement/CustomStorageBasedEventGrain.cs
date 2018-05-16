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
    using Orleans.EventSourcing.CustomStorage;
    using System.Collections.Generic;
    using EventTable;
    using Newtonsoft.Json;
    using System.Linq;

    [LogConsistencyProvider(ProviderName = "CustomStorage")]
    public class CustomStorageBasedEventGrain :
        JournaledGrain<EventState, Change>,
        ICustomStorageBasedEventGrain,
        ICustomStorageInterface<EventState, Change>
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

        #region ICustomStorageInterface

        private SimpleEventTable table;

        async Task InitStorageInterface()
        {
            table = new SimpleEventTable(logger);
            await table.Connect();
        }

        async Task<KeyValuePair<int, EventState>> ICustomStorageInterface<EventState, Change>.ReadStateFromStorage()
        {
            var events = await table.ReadEventState<Change>(EventName);
            EventState state = new EventState(events);
            return new KeyValuePair<int, EventState>(state.Changes.Count, state);
        }

        Task<bool> ICustomStorageInterface<EventState, Change>.ApplyUpdatesToStorage(IReadOnlyList<Change> updates, int expectedversion)
        {
            return table.UpdateEventState(EventName, expectedversion, updates);
        }

        #endregion
    }
}