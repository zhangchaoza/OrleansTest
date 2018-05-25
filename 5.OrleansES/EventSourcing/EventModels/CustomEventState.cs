namespace EventSourcing.EventModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EventSourcing;
    using EventSourcing.Abstractions;

    [Serializable]
    public abstract class CustomEventState<TEventBase, TEventValue, TEventuallyValue> where TEventBase : IEvent<TEventValue>
    {
        public const int pageSize = 4;
        private SortedDictionary<int, TEventBase> notSavedSnapshotChanges;
        private SortedDictionary<int, ISnopshot<TEventuallyValue>> snapshots;
        private string eventName;
        private IEventTable table;

        public CustomEventState()
        {
            notSavedSnapshotChanges = new SortedDictionary<int, TEventBase>();
            snapshots = new SortedDictionary<int, ISnopshot<TEventuallyValue>>();
        }

        public void Init(string eventName, IEventTable table)
        {
            this.eventName = eventName;
            this.table = table;
        }

        // [Obsolete("", true)]
        // public async Task<int> PlayAllEvents()
        // {
        //     var allEvents = await table.ReadAllEvents<TEventBase>(eventName);
        //     var allEventsArray = allEvents.OrderBy(i => i.When).ToArray();
        //     var pageCount = allEventsArray.Length / pageSize;
        //     ISnopshot<TEventuallyValue> newSnapshot = null;
        //     for (int i = 0; i < pageCount; i++)
        //     {
        //         var page = new ArraySegment<TEventBase>(allEventsArray, i * pageSize, pageSize);
        //         newSnapshot = GenSnapshot(newSnapshot, page);
        //         snapshots.Add(newSnapshot.When, newSnapshot);
        //     }

        //     var newestPage = new ArraySegment<TEventBase>(allEventsArray, pageCount * pageSize, allEventsArray.Length % pageSize);
        //     foreach (var change in newestPage)
        //     {
        //         notSavedSnapshotChanges.Add(change.When, change);
        //     }
        //     return allEventsArray.Length;
        // }

        public async Task<int> ReadFromStorage()
        {
            var notSavedEventsList = (await table.ReadNotSavedSnapshotEvents<TEventBase>(eventName)).ToList();
            var snopshotsList = (await table.ReadAllSnapshots<SimpleSnapshot<TEventuallyValue>, TEventuallyValue>(eventName)).ToList();

            notSavedSnapshotChanges = new SortedDictionary<int, TEventBase>(notSavedEventsList.ToDictionary(i => i.Version, i => i.Event));
            snapshots = new SortedDictionary<int, ISnopshot<TEventuallyValue>>(snopshotsList.ToDictionary(i => i.Version, i => i.SnopShot));

            var newestNotSavedEvent = notSavedEventsList.FirstOrDefault();
            if (null == newestNotSavedEvent.Event)
            {
                return snopshotsList.FirstOrDefault().Version;
            }
            else
            {
                return newestNotSavedEvent.Version;
            }
        }

        public void Apply(TEventBase change, int version)
        {
            if (change == null)
                throw new ArgumentNullException("changes");

            if (this.notSavedSnapshotChanges.ContainsKey(version))
                return;

            this.notSavedSnapshotChanges.Add(version, change);

            if (notSavedSnapshotChanges.Count >= pageSize)
            {
                var newsetSnapshot = snapshots
                    .OrderByDescending(o => o.Value.When)
                    .Select(i => i.Value)
                    .FirstOrDefault();
                var newSnapshot = GenSnapshot(newsetSnapshot, notSavedSnapshotChanges.OrderBy(o => o.Key).Select(i => i.Value));

                Task.WhenAll(
                    table.ApplySavedSnapshotEvents(eventName, notSavedSnapshotChanges),
                    table.ClearNotSavedSnapshotEvents(eventName),
                    table.ApplySnapshot(eventName, version, newSnapshot));

                notSavedSnapshotChanges.Clear();
                snapshots.Add(version, newSnapshot);
            }
        }

        public TEventuallyValue GetCurrent()
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

        public async Task<TEventBase> GetNewestEvent()
        {
            var newestChange = notSavedSnapshotChanges
                .OrderByDescending(o => o.Key)
                .Select(i => i.Value)
                .FirstOrDefault();

            //保存快照的事件中没有事件，需要在EventStore查找
            if (null == newestChange)
            {
                newestChange = (await table.ReadNewestEvent<TEventBase>(eventName)).Event;
            }
            return newestChange;
        }

        public async Task<IReadOnlyList<TEventBase>> GetAllEvents()
        {
            var events = await table.ReadAllEvents<TEventBase>(eventName);
            return (IReadOnlyList<TEventBase>)events.Select(i => i.Event).OrderBy(i => i.When).ToList();
        }

        private ISnopshot<TEventuallyValue> GenSnapshot(ISnopshot<TEventuallyValue> lastSnapshot, IEnumerable<TEventBase> events)
        {
            TEventuallyValue value = default(TEventuallyValue);
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
                    value = ValueOpertion(value, (TEventValue)e.Value);
                    when = e.When;
                }
                return new SimpleSnapshot<TEventuallyValue>(value, when);
            }
        }

        protected abstract TEventuallyValue ValueOpertion(TEventuallyValue value1, TEventValue value2);
    }
}