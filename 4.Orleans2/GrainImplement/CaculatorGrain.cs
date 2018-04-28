namespace GrainImplement
{
    using System;
    using System.Threading.Tasks;
    using GrainImplement.States;
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Concurrency;
    using Orleans.Providers;
    using Orleans.Runtime;

    [StatelessWorker(10)]
    public class CaculatorGrain : Grain, ICaculator
    {
        private readonly ILogger logger;

        public CaculatorGrain(ILogger<CaculatorGrain> logger)
        {
            this.logger = logger;
        }

        public Task<int> Add(int x, int y)
        {
            logger.LogInformation("Add:{0}+{1}", x, y);
            unchecked
            {
                return Task.FromResult(x + y);
            }
        }

        public Task<ImmutableType> GetImmutable(ImmutableType x)
        {
            return Task.FromResult(x);
        }

        public Task<Immutable<byte[]>> ProcessRequest(Immutable<byte[]> request)
        {
            request.Value[0] = 1;
            return Task.FromResult(request);
        }
    }
}