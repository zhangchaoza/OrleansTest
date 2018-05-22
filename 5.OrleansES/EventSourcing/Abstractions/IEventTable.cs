using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace EventSourcing
{
    public interface IEventTable
    {

        Task<IEnumerable<T>> ReadAllEvents<T>(string eventName);

        Task<bool> UpdateEvents<T>(string eventName, int expectedversion, IEnumerable<T> updates);

        Task<T> ReadNewestEvent<T>(string eventName, int version);

    }
}