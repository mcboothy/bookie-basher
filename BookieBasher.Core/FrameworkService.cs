using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BookieBasher.Core
{
    public abstract class FrameworkService
    {
        protected ResilientConnection consumerConnection;
        protected ResilientConnection publisherConnection;
        protected Dictionary<string, IModel> consumers;
        protected bool dispatchConsumersAsync;
        protected IConfigurationRoot configuration;

        protected FrameworkService(bool isAsync = true)
        {
            consumers = new Dictionary<string, IModel>();
            dispatchConsumersAsync = isAsync;
            ReadConfig();
        }

        protected abstract bool HasOutbound { get; }

        public virtual void Start()
        {
            PolicyHelper.ApplyPolicy(() =>
            {
                if (!consumerConnection.IsConnected)
                    consumerConnection.TryConnect();
            });
        }

        public virtual void Stop()
        {
            if (consumers != null)
            {
                foreach(var pair in consumers)
                {
                    pair.Value.BasicCancel(pair.Key);
                    pair.Value.Dispose();
                }

                consumers.Clear();
            }
        }

        public void SendMessage(Message message, string queue, string exchange = "")
        {
            if (HasOutbound)
            {
                if (!publisherConnection.IsConnected)
                {
                    publisherConnection.TryConnect();
                }

                using (var channel = publisherConnection.CreateModel())
                {
                    PolicyHelper.ApplyPolicy(() =>
                    {
                        var properties = channel.CreateBasicProperties();
                        properties.ReplyTo = message.ReplyTo;
                        properties.Persistent = message.IsPersistant;
                        properties.ContentType = message.ContentType;
                        properties.Headers = message.Headers;
                        channel.BasicPublish(
                            exchange: exchange,
                            routingKey: queue,
                            mandatory: true,
                            basicProperties: properties,
                            body: message.Content.Encode());
                    });
                }
            }
        }

        protected virtual void ReadConfig(IConfiguration config)
        {
            // do nothing
        }

        protected virtual void CreateConsumers()
        {
            // do nothing
        }

        private void ReadConfig()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            string host = configuration.GetValue<string>("MQHost");
            int port = configuration.GetValue<int>("MQPort");
            string user = configuration.GetValue<string>("MQUsername");
            string password = configuration.GetValue<string>("MQPassword");

            var factory = new ConnectionFactory()
            {
                DispatchConsumersAsync = dispatchConsumersAsync,
                HostName = host,
                Port = port,
                UserName = user,
                Password = password
            };

            consumerConnection = new ResilientConnection(factory);
            consumerConnection.OnConnectionCreated += (s, e) => CreateConsumers();

            if (HasOutbound)
            {
                var outboundFactory = new ConnectionFactory()
                {
                    HostName = host,
                    Port = port,
                    UserName = user,
                    Password = password
                };

                publisherConnection = new ResilientConnection(outboundFactory);
            }

            ReadConfig(configuration);
        }
    }
}
