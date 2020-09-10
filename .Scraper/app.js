'use strict';
var fs = require('fs');
var amqp = require('amqplib');
var handler = require('./message-handler');
var config = JSON.parse(fs.readFileSync('./config.json', 'UTF-8'));

const username = process.env.MQUsername || config.MQUsername;
const password = process.env.MQPassword || config.MQPassword;
const host = process.env.MQHost || config.MQHost;
const port = process.env.MQPort || config.MQPort;
const vHost = process.env.MQVirtualHost || config.MQVirtualHost;

const chrome = process.env.ChromePath || config.ChromePath;
const queueIn = process.env.InboundQueue || config.InboundQueue;
const queueOut = process.env.InboundQueue || config.OutboundQueue;
const queueError = process.env.ErrorQueue || config.ErrorQueue;

//var amqp = require('amqplib/callback_api');
//process.env.CLOUDAMQP_URL = 'amqp://localhost';

//// if the connection is closed or fails to be established at all, we will reconnect
//var amqpConn = null;
//function start() {
//    amqp.connect(process.env.CLOUDAMQP_URL + "?heartbeat=60", function (err, conn) {
//        if (err) {
//            console.error("[AMQP]", err.message);
//            return setTimeout(start, 7000);
//        }
//        conn.on("error", function (err) {
//            if (err.message !== "Connection closing") {
//                console.error("[AMQP] conn error", err.message);
//            }
//        });
//        conn.on("close", function () {
//            console.error("[AMQP] reconnecting");
//            return setTimeout(start, 7000);
//        });

//        console.log("[AMQP] connected");
//        amqpConn = conn;

//        whenConnected();
//    });
//}

//function whenConnected() {
//    amqpConn.createChannel(function (err, ch) {
//        if (closeOnErr(err)) return;
//        ch.on("error", function (err) {
//            console.error("[AMQP] channel error", err.message);
//        });
//        ch.on("close", function () {
//            console.log("[AMQP] channel closed");
//        });
//        ch.prefetch(10);
//        ch.assertQueue("jobs", { durable: true }, function (err, _ok) {
//            if (closeOnErr(err)) return;
//            ch.consume("jobs", processMsg, { noAck: false });
//            console.log("Worker is started");
//        });

//        function processMsg(msg) {

//        }
//    });
//}

//function closeOnErr(err) {
//    if (!err) return false;
//    console.error("[AMQP] error", err);
//    amqpConn.close();
//    return true;
//}




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

        await handler.init(chrome, queueOut, queueError);

        process.on('exit', async (code) => {
            await channel.close();
            await handler.shutdown();
        });

        await channel.prefetch(config.PrefetchCount, true);
        await channel.consume(queueIn, async function (msg) {
            await handler.onMessageRecieved(channel, msg);
        },
        { noAck: false });
    }
    catch (err) {
        console.log(err);
        process.exit(-1);
    }
})();

