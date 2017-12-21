using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using System.ServiceModel;
using System.Threading.Tasks;

// to fix issue described here: https://blogs.msdn.microsoft.com/azureservicefabric/2017/10/23/serializationexception-with-new-reliable-actor-projects-in-visual-studio-2017/
[assembly: FabricTransportServiceRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]
namespace Common
{

    [ServiceContract]
    public interface IServiceOperation: IService
    {
        [OperationContract]
        Task VisitByRemotingAsync(ServiceMessage message);

        [OperationContract]
        Task VisitWcfAsync(ServiceMessage message);
    }

    public interface IWebProxyService : IService
    {
        [OperationContract]
        Task VisitByRemotingAsync(ServiceMessage message);
    }

    public interface IServiceTwo : IServiceOperation
    {
    }

    public interface IServiceThree : IServiceOperation
    {
    }

    public interface IServiceFour : IServiceOperation
    {
    }
}
