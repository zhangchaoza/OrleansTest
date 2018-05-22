using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using EventSourcing.EventStates;

namespace EventSourcing
{
    public interface IEventTable
    {

        Task<IEnumerable<T>> ReadAllEvents<T>(string eventName);

        Task<T> ReadNewestEvent<T>(string eventName, int version);

        Task<IEnumerable<T>> ReadNotSavedSnapshotEvents<T>(string eventName);

        Task<bool> ApplyNotSavedSnapshotEvents<T>(string eventName, int expectedversion, IEnumerable<T> updates);

        Task<bool> ClearNotSavedSnapshotEvents(string eventName);

        Task<IEnumerable<T>> ReadSavedSnapshotEvents<T>(string eventName);

        Task<bool> ApplySavedSnapshotEvents<T>(string eventName, int expectedversion, IEnumerable<T> updates);

        Task<IEnumerable<SimpleSnapshot<TValue>>> ReadAllSnapshots<TValue>(string eventName);

        Task<SimpleSnapshot<TValue>> ReadNewestSnapshot<TValue>(string eventName, int version);

        Task<bool> ApplySnapshot<T>(string eventName, int expectedversion, IEnumerable<T> updates);

    }
}