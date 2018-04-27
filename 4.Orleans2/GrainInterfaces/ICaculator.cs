namespace GrainInterfaces
{
    using System.Threading.Tasks;
    using Orleans;
    using Orleans.Concurrency;

    public interface ICaculator : IGrainWithGuidKey
    {
        Task<int> Add(int x, int y);
        Task<ImmutableType> GetImmutable(ImmutableType x);
        Task<Immutable<byte[]>> ProcessRequest(Immutable<byte[]> request);
    }
}