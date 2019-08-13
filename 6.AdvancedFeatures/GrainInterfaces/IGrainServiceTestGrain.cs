namespace GrainInterfaces
{
    using Orleans;
    using System.Threading.Tasks;

    public interface IGrainServiceTestGrain : IGrainWithIntegerKey
    {
        Task Call();

        Task CallByService();
    }
}