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
    using EventSourcing.EventStates;

    public class RedisEventTable : IEventTable
    {
        private readonly ILogger logger;
        IDatabase db;

        public RedisEventTable(ILogger logger)
        {
            this.logger = logger;
            db = ConnectionMultiplexerManager.Connection.GetDatabase(db: -1);
        }

        public async Task<IEnumerable<T>> ReadAllEvents<T>(string eventName)
        {
            var events = await db.SortedSetRangeByRankAsync(PrepareNotSavedSnapshotEventsRedisKey(eventName), 0, -1);
            return events.Select(he => JsonConvert.DeserializeObject<T>((string)he ?? ""));
        }

        public async Task<T> ReadNewestEvent<T>(string eventName, int version)
        {
            return (await db.SortedSetRangeByRankAsync(PrepareNotSavedSnapshotEventsRedisKey(eventName), 0, 1, Order.Descending))
                .Select(he => JsonConvert.DeserializeObject<T>((string)he ?? ""))
                .FirstOrDefault();
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
                logger.LogError(ex, "写入事件失败");
                return false;
            }
        }


        public async Task<IEnumerable<SimpleSnapshot<TValue>>> ReadAllSnapshots<TValue>(string eventName)
        {
            var events = await db.SortedSetRangeByRankAsync(PrepareSnapshotsRedisKey(eventName), 0, -1);
            return events.Select(he => JsonConvert.DeserializeObject<SimpleSnapshot<TValue>>((string)he ?? ""));
        }

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

        public Task<IEnumerable<T>> ReadNotSavedSnapshotEvents<T>(string eventName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ClearNotSavedSnapshotEvents(string eventName)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> ReadSavedSnapshotEvents<T>(string eventName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ApplySavedSnapshotEvents<T>(string eventName, int expectedversion, IEnumerable<T> updates)
        {
            throw new NotImplementedException();
        }

        public Task<SimpleSnapshot<TValue>> ReadNewestSnapshot<TValue>(string eventName, int version)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ApplySnapshot<T>(string eventName, int expectedversion, IEnumerable<T> updates)
        {
            throw new NotImplementedException();
        }

        // private int ReadHashFieldKey(RedisValue key)
        // {
        //     return BitConverter.ToInt32((byte[])key, 0);
        // }
    }
}