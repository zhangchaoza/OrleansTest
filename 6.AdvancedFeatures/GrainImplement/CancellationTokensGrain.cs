namespace GrainImplement
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;

    public class CancellationTokensGrain : Grain, ICancellationTokensGrain
    {
        private readonly ILogger<CancellationTokensGrain> logger;

        public CancellationTokensGrain(ILogger<CancellationTokensGrain> logger)
        {
            this.logger = logger;
        }

        public async Task LongIoWork(GrainCancellationToken tc, TimeSpan delay)
        {
            logger.LogInformation("LongIoWork");
            var timesamp = DateTimeOffset.UtcNow;
            while (!tc.CancellationToken.IsCancellationRequested && (DateTimeOffset.UtcNow - timesamp) < delay)
            {
                await IoOperation(tc.CancellationToken);
            }
            if (tc.CancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Cancel");
            }
            else
            {
                logger.LogInformation("timeout");
            }
        }

        private Task IoOperation(CancellationToken cancellationToken)
        {
            logger.LogDebug("IoOperation");
            return Task.Delay(100);
        }
    }
}