namespace GrainInterfaces
{
    using System.Threading.Tasks;
    using Orleans;
    using Orleans.CodeGeneration;

    public interface IEventGrain
    {
        Task Update(Change change);
        Task<Change> GetTop();
    }
}