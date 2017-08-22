using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Newtonsoft.Json;
using System;
using System.Fabric;
using System.Fabric.Description;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    // https://github.com/vsaroopchand/AzureStorage/blob/master/AzureStorageExplorer/ExploreAzureServiceBus/Explore.cs
    public class ServiceBusTopicListener : ICommunicationListener
    {
        ServiceBusTopicReceiver _sbTopicReceiver;
        readonly string _connectionString = "";
        readonly string _topicName = "";
        readonly string _subscriptionName = "";
        Task _runTask;

        public ServiceBusTopicListener(StatefulServiceContext args, Action<ServiceMessage> messageCallback, Action<Exception> loggerAction)
        {

            ICodePackageActivationContext codePackageContext = args.CodePackageActivationContext;
            ConfigurationPackage configPackage = codePackageContext.GetConfigurationPackageObject("Config");
            ConfigurationSection configSection = configPackage.Settings.Sections[Constants.SB_CONFIG_SECTION];

            _connectionString = (configSection.Parameters[Constants.SB_CONN_STRING]).Value;
            _topicName = (configSection.Parameters[Constants.SB_TOPIC]).Value;
            _subscriptionName = (configSection.Parameters[Constants.SB_SUBSCRIPTION]).Value;

            _sbTopicReceiver = new ServiceBusTopicReceiver(_connectionString, _topicName, _subscriptionName, messageCallback, loggerAction);
        }
        public void Abort()
        {
            _runTask = null;
            _sbTopicReceiver
                .CloseAsync()
                .GetAwaiter();
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            _runTask = null;
            await _sbTopicReceiver.CloseAsync();
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            _runTask = this._sbTopicReceiver.OpenAsync();
            return Task.FromResult(this._connectionString);
        }
    }

    public class ServiceBusTopicReceiver : IDisposable
    {
        private readonly string _connectionString;
        private readonly string _topicName;
        private readonly Action<ServiceMessage> _callback;
        private readonly Action<Exception> _logger;
        private readonly string _subscriptionName = "";
        protected SubscriptionClient _client;

        public ServiceBusTopicReceiver(string connectionString, string topicName, string subscriptionName, Action<ServiceMessage> messageCallback, Action<Exception> loggerAction)
        {
            this._connectionString = connectionString;
            this._topicName = topicName;
            this._subscriptionName = subscriptionName;
            this._callback = messageCallback;
            this._logger = loggerAction;
        }

        public void Dispose()
        {
            if (!_client.IsClosed)
            {
                _client.Close();
            }
        }

        public async Task OpenAsync()
        {
            _client = SubscriptionClient.CreateFromConnectionString(_connectionString, _topicName, _subscriptionName, ReceiveMode.ReceiveAndDelete);
            _client.RetryPolicy = new RetryExponential(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(20), 5);
            _client.OnMessageAsync(async message =>
            {
                await Task.Run(() =>
                {
                    var messageBody = message.GetBody<string>();
                    var serviceMessage = JsonConvert.DeserializeObject<ServiceMessage>(messageBody);
                    _callback?.Invoke(serviceMessage);
                });                                
                
            });

            await Task.Yield();
        }

        public async Task CloseAsync()
        {
            await _client.CloseAsync();
        }
    }

    internal class ServiceBusTopicSender : IDisposable
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

        protected void Open()
        {
            _client = TopicClient.CreateFromConnectionString(_connectionString, _topicName);
        }

        private async Task SendMessageAsync(BrokeredMessage message)
        {
            try
            {
                if(this._client == null)
                {
                    this.Open();
                }

                await _client.SendAsync(message);

            }
            catch(Exception e)
            {
                _logger?.Invoke(e);
            }          
        }

        public async Task SendServiceMessageAsync(ServiceMessage message)
        {            
            var brokerMessage = new BrokeredMessage(JsonConvert.SerializeObject(message));
            await this.SendMessageAsync(brokerMessage);
        }

        public void Close()
        {
            if (!_client.IsClosed)
            {
                _client.Close();
            }
        }
        public void Dispose()
        {
            this.Close();
        }
    }


    public static class ServiceBusSenderClient
    {
        public static void Send(string topic, ServiceMessage message, Action<Exception> loggerAction)
        {

            var connString = @"ENTER YOUR SAS TOKEN";
            var sbSender = new ServiceBusTopicSender(connString, topic, loggerAction);

            try
            {
                sbSender.SendServiceMessageAsync(message).GetAwaiter().GetResult();               
            }         
            finally
            {
                sbSender.Close();
            }
        }    
    }

}
