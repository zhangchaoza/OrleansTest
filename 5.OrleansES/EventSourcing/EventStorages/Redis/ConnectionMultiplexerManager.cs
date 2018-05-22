namespace EventSourcing.EventStorages.Redis
{
    using System;
    using StackExchange.Redis;

    public static class ConnectionMultiplexerManager
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect(InternalConfig.ConnetctionString);
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }
}