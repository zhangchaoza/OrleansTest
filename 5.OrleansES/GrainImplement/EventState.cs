namespace GrainImplement
{
    using System;
    using System.Collections.Generic;
    using GrainInterfaces;

    [Serializable]
    public class EventState
    {
        private SortedDictionary<DateTimeOffset, Change> changes;

        public EventState()
        {
            changes = new SortedDictionary<DateTimeOffset, Change>();
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