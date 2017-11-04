using Grpc.Core;
using System;
using System.Fabric;
using System.Threading.Tasks;

namespace Common.Grpc
{
    public class GrpcMessageServiceImpl : GrpcMessageService.GrpcMessageServiceBase   {
        private readonly ServiceContext _ctx;
        private readonly Action<ServiceMessage2> _messageHandle;

        public GrpcMessageServiceImpl(ServiceContext ctx, Action<ServiceMessage2> messageHandle)
        {
            _ctx = ctx;
            _messageHandle = messageHandle;
        }

        public override Task<Noop> Send(ServiceMessage2 request, ServerCallContext context)
        {           
            this._messageHandle?.Invoke(request);
            return Task.FromResult(new Noop { });
        }
    }
}
