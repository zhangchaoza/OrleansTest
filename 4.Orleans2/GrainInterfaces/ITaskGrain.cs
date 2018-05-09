using System.Threading.Tasks;
using Orleans;
using Orleans.CodeGeneration;

namespace GrainInterfaces
{
    [Version(1)]
    public interface ITaskGrain : IGrainWithIntegerKey
    {
        Task MyGrainMethod();
    }
}