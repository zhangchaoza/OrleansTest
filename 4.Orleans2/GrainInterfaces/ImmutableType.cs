using Orleans.Concurrency;

namespace GrainInterfaces
{
    [Immutable]
    public class ImmutableType
    {
        public ImmutableType(int value)
        {
            this.MyValue = value;
        }

        public int MyValue { get; }

    }
}