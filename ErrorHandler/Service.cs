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

namespace BookieBaher.ErrorHandler
{
    public class Service : FrameworkService
    {
        public static string Name = "Error Handler";

        protected override bool HasOutbound => false;

        protected override void ReadConfig(IConfiguration config)
        {
            inboundQueue = config.GetValue<string>("ErrorQueue");
            outboundQueue = config.GetValue<string>("ScrapeQueue");
        }

        protected override Task<bool> OnMessageRecieved(object sender, BasicDeliverEventArgs args)
        {
            if (args.BasicProperties.ContentType == null)
                throw new ArgumentNullException(nameof(args.BasicProperties.ContentType));

            if (args.BasicProperties.ContentType.Contains("request-") )
            {
                JSError error = args.Body.Decode<JSError>();
                string request = args.BasicProperties.ContentType.Replace("-error", "");

                if (error.Error.Contains("TimeoutError"))
                {
                    SendMessage(Message.Create(error.Request, request));
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(false);
        }
    }
}
