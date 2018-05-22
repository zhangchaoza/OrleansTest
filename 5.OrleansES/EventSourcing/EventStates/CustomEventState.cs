namespace EventSourcing.EventStates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EventSourcing;
    using EventSourcing.Abstractions;

    [Serializable]
    public abstract class CustomEventState<TEventBase> where TEventBase : IEventBase
    {
        public const int pageSize = 4;
        private SortedDictionary<DateTimeOffset, TEventBase> notSavedSnapshotChanges;
        private SortedDictionary<DateTimeOffset, SimpleSnapshot<double>> snapshots;
        private string eventName;
        private IEventTable table;

        public CustomEventState()
        {
            notSavedSnapshotChanges = new SortedDictionary<DateTimeOffset, TEventBase>();
            snapshots = new SortedDictionary<DateTimeOffset, SimpleSnapshot<double>>();
        }

        public void Init(string eventName, IEventTable table)
        {
            this.eventName = eventName;
            this.table = table;
        }

        public async Task<int> PlayAllEvents()
        {
            var allEvents = await table.ReadAllEvents<TEventBase>(eventName);
            var allEventsArray = allEvents.OrderBy(i => i.When).ToArray();
            var pageCount = allEventsArray.Length / pageSize;
            SimpleSnapshot<double> newSnapshot = null;
            for (int i = 0; i < pageCount; i++)
            {
                var page = new ArraySegment<TEventBase>(allEventsArray, i * pageSize, pageSize);
                newSnapshot = GenSnapshot(newSnapshot, page);
                snapshots.Add(newSnapshot.When, newSnapshot);
            }

            var newestPage = new ArraySegment<TEventBase>(allEventsArray, pageCount * pageSize, allEventsArray.Length % pageSize);
            foreach (var change in newestPage)
            {
                notSavedSnapshotChanges.Add(change.When, change);
            }
            return allEventsArray.Length;
        }

        public void Apply(TEventBase change)
        {
            if (change == null)
                throw new ArgumentNullException("changes");

            if (this.notSavedSnapshotChanges.ContainsKey(change.When))
                return;

            this.notSavedSnapshotChanges.Add(change.When, change);

            if (notSavedSnapshotChanges.Count >= pageSize)
            {
                var newsetSnapshot = snapshots
                    .OrderByDescending(o => o.Value.When)
                    .Select(i => i.Value)
                    .FirstOrDefault();
                var newSnapshot = GenSnapshot(newsetSnapshot, notSavedSnapshotChanges.OrderBy(o => o.Key).Select(i => i.Value));
                notSavedSnapshotChanges.Clear();
                snapshots.Add(newSnapshot.When, newSnapshot);
            }
        }

        public double GetCurrent()
        {
            var newsetSnapshot = snapshots
                .OrderByDescending(o => o.Value.When)
                .Select(i => i.Value)
                .FirstOrDefault();
            var newEvents = notSavedSnapshotChanges
                .OrderBy(o => o.Value.When)
                .Select(i => i.Value);
            return GenSnapshot(newsetSnapshot, newEvents).Value;
        }

        public async Task<TEventBase> GetNewestEvent(int version)
        {
            var newestChange = notSavedSnapshotChanges
                .OrderByDescending(o => o.Key)
                .Select(i => i.Value)
                .FirstOrDefault();

            //保存快照的事件中没有事件，需要在EventStore查找
            if (null == newestChange)
            {
                newestChange = await table.ReadNewestEvent<TEventBase>(eventName, version);
            }
            return newestChange;
        }

        public async Task<IReadOnlyList<TEventBase>> GetAllEvents()
        {
            var events = await table.ReadAllEvents<TEventBase>(eventName);
            return (IReadOnlyList<TEventBase>)events.OrderBy(i => i.When).ToList();
        }

        private SimpleSnapshot<double> GenSnapshot(SimpleSnapshot<double> lastSnapshot, IEnumerable<TEventBase> events)
        {
            double value = 0;
            DateTimeOffset when;
            if (null != lastSnapshot)
            {
                value = lastSnapshot.Value;
                when = lastSnapshot.When;
            }
            unchecked
            {
                foreach (var e in events)
                {
                    if (DateTimeOffset.Compare(e.When, when) < 0)//事件必须是新的或是同时的
                    {
                        continue;
                    }
                    value += e.Value;
                    when = e.When;
                }
                return new SimpleSnapshot<double>(value, when);
            }
        }

    }
}