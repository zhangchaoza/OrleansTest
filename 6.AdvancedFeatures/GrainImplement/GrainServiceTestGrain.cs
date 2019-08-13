namespace GrainImplement
{
    using GrainImplement.States;
    using GrainInterfaces;
    using Orleans;
    using Orleans.Providers;
    using Orleans.Runtime;
    using System.Threading.Tasks;

    [StorageProvider(ProviderName = "DevStore")]
    public class GrainServiceTestGrain : Grain<GrainServiceTestGrainState>, IGrainServiceTestGrain
    {
        private readonly IDataServiceClient DataServiceClient;

        public GrainServiceTestGrain(IGrainActivationContext grainActivationContext, IDataServiceClient dataServiceClient)
        {
            DataServiceClient = dataServiceClient;
        }

        public async Task Call()
        {
            State.Num++;
            await WriteStateAsync();
            await DataServiceClient.MyMethod();
        }

        public Task CallByService()
        {
            return Task.Delay(1000);
        }
    }
}