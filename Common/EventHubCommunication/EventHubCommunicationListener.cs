using Common.EventHubCommunication;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Fabric;
using System.Fabric.Description;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public class EventHubCommunicationListener : ICommunicationListener
    {
        string _ehConnString;
        string _storageConnString;
        string _ehPath;
        string _ehConsumerGroup;
        private EventProcessorClient _client;
        private readonly Action<string> _messageCallback;
        private readonly Action<Exception> _loggerAction;

#if !Console

        public EventHubCommunicationListener(ServiceContext args, Action<string> messageCallback, Action<Exception> loggerAction)
        {
            ICodePackageActivationContext codePackageContext = args.CodePackageActivationContext;
            ConfigurationPackage configPackage = codePackageContext.GetConfigurationPackageObject("Config");
            ConfigurationSection configSection = configPackage.Settings.Sections[Constants.EH_CONFIG_SECTION];

            _ehConnString = (configSection.Parameters[Constants.EH_CONN_STRING]).Value;
            _ehPath = (configSection.Parameters[Constants.EH_HUB_PATH]).Value;
            _ehConsumerGroup = (configSection.Parameters[Constants.EH_CONSUMER_GROUP]).Value;
            _storageConnString = (configSection.Parameters[Constants.EH_STORAGE_CONN_STRING]).Value;
#else
        public EventHubCommunicationListener(Action<string> messageCallback, Action<Exception> loggerAction)
        {
             _ehConnString = (ConfigurationManager.AppSettings[Constants.EH_CONN_STRING]);
            _ehPath = (ConfigurationManager.AppSettings[Constants.EH_HUB_PATH]);
            _ehConsumerGroup = (ConfigurationManager.AppSettings[Constants.EH_CONSUMER_GROUP]);
            _storageConnString = (ConfigurationManager.AppSettings[Constants.EH_STORAGE_CONN_STRING]);
#endif

            _messageCallback = messageCallback;
            _loggerAction = loggerAction;
        }
        public void Abort()
        {
            _client?.Stop();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            _client?.Stop();
            return Task.FromResult(0);
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            try
            {
                var nsm = NamespaceManager.CreateFromConnectionString(_ehConnString);
                EventHubDescription desc = new EventHubDescription(_ehPath);
                await nsm.CreateEventHubIfNotExistsAsync(desc);

                _client = new EventProcessorClient(new EventprocessorClientOptions
                {
                    Host = Environment.MachineName + " " + Guid.NewGuid().ToString(),
                    ConnString = _ehConnString,
                    HubPath = _ehPath,
                    StorageConnString = _storageConnString,
                    OnMessageAction = (message) =>
                    {
                        this._messageCallback?.Invoke(message);
                    },
                    OnError = (error) =>
                    {
                        this._loggerAction?.Invoke(error);
                    }
                });
                _client.Start();
                //return Task.FromResult($"EventHub at {this._ehPath}");
                return $"EventHub at {this._ehPath}";
            }
            catch(Exception e)
            {
                this._loggerAction?.Invoke(e);
                return $"Not listening on {this._ehPath}";
            }            
        }
    }
}
