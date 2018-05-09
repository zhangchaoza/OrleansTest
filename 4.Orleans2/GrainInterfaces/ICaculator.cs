namespace GrainInterfaces
{
    using System.Threading.Tasks;
    using Orleans;
    using Orleans.CodeGeneration;
    using Orleans.Concurrency;

    [Version(1)]
    public interface ICaculator : IGrainWithGuidKey
    {
        Task<int> Add(int x, int y);
        Task<ImmutableType> GetImmutable(ImmutableType x);
        Task<Immutable<byte[]>> ProcessRequest(Immutable<byte[]> request);
    }
}