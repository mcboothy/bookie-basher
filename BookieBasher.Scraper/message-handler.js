'use strict';
var browser = require('./browser');
var flashScraper = require('./flashscores-scraper');
var wikiScraper = require('./wiki-scraper');

class MessageHandler {
    async init(chrome, queueOut, queueError) {
        this.queueOut = queueOut;
        this.queueError = queueError;
        await browser.init(chrome);

        //var request = {
        //    "SeasonId": 4,
        //    "CompetitionId": 1,
        //    "Year": "2020/2021",
        //    "Status": "Creating",
        //    "Competition": {
        //        "CompetitionId": 1,
        //        "Name": "Premier League",
        //        "FlashScoreUrl": "scotland/premiership",
        //        "SoccerWikiId": 28
        //    }
        //};

        //var teams = await flashScraper.scrapeFixtures(request);

        //wikiScraper.scrapeCompetition(request)
        //    .then((teams) => {
        //        console.log(teams);
        //    })
        //    .catch((err) => {
        //        console.error(err);
        //    });
    }

    async shutdown() {
        await browser.shutdown();
    }

    async onMessageRecieved(channel, msg) {
        try {
            var request = JSON.parse(msg.content.toString());

            switch (msg.properties.contentType) {
                case "request-teams": {
                    console.log(`scraping competition for ${request.Competition.DefaultAlias}`);
                    var wikiPromise = wikiScraper.scrapeTeams(request);
                    var fsFull = flashScraper.scrapeTeams(request, true);
                    var fsShort = flashScraper.scrapeTeams(request, false);

                    var results = await Promise.allSettled([wikiPromise, fsFull, fsShort]);

                    var data = Buffer.from(JSON.stringify({
                        Season: request.Season,
                        WikiTeams: results[0],
                        FSFullTeams: results[1],
                        FSShortTeams: results[2]
                    }));

                    this.sendReply(channel, "process-teams", data, msg);
                    break;
                }

                case "request-fixtures": {
                    console.log(`scraping fixtures for ${request.Competition.DefaultAlias}`);
                    await flashScraper.scrapeFixtures(request)
                        .then((fixtures) => {
                            var data = Buffer.from(JSON.stringify({
                                Season: request,
                                Matches: fixtures
                            }));
                            this.sendResults(channel, "process-fixtures", data, msg);
                        })
                        .catch((err) => {
                            console.error(err);
                            var data = Buffer.from(JSON.stringify({
                                Request: request,
                                Error: JSON.stringify(err)
                            }));
                            this.sendError(channel, data, msg);
                        });
                    break;
                }
                case "request-match": {
                    console.log(`scraping match for ${request.HomeTeam} vs ${request.AwayTeam}`);
                    flashScraper.scrapeMatch(request)
                        .then((result) => {
                            var data = Buffer.from(JSON.stringify({
                                Request: request,
                                MatchStats: result
                            }));
                            this.sendResults(channel, "process-match", data, msg);
                        })
                        .catch((err) => {
                            console.error(err);
                            var data = Buffer.from(JSON.stringify({
                                Request: request,
                                Error: JSON.stringify(err)
                            }));
                            this.sendError(channel, data, msg);
                        });
                    break;
                }
                case "request-standings": {
                    console.log(`scraping standings for ${request.Competition.DefaultAlias}`);
                    flashScraper.scrapeStandings(request)
                        .then((result) => {
                            var data = Buffer.from(JSON.stringify({
                                Season: request,
                                Leagues: result
                            }));
                            this.sendResults(channel, "process-standings", data, msg);
                        })
                        .catch((err) => {
                            console.error(err);
                            var data = Buffer.from(JSON.stringify({
                                Request: request,
                                Error: JSON.stringify(err)
                            }));
                            this.sendError(channel, data, msg);
                        });
                    break;
                }
                case "request-all-teams": {
                    console.log('scraping all teams');

                    var results = [];
                    for (const competition of request.Competitions) {
                        console.log(`scraping teams for ${competition.DefaultAlias}`);

                        var wiki = wikiScraper.scrapeTeams(competition);
                        var flash = flashScraper.scrapeTeams(competition);
                        var flashFull = flashScraper.scrapeTeams(competition, true);

                        await Promise.all([wiki, flash, flashFull])
                            .then((result) => {
                                results.push({
                                    Competition: competition,
                                    WikiTeams: result[0],
                                    FSShortTeams: result[1],
                                    FSFullTeams: result[2]
                                });                                
                            })
                            .catch((err) => {
                                console.error(err);
                                var data = Buffer.from(JSON.stringify({
                                    Request: request,
                                    Error: JSON.stringify(err)
                                }));
                                this.sendError(channel, data, msg);
                            });
                    }
                    var data = Buffer.from(JSON.stringify({
                        CompetitionTeams: results
                    }));
                    this.sendResults(channel, "process-all-teams", data, msg);
                    break;
                }
                default:
                    console.error("UNKNOWN MESSAGE : " + msg.properties.contentType);
                    channel.reject(msg, false);
                    break;
            }
        } catch (err) {
            console.error(err);
            var errorData = Buffer.from(JSON.stringify({
                Request: request,
                Error: JSON.stringify(err)
            }));
            this.sendError(channel, errorData, msg);
        }
    }

    sendReply(channel, type, data, msg) {
        var opts = { contentType: type };
        channel.sendToQueue(msg.properties.replyTo, data, opts);
        channel.ack(msg);
    }

    sendResults(channel, type, data, msg) {
        var opts = { contentType: type };
        channel.sendToQueue(this.queueOut, data, opts);
        channel.ack(msg);
    }

    sendError(channel, data, msg) {
        var opts = { contentType: "Fetch-Error" };
        channel.sendToQueue(this.queueError, data, opts);
        channel.reject(msg, false);// TODO - Requeue X times
    }
}

module.exports = new MessageHandler();