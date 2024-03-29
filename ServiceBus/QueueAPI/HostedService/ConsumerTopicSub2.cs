﻿using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueAPI.HostedService
{
    public class ConsumerTopicSub2 : IHostedService
    {
        static SubscriptionClient subscriptionClient;
        private readonly IConfiguration _config;

        public ConsumerTopicSub2(IConfiguration config)
        {
            _config = config;
            var serviceBusConnection = _config.GetValue<string>("ServiceBus:ConnectionString");
            subscriptionClient = new SubscriptionClient(serviceBusConnection, "team-topic", "team-sub2");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("############## INICIANDO CONSUMER SUBSCRIPTION 2 ####################");
            ProcessMessageHandler();
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("############## DESLIGANDO CONSUMER DO TOPICO ####################");
            await subscriptionClient.CloseAsync();
            await Task.CompletedTask;
        }

        public void ProcessMessageHandler()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        public async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            Console.WriteLine("### PROCESSANDO MENSAGEM SUBSCRIPTION 2 ###");
            Console.WriteLine($"{DateTime.Now}");
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        public Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
