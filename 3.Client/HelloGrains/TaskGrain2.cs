using GrainInterfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloGrains
{
    public class TaskGrain2 : Grain, ITaskGrain2
    {
        public Task<int> MyGrainMethod2()
        {
            return Task.FromResult(10);
        }
    }
}
