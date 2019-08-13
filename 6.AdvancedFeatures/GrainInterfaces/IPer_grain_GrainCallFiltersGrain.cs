namespace GrainInterfaces
{
    using Orleans;
    using System.Threading.Tasks;

    public interface IPer_grain_GrainCallFiltersGrain : IGrainWithIntegerKey
    {
        Task<int> Call(int num = 86);
    }
}