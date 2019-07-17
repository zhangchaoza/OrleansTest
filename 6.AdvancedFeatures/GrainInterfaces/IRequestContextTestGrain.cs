namespace GrainInterfaces
{
    using System.Threading.Tasks;
    using Orleans;
    using Orleans.CodeGeneration;

    [Version(1)]
    public interface IRequestContextTestGrain : IGrainWithIntegerKey
    {
        Task DisplayRequestContext();
    }
}