using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
namespace GrainInterfaces
{
    public interface IChat : IGrainObserver
    {
        void ReceiveMessage(string message);
    }
}
