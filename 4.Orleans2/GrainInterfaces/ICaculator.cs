namespace GrainInterfaces
{
    using System.Threading.Tasks;
    using Orleans;

    public interface ICaculator : IGrainWithGuidKey
    {
        Task<int> Add(int x, int y);
    }
}