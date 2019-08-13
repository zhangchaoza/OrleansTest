namespace GrainInterfaces
{
    using System;
    using System.Threading.Tasks;
    using Orleans;

    public interface ICancellationTokensGrain : IGrainWithIntegerKey
    {
        Task LongIoWork(GrainCancellationToken tc, TimeSpan delay);
    }
}