namespace EventSourcing.EventStorages.Redis
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StackExchange.Redis;
    using System.Linq;
    using Newtonsoft.Json;
    using Microsoft.Extensions.Logging;
    using EventSourcing.EventModels;
    using EventSourcing.Abstractions;

    public partial class RedisEventTable : IEventTable
    {
        private readonly ILogger logger;
        IDatabase db;

        public RedisEventTable(ILogger logger)
        {
            this.logger = logger;
            db = ConnectionMultiplexerManager.Connection.GetDatabase(db: -1);
        }

        public async Task<IEnumerable<(T, int)>> ReadAllEvents<T>(string eventName)
        {
            var notSavedEvents = await ReadNotSavedSnapshotEvents<T>(eventName);
            var savedEvents = await ReadSavedSnapshotEvents<T>(eventName);
            return savedEvents.Concat(notSavedEvents);
        }

        public async Task<(T, int)> ReadNewestEvent<T>(string eventName)
        {
            var notSavedNewestEvent = (await db.SortedSetRangeByRankAsync(PrepareNotSavedSnapshotEventsRedisKey(eventName), 0, -1, Order.Descending)).FirstOrDefault();
            if (notSavedNewestEvent.IsNull)
            {
                var savedNewestEvent = (await db.SortedSetRangeByRankAsync(PrepareSavedSnapshotEventsRedisKey(eventName), 0, -1)).FirstOrDefault();
                if (savedNewestEvent.IsNull)
                {
                    return (default(T), 0);
                }
                else
                {
                    var score = (int)(await db.SortedSetScoreAsync(PrepareSavedSnapshotEventsRedisKey(eventName), savedNewestEvent));
                    return (JsonConvert.DeserializeObject<T>((string)savedNewestEvent ?? ""), score);
                }
            }
            else
            {
                var score = (int)(await db.SortedSetScoreAsync(PrepareSavedSnapshotEventsRedisKey(eventName), notSavedNewestEvent));
                return (JsonConvert.DeserializeObject<T>((string)notSavedNewestEvent ?? ""), score);
            }
        }

        public Task<IEnumerable<(T, int)>> ReadNotSavedSnapshotEvents<T>(string eventName)
        {
            var notSavedEvents = db
                .SortedSetScan(PrepareNotSavedSnapshotEventsRedisKey(eventName))
                .Select(he => (JsonConvert.DeserializeObject<T>((string)he.Element ?? ""), (int)he.Score));
            return Task.FromResult(notSavedEvents);
        }

        public async Task<bool> ApplyNotSavedSnapshotEvents<T>(string eventName, int expectedversion, IEnumerable<T> updates)
        {
            var events = updates
                .Select((e, i) => db.SortedSetAddAsync(PrepareNotSavedSnapshotEventsRedisKey(eventName), JsonConvert.SerializeObject(e), expectedversion + i));
            try
            {
                var res = await Task.WhenAll(events);
                return res.Count(i => !i) == 0;
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "写入未保存快照事件失败");
                return false;
            }
        }

        public Task<bool> ClearNotSavedSnapshotEvents(string eventName)
        {
            return db.KeyDeleteAsync(PrepareNotSavedSnapshotEventsRedisKey(eventName));
        }

        public Task<IEnumerable<(T, int)>> ReadSavedSnapshotEvents<T>(string eventName)
        {
            var savedEvents = db
                .SortedSetScan(PrepareSavedSnapshotEventsRedisKey(eventName))
                .Select(he => (JsonConvert.DeserializeObject<T>((string)he.Element ?? ""), (int)he.Score));
            return Task.FromResult(savedEvents);
        }

        public async Task<bool> ApplySavedSnapshotEvents<TEvent>(string eventName, SortedDictionary<int, TEvent> updates)
        {
            var events = updates
                .Select((e, i) => db.SortedSetAddAsync(PrepareSavedSnapshotEventsRedisKey(eventName), JsonConvert.SerializeObject(e.Value), e.Key));
            try
            {
                var res = await Task.WhenAll(events);
                return res.Count(i => !i) == 0;
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "写入已保存快照事件失败");
                return false;
            }
        }

        public Task<IEnumerable<(ISnopshot<TEventuallyValue> SnopShot, int Version)>> ReadAllSnapshots<TSnopshot, TEventuallyValue>(string eventName) where TSnopshot : ISnopshot<TEventuallyValue>
        {
            var events = db
                .SortedSetScan(PrepareSnapshotsRedisKey(eventName))
                .Select(he => ((ISnopshot<TEventuallyValue>)JsonConvert.DeserializeObject<TSnopshot>((string)he.Element ?? ""), (int)he.Score));
            return Task.FromResult(events);
        }

        public async Task<(ISnopshot<TEventuallyValue> SnopShot, int Version)> ReadNewestSnapshot<TEventuallyValue>(string eventName, int version)
        {
            var snopshot = (await db.SortedSetRangeByRankAsync(PrepareNotSavedSnapshotEventsRedisKey(eventName), 0, -1, Order.Descending))
                // .Select(he => JsonConvert.DeserializeObject<SimpleSnapshot<TEventuallyValue>>((string)he ?? ""))
                .FirstOrDefault();
            var score = (int)(await db.SortedSetScoreAsync(PrepareSavedSnapshotEventsRedisKey(eventName), snopshot));
            return (JsonConvert.DeserializeObject<SimpleSnapshot<TEventuallyValue>>((string)snopshot), score);
        }

        public async Task<bool> ApplySnapshot<TEventuallyValue>(string eventName, int expectedversion, ISnopshot<TEventuallyValue> newSnopshot)
        {
            try
            {
                return await db.SortedSetAddAsync(PrepareSnapshotsRedisKey(eventName), JsonConvert.SerializeObject(newSnopshot), expectedversion);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "保存快照失败");
                return false;
            }
        }

    }

    public partial class RedisEventTable
    {
        private RedisKey PrepareNotSavedSnapshotEventsRedisKey(string eventName)
        {
            return $"es_{eventName}_NotSavedSnapshotEvents";
        }

        private RedisKey PrepareSavedSnapshotEventsRedisKey(string eventName)
        {
            return $"es_{eventName}_SavedSnapshotEvents";
        }

        private RedisKey PrepareSnapshotsRedisKey(string eventName)
        {
            return $"es_{eventName}_Snapshots";
        }
    }
}