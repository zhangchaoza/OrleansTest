namespace GrainImplement
{
    using System;
    using System.Threading.Tasks;
    using GrainImplement.States;
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Concurrency;
    using Orleans.Providers;
    using Orleans.Runtime;

    [StatelessWorker(10)]
    public class CaculatorGrain : Grain, ICaculator
    {
        public Task<int> Add(int x, int y)
        {
            unchecked
            {
                return Task.FromResult(x + y);
            }
        }
    }
}