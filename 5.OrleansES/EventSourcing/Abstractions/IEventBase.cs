namespace EventSourcing.Abstractions
{
    using System;

    public interface IEventBase
    {
        DateTimeOffset When { get; }
        double Value { get; }

    }
}