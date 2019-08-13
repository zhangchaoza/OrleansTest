namespace GrainInterfaces
{
    using Orleans.CodeGeneration;
    using Orleans.Services;
    using System.Threading.Tasks;

    [Version(1)]
    public interface IDataService : IGrainService
    {
        Task MyMethod();
    }
}