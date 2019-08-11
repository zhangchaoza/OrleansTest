namespace GrainInterfaces
{
    using System.Threading.Tasks;
    using Orleans;

    public interface IPer_grain_GrainCallFiltersGrain : IGrainWithIntegerKey
    {
        Task<int> Call(int num = 86);

    }
}