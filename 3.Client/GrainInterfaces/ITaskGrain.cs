using Orleans;
using System.Threading.Tasks;

namespace GrainInterfaces
{
    public interface ITaskGrain : IGrainWithIntegerKey
    {
        Task MyGrainMethod();
    }
}