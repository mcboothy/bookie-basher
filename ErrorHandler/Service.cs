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

        protected override bool HasOutbound => true;

        protected override void ReadConfig(IConfiguration config)
        {
            inboundQueue = config.GetValue<string>("ErrorQueue");
            outboundQueue = config.GetValue<string>("ScrapeQueue");
        }

        protected override async Task<bool> OnMessageRecieved(object sender, BasicDeliverEventArgs args)
        {
            try
            {
                if (args.BasicProperties.ContentType == null)
                    throw new ArgumentNullException(nameof(args.BasicProperties.ContentType));

                JSError error = args.Body.Decode<JSError>();

                Log($"Processing error for {args.BasicProperties.ContentType}");

                if (args.BasicProperties.ContentType.Contains("request-"))
                {
                    string request = args.BasicProperties.ContentType.Replace("-error", "");

                    if (error.Error.Contains("TimeoutError"))
                    {
                        Log($"Retrying due to timeout {request}");
                        SendMessage(Message.Create(error.Request, request));
                    }
                }

                using (BBDBContext context = new BBDBContext(options))
                {
                    context.Errors.Add(new Error()
                    {
                        Message = error.Error,
                        Request = error.Request,
                        ContentType = error.ContentType
                    });

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Log($"Exception occurred while processing {args.BasicProperties.ContentType}\n{ex.ToDetailedString()}");
            }

            return true;
        }
    }
}
