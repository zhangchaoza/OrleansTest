namespace GrainInterfaces
{
    using System.Threading.Tasks;
    using Orleans;

    public interface IGrainCallFiltersGrain : IGrainWithIntegerKey
    {

        Task<int> Call(int num = 86);
    }
}