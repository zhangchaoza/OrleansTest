using Orleans.Streams;

namespace GrainImplement
{
    public class SimpleStreamSequenceToken : StreamSequenceToken
    {
        internal SimpleStreamSequenceToken(int index)
        {
            Index = index;
        }

        public int Index { get; }

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