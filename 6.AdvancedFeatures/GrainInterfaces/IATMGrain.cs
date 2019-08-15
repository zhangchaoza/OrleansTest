namespace GrainInterfaces
{
    using System;
    using System.Threading.Tasks;
    using Orleans;

    public interface IATMGrain : IGrainWithIntegerKey
    {
        [Transaction(TransactionOption.Create)]
        Task Transfer(Guid fromAccount, Guid toAccount, uint amountToTransfer);
    }
}