using Orleans;
using System.Threading.Tasks;

namespace GrainInterfaces
{
    public interface ITaskGrain2 : IGrainWithIntegerKey
    {
        Task<int> MyGrainMethod2();
    }
}