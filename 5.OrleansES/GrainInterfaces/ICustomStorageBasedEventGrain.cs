namespace GrainInterfaces
{
    using Orleans;
    using Orleans.CodeGeneration;

    [Version(1)]

    public interface ICustomStorageBasedEventGrain : IEventGrain, IGrainWithIntegerKey
    {

    }
}