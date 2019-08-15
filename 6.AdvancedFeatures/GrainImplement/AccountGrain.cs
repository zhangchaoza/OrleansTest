namespace GrainImplement
{
    using System;
    using System.Threading.Tasks;
    using GrainImplement.States;
    using GrainInterfaces;
    using Orleans;
    using Orleans.Transactions.Abstractions;

    public class AccountGrain : Grain, IAccountGrain
    {
        private readonly ITransactionalState<Balance> balance;

        public AccountGrain(
            [TransactionalState("balance", "TransactionStore")]ITransactionalState<Balance> balance)
        {
            this.balance = balance ?? throw new ArgumentNullException(nameof(balance));
        }

        Task IAccountGrain.Deposit(uint amount)
        {
            return this.balance.PerformUpdate(x => x.Value += amount);
        }

        Task IAccountGrain.Withdraw(uint amount)
        {
            return this.balance.PerformUpdate(x => x.Value -= amount);
        }

        Task<uint> IAccountGrain.GetBalance()
        {
            return this.balance.PerformRead(x => x.Value);
        }
    }
}