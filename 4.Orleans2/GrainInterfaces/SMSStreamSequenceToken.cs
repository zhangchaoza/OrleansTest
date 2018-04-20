using Orleans.Streams;

namespace GrainInterfaces
{
    public class SMSStreamSequenceToken : StreamSequenceToken
    {
        public override int CompareTo(StreamSequenceToken other)
        {
            throw new System.NotImplementedException();
        }

        public override bool Equals(StreamSequenceToken other)
        {
            throw new System.NotImplementedException();
        }
    }
}