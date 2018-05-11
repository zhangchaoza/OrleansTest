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
            throw new NotImplementedException();
        }

        Task IEventGrain.Update(Change change)
        {
            if (null == change)
            {
                return Task.CompletedTask;
            }
            logger.LogInformation($"{EventName} update:{{0}},{{1}},{{2}}", change.Name, change.Value, change.When);
            RaiseEvent(change);
            return Task.CompletedTask;
            // await ConfirmEvents();
        }


        #endregion

        #region ICustomStorageInterface

        Task<KeyValuePair<int, EventState>> ICustomStorageInterface<EventState, Change>.ReadStateFromStorage()
        {
            EventState state = new EventState();
            state.Apply(new Change
            {
                Name = "Fake",
                Value = -2,
                When = DateTimeOffset.UtcNow
            });
            return Task.FromResult(new KeyValuePair<int, EventState>(1, state));
        }

        Task<bool> ICustomStorageInterface<EventState, Change>.ApplyUpdatesToStorage(IReadOnlyList<Change> updates, int expectedversion)
        {
            return Task.FromResult(true);
        }

        #endregion
    }
}