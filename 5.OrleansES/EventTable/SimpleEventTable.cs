using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace EventTable
{
    public class SimpleEventTable
    {
        private readonly ILogger logger;
        IDatabase db;

        public SimpleEventTable(ILogger logger)
        {
            this.logger = logger;
        }

        public Task Connect()
        {
            db = ConnectionMultiplexerManager.Connection.GetDatabase(db: -1);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<T>> ReadEventState<T>(string eventName)
        {
            var events = await db.HashGetAllAsync($"event_{eventName}");
            return events.Select(he => JsonConvert.DeserializeObject<T>(he.Value));
        }

        public async Task<bool> UpdateEventState<T>(string eventName, int expectedversion, IEnumerable<T> updates)
        {
            var events = updates
                .Select(i => new HashEntry(DateTimeOffset.Now.UtcTicks, JsonConvert.SerializeObject(i)))
                .ToArray();
            try
            {
                await db.HashSetAsync($"event_{eventName}", events);
                return true;
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "写入时间失败");
                return false;
            }
        }
    }
}