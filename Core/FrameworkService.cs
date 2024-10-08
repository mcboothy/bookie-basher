﻿using BookieBasher.Core.Database;
using BookieBasher.Core.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BookieBasher.Core
{
    public abstract class FrameworkService
    {
        protected ResilientConnection consumerConnection;
        protected ResilientConnection publisherConnection;
        protected IModel channel;
        protected string consumerTag;
        protected string errorQueue;
        protected string inboundQueue;
        protected string outboundQueue;
        protected string logQueue;
        protected string serviceName;
        protected DbContextOptions<BBDBContext> options;
        protected bool dispatchConsumersAsync;
        protected IConfiguration configuration;
              
        protected FrameworkService(bool isAsync = true)
        {
            var assemblyName = Assembly.GetCallingAssembly().FullName;
            serviceName = assemblyName.Substring(0, assemblyName.IndexOf(','));
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
            if (channel != null)
            {
                channel.BasicCancel(consumerTag);
                channel.Dispose();
            }
        }

        public void Log(string message)
        {
            var log = new Log()
            {
                Host = Environment.MachineName,
                Message = message,
                ServiceName = serviceName
            };

            SendMessage(Message.Create(log, "log-message"), logQueue);
        }

        public void SendMessage(Message message, string queue = null, string exchange = "")
        {
            PolicyHelper.ApplyPolicy(() =>
            {
                if (HasOutbound)
                {
                    if (!publisherConnection.IsConnected)
                    {
                        publisherConnection.TryConnect();
                    }

                    if (queue == null)
                        queue = outboundQueue;

                    using (var channel = publisherConnection.CreateModel())
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
                    }
                }
            });
        }

        protected virtual void CreateConsumers()
        {
            if (channel != null)
            {
                channel.BasicCancel(consumerTag);
                channel.Dispose();
            }

            channel = consumerConnection.CreateModel();
            channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                bool reject = true;

                try
                {
                    reject = !(await OnMessageRecieved(channel, ea));
                }
                catch (Exception ex)
                {
                    JSError error = new JSError()
                    {
                        Error = ex.ToDetailedString(),
                        ContentType = ea.BasicProperties.ContentType,
                        Request = Encoding.UTF8.GetString(ea.Body.ToArray())
                    };

                    SendMessage(Message.Create(error, "process-error"), errorQueue);
                }

                if (reject)
                    channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: false);
                else
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            consumerTag = channel.BasicConsume(
                queue: inboundQueue,
                autoAck: false,
                consumer: consumer);
        }

        private void ReadConfig()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            string connectionString = configuration.GetValue<string>("ConnectionString");
            var optionsBuilder = new DbContextOptionsBuilder<BBDBContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.FromString("10.4.12-mariadb"), (opts) => opts.EnableRetryOnFailure());

            logQueue = configuration.GetValue<string>("LogQueue");
            errorQueue = configuration.GetValue<string>("ErrorQueue");
            options = optionsBuilder.Options;

            var factory = new ConnectionFactory()
            {
                DispatchConsumersAsync = dispatchConsumersAsync,
                HostName = configuration.GetValue<string>("MQHost"),
                VirtualHost = configuration.GetValue<string>("MQVirtualHost"),
                Port = configuration.GetValue<int>("MQPort"),
                UserName = configuration.GetValue<string>("MQUsername"),
                Password = configuration.GetValue<string>("MQPassword")
            };

            consumerConnection = new ResilientConnection(factory);
            consumerConnection.OnConnectionCreated += (s, e) => CreateConsumers();

            if (HasOutbound)
            {
                var outboundFactory = new ConnectionFactory()
                {
                    HostName = configuration.GetValue<string>("MQHost"),
                    VirtualHost = configuration.GetValue<string>("MQVirtualHost"),
                    Port = configuration.GetValue<int>("MQPort"),
                    UserName = configuration.GetValue<string>("MQUsername"),
                    Password = configuration.GetValue<string>("MQPassword")
                };

                publisherConnection = new ResilientConnection(outboundFactory);
            }

            ReadConfig(configuration);
        }

        protected virtual void ReadConfig(IConfiguration config)
        {
            // do nothing
        }

        protected abstract Task<bool> OnMessageRecieved(object sender, BasicDeliverEventArgs args);
    }
}
