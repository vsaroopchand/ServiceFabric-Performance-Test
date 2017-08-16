using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;

namespace Common
{

    public class SvcTwoWcfCommunicationClient : ServicePartitionClient<WcfCommunicationClient<IServiceTwo>>
    {
        public SvcTwoWcfCommunicationClient(ICommunicationClientFactory<WcfCommunicationClient<IServiceTwo>> communicationClientFactory, Uri serviceUri, ServicePartitionKey partitionKey = null, TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default, string listenerName = null, OperationRetrySettings retrySettings = null)
            : base(communicationClientFactory, serviceUri, partitionKey, targetReplicaSelector, listenerName, retrySettings)
        {

        }
    }

    public class SvcThreeWcfCommunicationClient: ServicePartitionClient<WcfCommunicationClient<IServiceThree>>        
    {
        public SvcThreeWcfCommunicationClient(ICommunicationClientFactory<WcfCommunicationClient<IServiceThree>> communicationClientFactory, Uri serviceUri, ServicePartitionKey partitionKey = null, TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default, string listenerName = null, OperationRetrySettings retrySettings = null)
            : base(communicationClientFactory, serviceUri, partitionKey, targetReplicaSelector, listenerName, retrySettings)
        {

        }
    }

    public class SvcFourWcfCommunicationClient : ServicePartitionClient<WcfCommunicationClient<IServiceFour>>
    {
        public SvcFourWcfCommunicationClient(ICommunicationClientFactory<WcfCommunicationClient<IServiceFour>> communicationClientFactory, Uri serviceUri, ServicePartitionKey partitionKey = null, TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default, string listenerName = null, OperationRetrySettings retrySettings = null)
            : base(communicationClientFactory, serviceUri, partitionKey, targetReplicaSelector, listenerName, retrySettings)
        {

        }
    }
}
