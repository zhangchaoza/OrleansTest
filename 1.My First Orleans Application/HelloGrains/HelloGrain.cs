using GrainInterfaces;
using Orleans;
using System.Threading.Tasks;

namespace HelloGrains
{
    /// <summary>
    /// Grain implementation class Grain1.
    /// </summary>
    internal class HelloGrain : Grain, IHello
    {
        public Task<string> SayHello(string msg)
        {
            return Task.FromResult(string.Format("You said {0}, I say: Hello!", msg));
        }
    }
}