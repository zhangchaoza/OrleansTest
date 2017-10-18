using Orleans;
using System.Threading.Tasks;

namespace GrainInterfaces
{
    /// <summary>
    /// Grain interface IGrain1
    /// </summary>
    public interface IHello : IGrainWithIntegerKey
    {
        Task<string> SayHello(string msg);
    }
}