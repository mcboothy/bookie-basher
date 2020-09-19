using BookieBaher.SeasonUpdater.IO;
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

namespace BookieBaher.TeamUpdater
{
    public class Service : FrameworkService
    {
        public static string Name = "Team Updater";

        protected override bool HasOutbound => true;

        protected override void ReadConfig(IConfiguration config)
        {
            inboundQueue = config.GetValue<string>("TeamQueue");
            outboundQueue = config.GetValue<string>("UpdateQueue");
        }

        protected override async Task<bool> OnMessageRecieved(object sender, BasicDeliverEventArgs args)
        {
            if (args.BasicProperties.ContentType == null)
                throw new ArgumentNullException(nameof(args.BasicProperties.ContentType));

            switch (args.BasicProperties.ContentType)
            {
                case "process-teams":
                    await InsertTeams(args.Body.Decode<JSCompetitionTeams>());
                    return true;

                default:
                    throw new ArgumentException($"Invalid content type {args.BasicProperties.ContentType}");
            }
        }

        private async Task InsertTeams(JSCompetitionTeams result)
        {
            using (BBDBContext context = new BBDBContext(options))
            {
                var ut = await context.UnknownTeams.FirstOrDefaultAsync(ut => ut.SeasonId == result.Season.SeasonId);

                if (ut == null)
                    throw new Exception("No season in unknown teams");

                var responce = new JSTeam[3][]
                {
                    result.WikiTeams.ToArray(),
                    result.FSShortTeams.ToArray(),
                    result.FSFullTeams.ToArray(),
                };

                ut.TeamsResponce = responce.ToJSON();

                context.UnknownTeams.Update(ut);
                await context.SaveChangesAsync();
            }
        }
    }
}
