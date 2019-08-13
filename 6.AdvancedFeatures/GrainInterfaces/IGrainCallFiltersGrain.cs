namespace GrainInterfaces
{
    using Orleans;
    using System.Threading.Tasks;

    public interface IGrainCallFiltersGrain : IGrainWithIntegerKey
    {
        Task<int> Call(int num = 86);
    }
}