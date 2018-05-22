namespace EventSourcing.EventStates
{
    using System;
    using System.Collections.Generic;
    using EventSourcing.Abstractions;
    using System.Linq;

    public class SimpleEventState<TEventBase> where TEventBase : IEventBase
    {

        private SortedDictionary<DateTimeOffset, TEventBase> events;


        public SimpleEventState()
        {
            events = new SortedDictionary<DateTimeOffset, TEventBase>();
        }

        public SortedDictionary<DateTimeOffset, TEventBase> Events => events;

        public void Apply(TEventBase delta)
        {
            if (events == null)
                throw new ArgumentNullException("events");

            // idempotency check: ignore update if already there
            if (events.ContainsKey(delta.When))
                return;

            events.Add(delta.When, delta);
        }

        public double GetCurrent()
        {
            return events
                 .OrderBy(o => o.Key)
                 .Sum(i => i.Value.Value);
        }

        public TEventBase GetNewestEvent()
        {
            return events
                .OrderByDescending(o => o.Key)
                .Select(i => i.Value)
                .FirstOrDefault();
        }
    }
}