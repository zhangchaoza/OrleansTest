namespace GrainImplement
{
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Runtime;

    public class RequestContextTestGrain : Grain, IRequestContextTestGrain
    {
        private readonly ILogger<RequestContextTestGrain> logger;

        public RequestContextTestGrain(ILogger<RequestContextTestGrain> logger)
        {
            this.logger = logger;
        }

        public Task DisplayRequestContext()
        {
            logger.LogInformation("Currently processing external request {0}", RequestContext.Get("TraceId"));
            return Task.CompletedTask;
        }
    }
}