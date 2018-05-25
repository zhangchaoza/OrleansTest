namespace EventSourcing.EventModels
{
    using System;
    using System.Collections.Generic;
    using EventSourcing.Abstractions;
    using System.Linq;

    /// <summary>
    /// 用户logbased、statebased
    /// </summary>
    /// <typeparam name="TEventBase"></typeparam>
    /// <typeparam name="TEventValue"></typeparam>
    public abstract class SimpleEventState<TEventBase, TEventValue, TEventuallyValue> where TEventBase : IEvent<TEventValue>
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

        public TEventuallyValue GetCurrent()
        {
            TEventuallyValue value = default(TEventuallyValue);
            foreach (var item in events.OrderBy(o => o.Key))
            {
                value = ValueOpertion(value, item.Value.Value);
            }
            return value;
        }

        public TEventBase GetNewestEvent()
        {
            return events
                .OrderByDescending(o => o.Key)
                .Select(i => i.Value)
                .FirstOrDefault();
        }

        protected abstract TEventuallyValue ValueOpertion(TEventuallyValue value1, TEventValue value2);
    }
}