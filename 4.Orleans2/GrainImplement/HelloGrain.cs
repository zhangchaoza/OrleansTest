namespace GrainImplement
{
    using System;
    using System.Threading.Tasks;
    using GrainImplement.States;
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Providers;
    using Orleans.Runtime;


    [StorageProvider(ProviderName = "DevStore")]
    public class HelloGrain : Grain<HelloGrainState>, IHello, IRemindable
    {
        private readonly ILogger logger;

        public HelloGrain(ILogger<HelloGrain> logger)
        {
            this.logger = logger;
        }

        public override Task OnActivateAsync()
        {
            return RegisterOrUpdateReminder("r1", TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1));
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            logger.LogInformation($"---ReceiveReminder {reminderName}, last message{State.LastMessage}'");
            return Task.CompletedTask;
        }

        async Task<string> IHello.SayHello(string greeting)
        {
            logger.LogInformation($"+++SayHello message received: greeting = '{greeting}',last message is '{State.LastMessage}'");
            State.LastMessage = greeting;
            await base.WriteStateAsync();
            return $"You said: '{greeting}', I say: Hello!";
        }

    }
}