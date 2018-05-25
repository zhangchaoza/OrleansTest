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
    using System.Collections.Generic;
    using EventSourcing.EventModels;
    using GrainImplement.EventStates;

    [LogConsistencyProvider(ProviderName = "StateStorage")]
    public class StateStorageBasedEventGrain : JournaledGrain<SimpleChangeEventState, Change>, IStateStorageBasedEventGrain
    {
        private readonly ILogger logger;

        public StateStorageBasedEventGrain(ILogger<StateStorageBasedEventGrain> logger)
        {
            this.logger = logger;
        }

        protected string EventName => $"StateStorageBasedEventGrain<{this.GetPrimaryKeyLong()}>";

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

        Task<IReadOnlyList<Change>> IEventGrain.GetAllEvents()
        {
            var res = (IReadOnlyList<Change>)State.Events
                .OrderBy(o => o.Key)
                .Select(i => i.Value)
                .ToList();
            return Task.FromResult(res);
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