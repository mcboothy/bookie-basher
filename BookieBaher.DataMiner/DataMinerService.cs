using BookieBaher.DataMiner.IO;
using BookieBasher.Core;
using BookieBasher.Core.Database;
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

namespace BookieBaher.DataMiner
{
    public class DataMinerService : FrameworkService
    {
        private DbContextOptions<BBDBContext> options;
        private string inQueue;
        private string outQueue;
        private string errorQueue;

        protected override bool HasOutbound => true;

        protected override void ReadConfig(IConfiguration config)
        {
            string connectionString = config.GetValue<string>("ConnectionString");
            var optionsBuilder = new DbContextOptionsBuilder<BBDBContext>();
            optionsBuilder.UseMySql(connectionString);

            options = optionsBuilder.Options;
            inQueue = config.GetValue<string>("InboundQueue");
            outQueue = config.GetValue<string>("OutboundQueue");
            errorQueue = config.GetValue<string>("ErrorQueue");

            //using (BBDBContext context = new BBDBContext(options))
            //{
            //    context.Database.EnsureDeleted();
            //    context.Database.EnsureCreated();

            //    string viewSQL = "CREATE OR REPLACE VIEW LeagueTeams AS\n" +
            //                     "SELECT DISTINCT Matches.HomeTeamID AS TeamID\n" +
            //                     "				 ,Matches.SeasonID AS SeasonID\n" +
            //                     "                 ,Team.Name AS Name\n" +
            //                     "                 ,Team.LogoURL AS LogoURL\n" +
            //                     "FROM bookie_basher.match AS Matches\n" +
            //                     "JOIN Team ON Team.TeamID = Matches.HomeTeamID\n" +
            //                     "UNION\n" +
            //                     "SELECT DISTINCT Matches.AwayTeamID AS TeamID\n" +
            //                     "				 ,Matches.SeasonID AS SeasonID\n" +
            //                     "                 ,Team.Name AS Name\n" +
            //                     "                 ,Team.LogoURL AS LogoURL\n" +
            //                     "FROM bookie_basher.match AS Matches\n" +
            //                     "JOIN Team ON Team.TeamID = Matches.AwayTeamID";

            //    context.Database.ExecuteSqlRawAsync(viewSQL).Wait();

            //    viewSQL = "CREATE OR REPLACE VIEW Fixtures AS\n" +
            //              "SELECT `Match`.MatchID\n" +
            //              "	  ,`Match`.SeasonID\n" +
            //              "      ,`HomeTeam`.Name AS HomeTeam\n" +
            //              "      ,`HomeTeam`.TeamID AS HomeTeamID\n" +
            //              "      ,`AwayTeam`.Name AS AwayTeam\n" +
            //              "      ,`AwayTeam`.TeamID AS AwayTeamID\n" +
            //              "      ,`Match`.HomeTeamStatsID\n" +
            //              "      ,`Match`.AwayTeamStatsID\n" +
            //              "      ,`Match`.DateTime\n" +
            //              "      ,`Match`.Status\n" +
            //              "FROM `Match`\n" +
            //              "JOIN `Team` AS HomeTeam ON HomeTeam.TeamID = HomeTeamID\n" +
            //              "JOIN `Team` AS AwayTeam ON AwayTeam.TeamID = AwayTeamID\n" +
            //              "WHERE Status = 'Fixture'";

            //    context.Database.ExecuteSqlRawAsync(viewSQL).Wait();

            //    context.Competition.Add(new Competition()
            //    {
            //        Name = "Premier League",
            //        Url = "england/premier-league"
            //    });
            //    context.Competition.Add(new Competition()
            //    {
            //        Name = "Championship",
            //        Url = "england/championship"
            //    });
            //    context.Competition.Add(new Competition()
            //    {
            //        Name = "League One",
            //        Url = "england/league-one"
            //    });
            //    context.Competition.Add(new Competition()
            //    {
            //        Name = "Ligue 1",
            //        Url = "france/ligue-1"
            //    });
            //    context.Competition.Add(new Competition()
            //    {
            //        Name = "Bundesliga",
            //        Url = "germany/bundesliga"
            //    });
            //    context.Competition.Add(new Competition()
            //    {
            //        Name = "Serie A",
            //        Url = "italy/serie-a"
            //    });
            //    context.Competition.Add(new Competition()
            //    {
            //        Name = "Eredivisie",
            //        Url = "netherlands/eredivisie"
            //    });
            //    context.Competition.Add(new Competition()
            //    {
            //        Name = "Laliga",
            //        Url = "spain/laliga"
            //    });
            //    context.SaveChanges();
            //}
        }

        protected override void CreateConsumers()
        {
            IModel channel = consumerConnection.CreateModel();
            channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                await OnMessageRecieved(channel, ea);
            };

            string consumerTag = channel.BasicConsume(
                queue: inQueue,
                autoAck: false,
                consumer: consumer);

            consumers.Add(consumerTag, channel);
        }

        private async Task OnMessageRecieved(object sender, BasicDeliverEventArgs args)
        {
            bool reject = true;

            try
            {
                if (args.BasicProperties.ContentType != null)
                {
                    switch (args.BasicProperties.ContentType)
                    {
                        case "update":
                        {
                            Console.WriteLine("Updating Seasons....");
                            await SeasonUpdater.UpdateSeasons(options, this, outQueue);
                            reject = false;
                            break;
                        }

                        case "update-season":
                        {
                            JSSeason result = args.Body.Decode<JSSeason>();
                            Console.WriteLine($"Updating season {result.Competition.Name}");
                            await SeasonUpdater.UpdateSeason(options, result, this, outQueue);
                            reject = false;
                            break;
                        }

                        case "update-all-averages":
                        {
                            Console.WriteLine("Updating All Stats....");
                            await SeasonUpdater.UpdateAllAverages(options, this, inQueue);
                            reject = false;
                            break;
                        }

                        case "update-averages":
                        {
                            JSSeason result = args.Body.Decode<JSSeason>();
                            Console.WriteLine($"Updating averages for season {result.Competition.Name}.");
                            await SeasonUpdater.UpdateAverages(options, result);
                            reject = false;
                            break;
                        }

                        case "process-teams":
                        {
                            JSProcessTeams result = args.Body.Decode<JSProcessTeams>();
                            Console.WriteLine($"Inserting teams for season {result.Season.Competition.Name}.");
                            await TeamUpdater.UpdateTeams(options, result, this, outQueue);
                            reject = false;
                            break;
                        }

                        case "process-fixtures":
                        {
                            JSProcessMatches result = args.Body.Decode<JSProcessMatches>();
                            Console.WriteLine($"Inserting fixtures for season {result.Season.Competition.Name}.");
                            await MatchUpdater.InsertFixtures(options, result, this, inQueue);
                            reject = false;
                            break;
                        }

                        case "process-match":
                        {
                            JSProcessMatch result = args.Body.Decode<JSProcessMatch>();
                            Console.WriteLine($"Inserting match stats for match {result.Request.MatchId}.");
                            await MatchUpdater.UpdateMatch(options, result, this, inQueue);                
                            reject = false;
                            break;
                        }

                        //case "process-standings":
                        //{
                        //    JSProcessStandings result = args.Body.Decode<JSProcessStandings>();
                        //    Console.WriteLine($"Updating form and league positions for {result.Season.Competition.Name}.");
                        //    await SeasonUpdater.UpdateStandings(options, result);
                        //    reject = false;
                        //    break;
                        //}
                    }
                }
                else
                { 
                    if (args.Body != null)
                    {
                        Console.WriteLine("UNKNOWN MESSAGE = " + Encoding.UTF8.GetString(args.Body));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR -  {ex.Message}");
                SendMessage(Message.Create(ex, "process-error"), errorQueue);
            }

            if (reject)
                ((IModel)sender).BasicReject(deliveryTag: args.DeliveryTag, requeue: false);
            else
                ((IModel)sender).BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
        }
    }
}
