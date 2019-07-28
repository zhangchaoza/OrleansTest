namespace GrainImplement
{
    using System.Threading;
    using System.Threading.Tasks;
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using System;

    public class ExternalTasksGrains : Grain, IExternalTasksGrains
    {
        private readonly ILogger<ExternalTasksGrains> logger;

        public ExternalTasksGrains(ILogger<ExternalTasksGrains> logger)
        {
            this.logger = logger;
        }

        public async Task RunExternalTask()
        {
            // Grab the Orleans task scheduler
            var orleansTs = TaskScheduler.Current;
            await Task.Delay(2000);
            // Current task scheduler did not change, the code after await is still running in the same task scheduler.
            logger.LogInformation($"{nameof(RunExternalTask)}:{nameof(orleansTs)} == {nameof(TaskScheduler.Current)} = {orleansTs.Equals(TaskScheduler.Current)}");

            Task t1 = Task.Run(() =>
            {
                // This code runs on the thread pool scheduler, not on Orleans task scheduler
                logger.LogInformation($"{nameof(t1)}:{nameof(orleansTs)} == {nameof(TaskScheduler.Current)} = {orleansTs.Equals(TaskScheduler.Current)}");
                logger.LogInformation($"{nameof(t1)}:{nameof(TaskScheduler.Default)} == {nameof(TaskScheduler.Current)} = {TaskScheduler.Default.Equals(TaskScheduler.Current)}");
            });
            await t1;
            // We are back to the Orleans task scheduler. 
            // Since await was executed in Orleans task scheduler context, we are now back to that context.
            logger.LogInformation($"{nameof(RunExternalTask)}:{nameof(orleansTs)} == {nameof(TaskScheduler.Current)} = {orleansTs.Equals(TaskScheduler.Current)}");
        }

        public async Task RunExternalTask2()
        {
            // Grab the Orleans task scheduler
            var orleansTs = TaskScheduler.Current;
            Task t1 = Task.Run(async () =>
            {
                // This code runs on the thread pool scheduler, not on Orleans task scheduler
                logger.LogInformation($"{nameof(t1)}:{nameof(orleansTs)} == {nameof(TaskScheduler.Current)} = {orleansTs.Equals(TaskScheduler.Current)}");
                // You can do whatever you need to do here. Now let's say you need to make a grain call.
                Task<Task> t2 = Task.Factory.StartNew(() =>
                {
                    // This code runs on the Orleans task scheduler since we specified the scheduler: orleansTs.
                    logger.LogInformation($"{nameof(t2)}:{nameof(orleansTs)} == {nameof(TaskScheduler.Current)} = {orleansTs.Equals(TaskScheduler.Current)}");

                    return GrainFactory.GetGrain<IRequestContextTestGrain>(0).DisplayRequestContext();
                }, CancellationToken.None, TaskCreationOptions.None, scheduler: orleansTs);

                await t2; // This code runs back on the thread pool scheduler, not on the Orleans task scheduler

                logger.LogInformation($"{nameof(t1)}:{nameof(orleansTs)} == {nameof(TaskScheduler.Current)} = {orleansTs.Equals(TaskScheduler.Current)}");
            });

            await t1;
            // We are back to the Orleans task scheduler.
            // Since await was executed in the Orleans task scheduler context, we are now back to that context.
            logger.LogInformation($"{nameof(RunExternalTask2)}:{nameof(orleansTs)} == {nameof(TaskScheduler.Current)} = {orleansTs.Equals(TaskScheduler.Current)}");
        }

        public async Task WaitGrainMethod()
        {
            var orleansTs = TaskScheduler.Current;
            {
                var t = new Thread(new ParameterizedThreadStart((o) =>
                {
                    ((IGrainFactory)o).GetGrain<GrainInterfaces.IRequestContextTestGrain>(0).DisplayRequestContext().GetAwaiter().GetResult();
                }));
                t.Start(GrainFactory);
                t.Join();
                logger.LogInformation($"{nameof(WaitGrainMethod)}通过新线程:执行完成");
            }

            {
                var gf = GrainFactory;
                Task t1 = Task.Run(() =>
                {
                    SampleWorker sw = new SampleWorker(gf, orleansTs);
                    sw.Do();
                });


                logger.LogInformation($"{nameof(WaitGrainMethod)}:{nameof(orleansTs)} == {nameof(TaskScheduler.Current)} = {orleansTs.Equals(TaskScheduler.Current)}");
                logger.LogInformation($"{nameof(WaitGrainMethod)}:{nameof(TaskScheduler.Default)} == {nameof(TaskScheduler.Current)} = {TaskScheduler.Default.Equals(TaskScheduler.Current)}");

                await t1;
                logger.LogInformation($"{nameof(WaitGrainMethod)}通过任务:执行完成");
            }
        }
    }
}