using System.Threading.Tasks;
using GrainInterfaces;
using Orleans;
using System.Diagnostics;
using System.Threading;

namespace GrainImplement
{
    public class TaskGrain : Grain, ITaskGrain
    {
        public async Task MyGrainMethod()
        {
            var a = Task.FromResult(10).Result;

            //var b = Task.Run(() => GrainFactory.GetGrain<ITaskGrain2>(0).MyGrainMethod2()).Result;
            // var b = GrainFactory.GetGrain<ITaskGrain2>(0).MyGrainMethod2().Result;

            var orleansTs = TaskScheduler.Current;
            Task<Task<int>> t2 = Task.Factory.StartNew(
                () => GrainFactory.GetGrain<ITaskGrain2>(0).MyGrainMethod2(),
                CancellationToken.None,
                TaskCreationOptions.None,
                scheduler: orleansTs);

            var b = await t2;
            // var res = t2.Result.Result;
            int res = await (await t2);
        }
    }
}