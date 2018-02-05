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
        private ObserverSubscriptionManager<IChat> _subsManager;

        private string text = "Hello World!";

        public Task<string> SayHello(string msg)
        {
            Console.WriteLine($"{this.GetPrimaryKeyLong()} said { msg},lastest msg :{text}");
            var oldText = text;
            text = msg;
            return Task.FromResult(string.Format("You said {0}, I say: Hello!", msg));
        }

        public override async Task OnActivateAsync()
        {
            // We created the utility at activation time.
            _subsManager = new ObserverSubscriptionManager<IChat>();
            await base.OnActivateAsync();
        }

        // Clients call this to subscribe.
        public Task Subscribe(IChat observer)
        {
            _subsManager.Subscribe(observer);
            return Task.CompletedTask;
        }

        //Also clients use this to unsubscribe themselves to no longer receive the messages.
        public Task UnSubscribe(IChat observer)
        {
            _subsManager.Unsubscribe(observer);
            return Task.CompletedTask;
        }

        public Task SendUpdateMessage(string message)
        {
            _subsManager.Notify(s => s.ReceiveMessage(message));
            return Task.CompletedTask;
        }
    }
}