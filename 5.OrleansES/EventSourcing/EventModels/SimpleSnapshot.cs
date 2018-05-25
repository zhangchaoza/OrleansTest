namespace EventSourcing.EventModels
{
    using System;
    using EventSourcing.Abstractions;

    [Serializable]
    public class SimpleSnapshot<TEventuallyValue> : ISnopshot<TEventuallyValue>
    {
        private TEventuallyValue value;
        private DateTimeOffset when;

        public SimpleSnapshot(TEventuallyValue value, DateTimeOffset when)
        {
            this.value = value;
            this.when = when;
        }

        public DateTimeOffset When => when;

        public TEventuallyValue Value => value;

    }
}