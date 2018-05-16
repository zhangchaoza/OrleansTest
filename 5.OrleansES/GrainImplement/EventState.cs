namespace GrainImplement
{
    using System;
    using System.Collections.Generic;
    using GrainInterfaces;
    using System.Linq;

    [Serializable]
    public class EventState
    {
        private SortedDictionary<DateTimeOffset, Change> changes;

        public EventState()
        {
            changes = new SortedDictionary<DateTimeOffset, Change>();
        }

        public EventState(IEnumerable<Change> events)
        {
            changes = new SortedDictionary<DateTimeOffset, Change>(events.ToDictionary(i => i.When));
        }

        public SortedDictionary<DateTimeOffset, Change> Changes => changes;

        public void Apply(Change change)
        {
            if (change == null)
                throw new ArgumentNullException("changes");

            if (this.changes.ContainsKey(change.When))
                return;

            this.changes.Add(change.When, change);
        }
    }
}