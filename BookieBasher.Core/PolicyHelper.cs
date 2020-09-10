using Polly;
using RabbitMQ.Client.Exceptions;
using System;
using System.IO;
using System.Net.Sockets;

namespace BookieBasher.Core
{
    public static class PolicyHelper
    {
        private static Func<int, TimeSpan> sleepProvider = (int retryAttempt) =>
        {
            return retryAttempt <= 5 ? TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                     : TimeSpan.FromSeconds(30);
        };

        private static Policy policy = Policy.Handle<SocketException>()
                                           .Or<BrokerUnreachableException>()
                                           .Or<IOException>()
                                           .WaitAndRetryForever(sleepProvider);

        public static void ApplyPolicy(Action action)
        {
            policy.Execute(action);
        }

        public static T ApplyPolicy<T>(Func<T> action)
        {
            return policy.Execute(action);
        }
    }
}
