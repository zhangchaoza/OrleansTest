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
            var events = await db.HashGetAllAsync(PrepareRedisKey(eventName));
            return events.Select(he => JsonConvert.DeserializeObject<T>((string)he.Value ?? ""));
        }

        public async Task<bool> UpdateEvents<T>(string eventName, int expectedversion, IEnumerable<T> updates)
        {
            var events = updates
                .Select((e, i) => new HashEntry(PrepareHashFieldKey(expectedversion + i), JsonConvert.SerializeObject(e)))
                .ToArray();
            try
            {
                await db.HashSetAsync(PrepareRedisKey(eventName), events);
                return true;
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "写入时间失败");
                return false;
            }
        }

        public async Task<T> ReadNewestEvent<T>(string eventName, int version)
        {
            string eventJson = await db.HashGetAsync(PrepareRedisKey(eventName), PrepareHashFieldKey(version));
            return JsonConvert.DeserializeObject<T>(eventJson ?? "");
        }

        private RedisKey PrepareRedisKey(string eventName)
        {
            return $"event_{eventName}";
        }

        private RedisValue PrepareHashFieldKey(int key)
        {
            return key;
            // return BitConverter.GetBytes(key);
        }

        // private int ReadHashFieldKey(RedisValue key)
        // {
        //     return BitConverter.ToInt32((byte[])key, 0);
        // }
    }
}