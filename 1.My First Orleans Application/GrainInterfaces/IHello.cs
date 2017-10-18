using Orleans;
using System.Threading.Tasks;

namespace GrainInterfaces
{
    /// <summary>
    /// Grain interface IGrain1
    /// </summary>
    public interface IHello : IGrainWithGuidKey
    {
        Task<string> SayHello(string msg);
    }
}