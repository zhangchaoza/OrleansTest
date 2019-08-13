namespace GrainImplement
{
    using GrainInterfaces;
    using Orleans.Runtime.Services;
    using System;
    using System.Threading.Tasks;

    public class DataServiceClient : GrainServiceClient<IDataService>, IDataServiceClient
    {
        public DataServiceClient(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public Task MyMethod() => GrainService.MyMethod();
    }
}