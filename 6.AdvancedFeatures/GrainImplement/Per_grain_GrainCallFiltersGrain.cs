namespace GrainImplement
{
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Runtime;

    /// <summary>
    /// Ordering of Grain Call Filters
    ///     1.Grain call filters follow a defined ordering:
    ///         IIncomingGrainCallFilter implementations configured in the dependency injection container, in the order in which they are registered.
    ///     2.Grain-level filter, if the grain implements IIncomingGrainCallFilter.
    ///     3.Grain method implementation or grain extension method implementation.
    /// </summary>
    public class Per_grain_GrainCallFiltersGrain : Grain, IPer_grain_GrainCallFiltersGrain, IIncomingGrainCallFilter
    {
        private readonly ILogger logger;

        public Per_grain_GrainCallFiltersGrain(ILogger<Per_grain_GrainCallFiltersGrain> logger)
        {
            this.logger = logger;
        }

        public Task<int> Call(int num = 86)
        {
            logger.LogInformation("call:{0}", num);

            return Task.FromResult(num);
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            logger.LogInformation("call filter:{0}", RequestContext.Get("intercepted value"));
            logger.LogInformation("CallFilter by grain");

            await context.Invoke();

            // Change the result of the call from 7 to 38.
            if (string.Equals(context.InterfaceMethod.Name, nameof(this.Call)))
            {
                context.Result = 85;
            }
        }
    }
}