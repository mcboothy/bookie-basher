'use strict';
var fs = require('fs');
var amqp = require('amqplib');
var handler = require('./message-handler');
var config;

if (fs.existsSync('./config.json')) {
    config = JSON.parse(fs.readFileSync('./config.json', 'UTF-8'));
} else {
    config = JSON.parse(fs.readFileSync('../config.json', 'UTF-8'));
}

const username = process.env.MQUsername || config.MQUsername;
const password = process.env.MQPassword || config.MQPassword;
const host = process.env.MQHost || config.MQHost;
const port = process.env.MQPort || config.MQPort;
const vHost = process.env.MQVirtualHost || config.MQVirtualHost;

const chrome = process.env.ChromePath || config.ChromePath;
const errorQueue = process.env.ErrorQueue || config.ErrorQueue;
const updateQueue = process.env.UpdateQueue || config.UpdateQueue;
const matchQueue = process.env.MatchQueue || config.MatchQueue;
const teamQueue = process.env.TeamQueue || config.TeamQueue;
const scrapeQueue = process.env.ScrapeQueue || config.ScrapeQueue;
const logQueue = process.env.LogQueue || config.LogQueue;

(async () => {
    
    try {
        var url = "amqp://" + encodeURIComponent(username)
            + ":" + encodeURIComponent(password)
            + "@" + host
            + ":" + port
            + "/" + vHost;

        console.log(url);

        const conn = await amqp.connect(url);
        const channel = await conn.createChannel();

        await handler.init(chrome, matchQueue, teamQueue, updateQueue, errorQueue, logQueue);

        process.on('exit', async (code) => {
            await channel.close();
            await handler.shutdown();
        });

        await channel.prefetch(config.PrefetchCount, true);
        await channel.consume(scrapeQueue, async function (msg) {
            await handler.onMessageRecieved(channel, msg);
        },
        { noAck: false });
    }
    catch (err) {
        console.log(err);
        process.exit(-1);
    }
})();

