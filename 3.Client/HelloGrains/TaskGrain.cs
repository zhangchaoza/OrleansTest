using GrainInterfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloGrains
{
    public class TaskGrain : Grain, ITaskGrain
    {
        public Task MyGrainMethod()
        {
            var a = Task.FromResult(10).Result;

            var b = GrainFactory.GetGrain<ITaskGrain2>(0).MyGrainMethod2().Result;

            return Task.CompletedTask;
        }
    }
}
