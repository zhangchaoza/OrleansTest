namespace GrainImplement
{
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Concurrency;
    using Orleans.Core;
    using Orleans.Runtime;
    using System;
    using System.Threading.Tasks;

    [Reentrant]
    public class LightstreamerDataService : GrainService, IDataService
    {
        private readonly ILogger logger;
        private readonly IGrainFactory GrainFactory;

        public LightstreamerDataService(IServiceProvider services, IGrainIdentity id, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory)
            : base(id, silo, loggerFactory)
        {
            logger = loggerFactory.CreateLogger<LightstreamerDataService>();
            GrainFactory = grainFactory;
        }

        public override Task Init(IServiceProvider serviceProvider)
        {
            logger.LogInformation("Init");
            return base.Init(serviceProvider);
        }

        public override async Task Start()
        {
            logger.LogInformation("Start");
            await base.Start();
        }

        protected override Task StartInBackground()
        {
            logger.LogInformation("StartInBackground");
            return base.StartInBackground();
        }

        public override Task OnRangeChange(IRingRange oldRange, IRingRange newRange, bool increased)
        {
            logger.LogInformation("OnRangeChange");
            return base.OnRangeChange(oldRange, newRange, increased);
        }

        public override Task Stop()
        {
            logger.LogInformation("Stop");
            return base.Stop();
        }

        public async Task MyMethod()
        {
            logger.LogInformation("MyMethod");
            var grain = GrainFactory.GetGrain<IGrainServiceTestGrain>(10);
            await grain.CallByService();
            logger.LogInformation("MyMethod2");
        }
    }
}