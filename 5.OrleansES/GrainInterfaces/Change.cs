namespace GrainInterfaces
{
    using System;
    using EventSourcing.Abstractions;

    [Serializable]
    public class Change : IEventBase
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public DateTimeOffset When { get; set; }
    }
}