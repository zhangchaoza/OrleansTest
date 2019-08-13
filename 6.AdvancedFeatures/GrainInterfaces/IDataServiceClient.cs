namespace GrainInterfaces
{
    using Orleans.Services;

    public interface IDataServiceClient : IGrainServiceClient<IDataService>, IDataService
    {
    }
}