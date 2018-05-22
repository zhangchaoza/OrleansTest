namespace GrainInterfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Orleans;
    using Orleans.CodeGeneration;

    public interface IEventGrain
    {
        Task Update(Change change);

        Task<Change> GetTop();

        Task<IReadOnlyList<Change>> GetAllEvents();

        Task<double> GetCurrentValue();
    }
}