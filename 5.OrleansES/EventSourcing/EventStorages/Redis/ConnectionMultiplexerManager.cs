namespace EventSourcing.EventStorages.Redis
{
    using System;
    using StackExchange.Redis;

    public static class ConnectionMultiplexerManager
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect("10.0.113.35:6379,password=admin@2023");
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