'use strict';
var browser = require('./browser');
var os = require("os");
var flashScraper = require('./flashscores-scraper');
var wikiScraper = require('./wiki-scraper');

class MessageHandler {
    async init(chrome, matchQueue, teamQueue, updateQueue, errorQueue, logQueue) {
        this.matchQueue = matchQueue;
        this.updateQueue = updateQueue;
        this.errorQueue = errorQueue;
        this.teamQueue = teamQueue;
        this.logQueue = logQueue;
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
        var request = msg.content;

        try {     
            request = JSON.parse(msg.content.toString());

            switch (msg.properties.contentType) {
                case "request-teams": {
                    this.log(channel, `scraping competition for ${request.Competition.DefaultAlias}`);
                    var wikiPromise = wikiScraper.scrapeTeams(request);
                    var fsFull = flashScraper.scrapeTeams(request, true);
                    var fsShort = flashScraper.scrapeTeams(request, false);

                    Promise.allSettled([wikiPromise, fsFull, fsShort])
                        .then((results) => {
                            var data = Buffer.from(JSON.stringify({
                                Season: request.Season,
                                WikiTeams: results[0],
                                FSFullTeams: results[1],
                                FSShortTeams: results[2]
                            }));

                            this.sendResults(channel, this.teamQueue, "process-teams", data, msg);
                        })
                        .catch((err) => {
                            this.sendError(channel, 'request-teams-error', request, err, msg);
                        });
                    break;
                }

                case "request-fixtures": {
                    this.log(channel, `scraping fixtures for ${request.Competition.DefaultAlias}`);
                    await flashScraper.scrapeFixtures(request)
                        .then((fixtures) => {
                            var data = Buffer.from(JSON.stringify({
                                Season: request,
                                Matches: fixtures
                            }));
                            this.sendResults(channel, this.matchQueue, "process-fixtures", data, msg);
                        })
                        .catch((err) => {
                            this.sendError(channel, 'request-fixtures-error', request, err, msg);
                        });
                    break;
                }
                case "request-match": {
                    this.log(channel, `scraping match for ${request.HomeTeam} vs ${request.AwayTeam}`);
                    flashScraper.scrapeMatch(request)
                        .then((result) => {
                            var data = Buffer.from(JSON.stringify({
                                Request: request,
                                MatchStats: result
                            }));
                            this.sendResults(channel, this.matchQueue, "process-match", data, msg);
                        })
                        .catch((err) => {
                            this.sendError(channel, 'request-match-error', request, err, msg);
                        });
                    break;
                }
                case "request-standings": {
                    this.log(channel, `scraping standings for ${request.Competition.DefaultAlias}`);
                    flashScraper.scrapeStandings(request)
                        .then((result) => {
                            var data = Buffer.from(JSON.stringify({
                                Season: request,
                                Leagues: result
                            }));
                            this.sendResults(channel, "process-standings", data, msg);
                        })
                        .catch((err) => {
                            this.sendError(channel, 'request-standings-error', request, err, msg);
                        });
                    break;
                }
                case "request-all-teams": {
                    this.log(channel, 'scraping all teams');

                    var results = [];
                    for (const competition of request.Competitions) {
                        this.log(channel, `scraping teams for ${competition.DefaultAlias}`);

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
                                this.sendError(channel, 'request-all-teams-error', request, err, msg);
                            });
                    }
                    var data = Buffer.from(JSON.stringify({
                        CompetitionTeams: results
                    }));
                    this.sendReply(channel, "process-all-teams", data, msg);
                    break;
                }
                default:
                    this.log(channel, "UNKNOWN MESSAGE : " + msg.properties.contentType);
                    channel.reject(msg, false);
                    break;
            }
        } catch (err) {
            this.sendError(channel, 'scraper-unknown', request, err, msg);
        }
    }

    sendReply(channel, type, data, msg) {
        var opts = { contentType: type };
        channel.sendToQueue(msg.properties.replyTo, data, opts);
        channel.ack(msg);
    }

    sendResults(channel, queue, type, data, msg) {
        var opts = { contentType: type };
        channel.sendToQueue(queue, data, opts);
        channel.ack(msg);
    }

    sendError(channel, type, request, err, msg) {
        this.log(channel, JSON.stringify(err));
        var opts = { contentType: type };
        var data = Buffer.from(JSON.stringify({
            Request: JSON.stringify(request),
            Error: JSON.stringify(err)
        }));
        channel.sendToQueue(this.errorQueue, data, opts);
        channel.reject(msg, false);// TODO - Requeue X times
    }

    log(channel, msg) {
        var opts = { contentType: 'log-message' };
        var hostname = os.hostname();
        var data = Buffer.from(JSON.stringify({
            Message: msg,
            Host: hostname,
            ServiceName: 'BookieBasher.Scraper'
        }));
        channel.sendToQueue(this.logQueue, data, opts);
    }
}

module.exports = new MessageHandler();