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
    public enum ServiceBusTopicReceiverType
    {
        Standard,
        Performance
    }

    #region ICommunicationListener Impls
    public class ServiceBusTopicListener : ICommunicationListener
    {
        IServiceBusTopicReceiver _sbTopicReceiver;
        readonly string _connectionString = "";
        readonly string _topicName = "";
        readonly string _subscriptionName = "";
        Task _runTask;

        public ServiceBusTopicListener(StatefulServiceContext args, Action<ServiceMessage> messageCallback, Action<Exception> loggerAction, ServiceBusTopicReceiverType receiverType)
        {

            ICodePackageActivationContext codePackageContext = args.CodePackageActivationContext;
            ConfigurationPackage configPackage = codePackageContext.GetConfigurationPackageObject("Config");
            ConfigurationSection configSection = configPackage.Settings.Sections[Constants.SB_CONFIG_SECTION];

            _connectionString = (configSection.Parameters[Constants.SB_CONN_STRING]).Value;
            _topicName = (configSection.Parameters[Constants.SB_TOPIC]).Value;
            _subscriptionName = (configSection.Parameters[Constants.SB_SUBSCRIPTION]).Value;

            if (receiverType == ServiceBusTopicReceiverType.Standard)
            {                
                _sbTopicReceiver = new ServiceBusTopicReceiver(_connectionString, _topicName, _subscriptionName, messageCallback, loggerAction);
            }
            else
            {
                _sbTopicReceiver = new ServiceBusTopicReceiver2(_connectionString, _topicName, _subscriptionName, messageCallback, loggerAction);
            }
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
    
    #endregion


    public interface IServiceBusTopicReceiver : IDisposable
    {
        Task OpenAsync();
        Task CloseAsync();
    }

    public class ServiceBusTopicReceiver : IServiceBusTopicReceiver
    {
        private readonly string _connectionString;
        private readonly string _topicName;
        private readonly string _subscriptionName;

        private readonly Action<ServiceMessage> _callback;
        private readonly Action<Exception> _logger;
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

    public class ServiceBusTopicReceiver2 : IServiceBusTopicReceiver
    {
        private readonly string _connectionString;
        private readonly string _topicName;
        private readonly string _subscriptionName;

        private readonly Action<ServiceMessage> _callback;
        private readonly Action<Exception> _logger;        
        protected MessagingFactory _factory;

        public ServiceBusTopicReceiver2(string connectionString, string topicName, string subscriptionName, Action<ServiceMessage> messageCallback, Action<Exception> loggerAction)
        {
            this._connectionString = connectionString;
            this._topicName = topicName;
            this._subscriptionName = subscriptionName;
            this._callback = messageCallback;
            this._logger = loggerAction;
        }

        public async Task OpenAsync()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(_connectionString);
            var tokenProvider = namespaceManager.Settings.TokenProvider;
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

            var subClient = _factory.CreateSubscriptionClient(_topicName, _subscriptionName);
            subClient.PrefetchCount = 100;
            subClient.OnMessageAsync(async message =>
            {                
                await Task.Run(() =>
                {
                    var messageBody = message.GetBody<string>();
                    var serviceMessage = JsonConvert.DeserializeObject<ServiceMessage>(messageBody);
                    _callback?.Invoke(serviceMessage);
                }).ContinueWith(async t =>
                {
                    await message.CompleteAsync();
                });
                
            });

            await Task.Yield();
        }

        public async Task CloseAsync()
        {
            await _factory.CloseAsync();
        }

        public void Dispose()
        {
            this.CloseAsync().GetAwaiter().GetResult();
        }
    }
}
