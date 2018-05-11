namespace GrainInterfaces
{
    using System;

    [Serializable]
    public class Change
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public DateTimeOffset When { get; set; }
    }
}