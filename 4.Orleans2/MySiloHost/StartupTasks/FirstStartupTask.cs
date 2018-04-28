using System;
using System.Threading;
using System.Threading.Tasks;
using GrainInterfaces;
using Orleans;
using Orleans.Runtime;

namespace MySiloHost.StartupTasks
{

    public class FirstStartupTask : IStartupTask
    {
        private readonly IGrainFactory grainFactory;

        public FirstStartupTask(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            var caculator = grainFactory.GetGrain<ICaculator>(Guid.Empty);
            await caculator.Add(1, 2);
        }
    }
}