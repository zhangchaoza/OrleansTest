namespace EventSourcing.Abstractions
{
    using System;

    public interface ISnopshot<TEventuallyValue>
    {
        DateTimeOffset When { get; }

        TEventuallyValue Value { get; }
    }
}