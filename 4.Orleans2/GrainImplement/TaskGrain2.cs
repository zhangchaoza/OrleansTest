using System.Threading.Tasks;
using GrainInterfaces;
using Orleans;
using System.Diagnostics;
using System.Threading;

namespace GrainImplement
{
    public class TaskGrain2 : Grain, ITaskGrain2
    {
        public Task<int> MyGrainMethod2()
        {
            return Task.FromResult(10);
        }
    }
}