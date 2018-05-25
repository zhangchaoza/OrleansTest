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

namespace EventSourcing
{
    public interface IEventTable
    {

        Task<IEnumerable<(TEvent Event, int Version)>> ReadAllEvents<TEvent>(string eventName);

        Task<(TEvent Event, int Version)> ReadNewestEvent<TEvent>(string eventName);

        Task<IEnumerable<(TEvent Event, int Version)>> ReadNotSavedSnapshotEvents<TEvent>(string eventName);

        Task<bool> ApplyNotSavedSnapshotEvents<TEvent>(string eventName, int expectedversion, IEnumerable<TEvent> updates);

        Task<bool> ClearNotSavedSnapshotEvents(string eventName);

        Task<IEnumerable<(TEvent Event, int Version)>> ReadSavedSnapshotEvents<TEvent>(string eventName);

        Task<bool> ApplySavedSnapshotEvents<TEvent>(string eventName, SortedDictionary<int, TEvent> updates);

        Task<IEnumerable<(ISnopshot<TEventuallyValue> SnopShot, int Version)>> ReadAllSnapshots<TSnopshot, TEventuallyValue>(string eventName) where TSnopshot : ISnopshot<TEventuallyValue>;

        Task<(ISnopshot<TEventuallyValue> SnopShot, int Version)> ReadNewestSnapshot<TEventuallyValue>(string eventName, int version);

        Task<bool> ApplySnapshot<TEventuallyValue>(string eventName, int expectedversion, ISnopshot<TEventuallyValue> newSnopshot);

    }
}