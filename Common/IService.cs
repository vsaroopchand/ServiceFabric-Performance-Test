using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

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
