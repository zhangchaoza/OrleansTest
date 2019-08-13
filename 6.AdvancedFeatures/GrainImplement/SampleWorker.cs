namespace GrainImplement
{
    using GrainInterfaces;
    using Orleans;
    using System.Threading;
    using System.Threading.Tasks;

    internal class SampleWorker
    {
        private readonly IGrainFactory grainFactory;
        private readonly TaskScheduler orleansTs;

        public SampleWorker(IGrainFactory grainFactory, TaskScheduler orleansTs)
        {
            this.grainFactory = grainFactory;
            this.orleansTs = orleansTs;
        }

        public void Do()
        {
            Task<Task> t2 = Task.Factory.StartNew(() =>
            {
                return grainFactory.GetGrain<IRequestContextTestGrain>(0).DisplayRequestContext();
            }, CancellationToken.None, TaskCreationOptions.None, scheduler: orleansTs);

            t2.Wait();// 同步调用Grain方法
                      // 必须在非orleans TaskScheduler
                      // 例如通过Task.Run方法在TaskScheduler.Default中等待Grain方法
        }
    }
}