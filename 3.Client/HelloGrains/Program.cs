using Orleans;
using Orleans.Runtime.Configuration;
using System;

namespace HelloGrains
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Waiting for Orleans Silo to start. Press Enter to proceed...");
            Console.ReadLine();

            // Orleans comes with a rich XML and programmatic configuration. Here we're just going to set up with basic programmatic config
            var config = ClientConfiguration.LocalhostSilo(30000);

            GrainClient.Initialize(config);
        }
    }
}