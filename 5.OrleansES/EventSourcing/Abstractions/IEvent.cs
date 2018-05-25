namespace EventSourcing.Abstractions
{
    using System;

    public interface IEvent<TEventValue>
    {
        DateTimeOffset When { get; }

        TEventValue Value { get; }

    }
}