using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrainInterfaces;
using Orleans;

namespace HelloGrains
{
    public class Chat : IChat
    {
        public void ReceiveMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
