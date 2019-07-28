namespace GrainInterfaces
{
    using Orleans;
    using Orleans.CodeGeneration;
    using System.Threading.Tasks;

    [Version(1)]
    public interface IExternalTasksGrains : IGrainWithIntegerKey
    {

        Task RunExternalTask();

        Task RunExternalTask2();

        Task WaitGrainMethod();

    }
}