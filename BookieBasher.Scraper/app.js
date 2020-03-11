'use strict';
var fs = require('fs');
var amqp = require('amqplib/callback_api');
var scraper = require('./flashscores-scraper');
var config = JSON.parse(fs.readFileSync('./config.json', 'UTF-8'));

let url = "amqp://" + config.MQUsername + ":" + config.MQPassword + "@" + config.MQHost;

scraper.init(config.ChromePath).then(() => {

    amqp.connect(url, function (err, conn) {
        if (err) {
            console.error(err);
            return;
        }

        conn.createChannel(async function (err, channel) {
            if (err) {
                console.error(err);
                return;
            }

            channel.prefetch(config.PrefetchCount, true);

            channel.consume(config.InboundQueue, async function (msg) {
                try {
                    var request = JSON.parse(msg.content.toString());

                    switch (msg.properties.contentType) {
                        case "request-teams": {
                            scraper.scrapeTeams(request)
                                .then((teams) => {
                                    var data = Buffer.from(JSON.stringify({
                                        Season: request,
                                        Teams: teams
                                    }));
                                    sendResults(channel, "process-teams", data, msg);
                                }).catch((err) => {
                                    console.error(err);
                                    var data = Buffer.from(JSON.stringify({
                                        Request: request,
                                        Error: JSON.stringify(err)
                                    }));
                                    sendError(channel, data, msg);
                                });
                            break;
                        }
                        case "request-fixtures": {
                            scraper.scrapeFixtures(request)
                                .then((fixtures) => {
                                    var data = Buffer.from(JSON.stringify({
                                        Season: request,
                                        Matches: fixtures
                                    }));
                                    sendResults(channel, "process-fixtures", data, msg);
                                }).catch((err) => {
                                    console.error(err);
                                    var data = Buffer.from(JSON.stringify({
                                        Request: request,
                                        Error: JSON.stringify(err)
                                    }));
                                    sendError(channel, data, msg);
                                });
                            break;
                        }
                        case "request-match": {
                            scraper.scrapeMatch(request)
                                .then((result) => {
                                    var data = Buffer.from(JSON.stringify({
                                        Request: request,
                                        MatchStats: result
                                    }));
                                    sendResults(channel, "process-match", data, msg);
                                }).catch((err) => {
                                    console.error(err);
                                    var data = Buffer.from(JSON.stringify({
                                        Request: request,
                                        Error: JSON.stringify(err)
                                    }));
                                    sendError(channel, data, msg);
                                });
                            break;
                        }
                        case "request-standings": {
                            scraper.scrapeStandings(request)
                                .then((result) => {
                                    var data = Buffer.from(JSON.stringify({
                                        Season: request,
                                        Leagues: result
                                    }));
                                    sendResults(channel, "process-standings", data, msg);
                                }).catch((err) => {
                                    console.error(err);
                                    var data = Buffer.from(JSON.stringify({
                                        Request: request,
                                        Error: JSON.stringify(err)
                                    }));
                                    sendError(channel, data, msg);
                                });
                            break;
                        }
                        default:
                            console.error("UNKNOWN MESSAGE : " + msg.properties.contentType);
                            channel.reject(msg, false);
                            break;
                    }
                } catch (err) {
                    console.error(err);
                    var data = Buffer.from(JSON.stringify({
                        Request: request,
                        Error: JSON.stringify(err)
                    }));
                    sendError(channel, data, msg);
                }
                
            }, {
                noAck: false
            });

            process.on('exit', (code) => {
                channel.close();
            });
        });
    
        process.on('exit', (code) => {
            scraper.shutdown();
        });
    });
});

function sendResults(channel, type, data, msg) {
    var opts = { contentType: type };
    channel.sendToQueue(config.OutboundQueue, data, opts);
    channel.ack(msg);
}

function sendError(channel, data, msg) {
    var opts = { contentType: "Fetch-Error" };
    channel.sendToQueue(config.ErrorQueue, data, opts);
    channel.reject(msg, false);// TODO - Requeue X times
}