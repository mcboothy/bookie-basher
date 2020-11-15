using BookieBasher.Core;
using BookieBasher.Core.Database;
using BookieBasher.Core.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookieBaher.SeasonUpdater
{
    public class Service : FrameworkService
    {
        public static string Name = "Log Processor";

        protected override bool HasOutbound => true;

        public Service()
        {
            ServiceName = Name;
        }

        protected override async Task<bool> OnMessageRecieved(object sender, BasicDeliverEventArgs args)
        {
            if (args.BasicProperties.ContentType == null)
                throw new ArgumentNullException(nameof(args.BasicProperties.ContentType));

            switch (args.BasicProperties.ContentType)
            {
                case "log-message":
                    using (BBDBContext context = new BBDBContext(options))
                    {
                        context.Log.Add(args.Body.Decode<Log>());
                        await context.SaveChangesAsync();
                    }
                    return true;

                default:
                    throw new ArgumentException($"Invalid content type {args.BasicProperties.ContentType}");
            }
        }
    }
}
