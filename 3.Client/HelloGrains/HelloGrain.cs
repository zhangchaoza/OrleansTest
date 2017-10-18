using GrainInterfaces;
using Orleans;
using System;
using System.Threading.Tasks;

namespace HelloGrains
{

    /// <summary>
    /// Grain implementation class Grain1.
    /// </summary>
    internal class HelloGrain : Grain, IHello
    {

        private string text = "Hello World!";

        public Task<string> SayHello(string msg)
        {
            Console.WriteLine($"{this.GetPrimaryKeyLong()} said { msg},lastest msg :{text}");
            var oldText = text;
            text = msg;
            return Task.FromResult(string.Format("You said {0}, I say: Hello!", msg));
        }
    }
}