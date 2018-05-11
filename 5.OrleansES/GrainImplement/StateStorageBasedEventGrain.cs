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

    [LogConsistencyProvider(ProviderName = "StateStorage")]
    public class StateStorageBasedEventGrain : JournaledGrain<EventState, Change>, IStateStorageBasedEventGrain
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

    }
}