﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace BookieBasher.Core
{
    public class ResilientConnection
    {
        private readonly IConnectionFactory connectionFactory;
        private IConnection connection;
        private object syncObject = new object();
        private bool disposed;

        public ResilientConnection(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public event EventHandler OnConnectionCreated;

        public bool IsConnected
        {
            get { return connection != null && 
                         connection.IsOpen && 
                         !disposed; }
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Not connected");
            }

            return connection.CreateModel();
        }

        public void Dispose()
        {
            if (disposed) return;

            disposed = true;
            connection.Dispose();
        }

        public bool TryConnect()
        {
            lock (syncObject)
            {
                connection = connectionFactory.CreateConnection();

                if (IsConnected)
                {
                    connection.ConnectionShutdown += OnConnectionShutdown;
                    connection.CallbackException += OnCallbackException;
                    connection.ConnectionBlocked += OnConnectionBlocked;
                    OnConnectionCreated?.Invoke(this, EventArgs.Empty);

                    return true;
                }

                return false;
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            Console.WriteLine($"Connection Blocked - {e.Reason}");
            if (!disposed)
                TryConnect();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            Console.WriteLine($"Callback Exception - {e.Exception.Message}");
            if (!disposed)
                TryConnect();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine($"Connection Shutedown - {e.Cause}");
            if (!disposed)
                TryConnect();
        }
    }
}
