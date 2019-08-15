namespace GrainInterfaces
{
    using System.Threading.Tasks;
    using Orleans;

    public interface IAccountGrain : IGrainWithGuidKey
    {
        [Transaction(TransactionOption.Join)]
        Task Withdraw(uint amount);

        [Transaction(TransactionOption.Join)]
        Task Deposit(uint amount);

        [Transaction(TransactionOption.CreateOrJoin)]
        Task<uint> GetBalance();
    }
}