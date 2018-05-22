namespace EventSourcing.EventStates
{
    using System;

    [Serializable]
    public class SimpleSnapshot<TValue>
    {
        private TValue value;
        private DateTimeOffset when;

        public SimpleSnapshot(TValue value, DateTimeOffset when)
        {
            this.value = value;
            this.when = when;
        }

        public DateTimeOffset When => when;

        public TValue Value => value;

    }
}