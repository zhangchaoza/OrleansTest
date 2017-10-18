using GrainInterfaces;
using Orleans;
using System.Threading.Tasks;

namespace GrainCollection
{
    internal class HelloGrain : Grain, IHello
    {
        public Task<string> SayHello(string msg)
        {
            return Task.FromResult(string.Format("You said {0}, I say: Hello!", msg));
        }
    }
}