using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Common
{
    internal interface IServiceBusTopicSender : IDisposable
    {
        Task Open();
        Task Close();
        Task SendServiceMessageAsync(ServiceMessage message);
    }


    #region Sender Impls
    internal class ServiceBusTopicSender : IServiceBusTopicSender
    {
        private readonly string _connectionString;
        private readonly string _topicName;
        private TopicClient _client;
        private readonly Action<Exception> _logger;

        public ServiceBusTopicSender(string connectionString, string topicName, Action<Exception> loggerAction)
        {
            this._connectionString = connectionString;
            this._topicName = topicName;
            this._logger = loggerAction;
        }

        public Task Open()
        {
            _client = TopicClient.CreateFromConnectionString(_connectionString, _topicName);
            return Task.FromResult(0);
        }

        private async Task SendMessageAsync(BrokeredMessage message)
        {
            try
            {
                if (this._client == null)
                {
                    await this.Open();
                }

                await _client.SendAsync(message);

            }
            catch (Exception e)
            {
                _logger?.Invoke(e);
            }
        }

        public async Task SendServiceMessageAsync(ServiceMessage message)
        {
            var brokerMessage = new BrokeredMessage(JsonConvert.SerializeObject(message));
            await this.SendMessageAsync(brokerMessage);
        }

        public async Task Close()
        {
            if (!_client.IsClosed)
            {
                await _client.CloseAsync();
            }
        }
        public void Dispose()
        {
            this.Close().GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Uses optimization techniques discussed here: https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-performance-improvements
    /// </summary>
    internal class ServiceBusTopicSender2 : IServiceBusTopicSender
    {
        private readonly string _connectionString;
        private readonly string _topicName;
        private readonly Action<Exception> _logger;
        MessagingFactory _factory;
        TopicClient _topicClient;

        public ServiceBusTopicSender2(string connectionString, string topicName, Action<Exception> loggerAction)
        {
            this._connectionString = connectionString;
            this._topicName = topicName;
            this._logger = loggerAction;

        }

        public async Task Open()
        {            
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(_connectionString);
            TokenProvider tokenProvider = namespaceManager.Settings.TokenProvider;

            var topicDescription = new TopicDescription(_topicName);
            topicDescription.EnableExpress = true;
            topicDescription.EnableBatchedOperations = true;

            if (!await namespaceManager.TopicExistsAsync(_topicName))
            {
                topicDescription = namespaceManager.CreateTopic(topicDescription);
            }

            if (!await namespaceManager.SubscriptionExistsAsync(topicDescription.Path, "express"))
            {
                var sd = new SubscriptionDescription(topicDescription.Path, "express");
                await namespaceManager.CreateSubscriptionAsync(sd);
            }

            var settings = new MessagingFactorySettings
            {
                TokenProvider = tokenProvider,
                TransportType = TransportType.Amqp,
                AmqpTransportSettings = new Microsoft.ServiceBus.Messaging.Amqp.AmqpTransportSettings
                {
                    BatchFlushInterval = TimeSpan.FromMilliseconds(50),
                }

            };

            _factory = await MessagingFactory.CreateAsync(namespaceManager.Address, settings);
            _topicClient = _factory.CreateTopicClient(topicDescription.Path);
        }

        private async Task SendMessageAsync(BrokeredMessage message)
        {
            try
            {
                if (this._topicClient == null)
                {
                    await this.Open();
                }

                await _topicClient.SendAsync(message);

            }
            catch (Exception e)
            {
                _logger?.Invoke(e);
            }
        }

        public async Task SendServiceMessageAsync(ServiceMessage message)
        {
            var brokerMessage = new BrokeredMessage(JsonConvert.SerializeObject(message));
            await this.SendMessageAsync(brokerMessage);
        }

        public async Task Close()
        {
            if (_factory.IsClosed)
            {
                await _factory.CloseAsync();
            }
        }

        public void Dispose()
        {
            this.Close().GetAwaiter().GetResult();
        }
    }

    #endregion

    #region Helpers
    public static class ServiceBusSenderClient
    {
        public static async Task Send(string connString, string topic, ServiceMessage message, Action<Exception> loggerAction)
        {                        
            var sbSender = new ServiceBusTopicSender(connString, topic, loggerAction);

            try
            {
                await sbSender.SendServiceMessageAsync(message);
            }
            finally
            {
                await sbSender.Close();
            }
        }
    }
    public static class ServiceBusSenderClient2
    {
        public static async Task Send(string connString, string topic, ServiceMessage message, Action<Exception> loggerAction)
        {
            var sbSender = new ServiceBusTopicSender2(connString, topic, loggerAction);

            try
            {
                await sbSender.SendServiceMessageAsync(message);
            }
            finally
            {
                await sbSender.Close();
            }
        }
    }

    #endregion
}

