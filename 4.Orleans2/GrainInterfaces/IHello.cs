namespace GrainInterfaces
{
    using System.Threading.Tasks;
    using Orleans;
    using Orleans.CodeGeneration;

    [Version(1)]
    public interface IHello : IGrainWithIntegerKey
    {
        Task<string> SayHello(string greeting);

    }
}