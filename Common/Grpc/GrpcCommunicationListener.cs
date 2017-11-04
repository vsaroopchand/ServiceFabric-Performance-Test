using Grpc.Core;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Grpc
{
    public class GrpcCommunicationListener : ICommunicationListener
    {

        private readonly IEnumerable<ServerServiceDefinition> _services;
        private readonly ServiceContext _serviceContext;
        private readonly string _endpointName;
        private readonly Action<string> _loggerAction;
        private Server _server;

        public GrpcCommunicationListener(
          ServiceContext serviceContext,
          IEnumerable<ServerServiceDefinition> services,
          Action<string> loggerAction,
          string endpointName)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceContext == null)
            {
                throw new ArgumentNullException(nameof(serviceContext));
            }

            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            _services = services;
            _serviceContext = serviceContext;
            _endpointName = endpointName;
            _loggerAction = loggerAction;
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var serviceEndpoint = _serviceContext.CodePackageActivationContext.GetEndpoint(_endpointName);
            var port = serviceEndpoint.Port;
            var host = FabricRuntime.GetNodeContext().IPAddressOrFQDN;

            try
            {
                _loggerAction.Invoke($"Starting gRPC server on http://{host}:{port}");

                _server = new Server
                {
                    Ports = { new ServerPort(host, port, ServerCredentials.Insecure) }
                };
                foreach (var service in _services)
                {
                    _server.Services.Add(service);
                }

                _server.Start();

                _loggerAction.Invoke($"Listening on http://{host}:{port}");

                return $"http://{host}:{port}";
            }
            catch (Exception ex)
            {
                _loggerAction.Invoke($"gRPC server failed to open with {ex.Message}" );

                await StopServerAsync();

                throw;
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            _loggerAction?.Invoke("Closing gRPC server");

            return StopServerAsync();
        }

        public void Abort()
        {
            _loggerAction?.Invoke("Aborting gRPC server");

            StopServerAsync().Wait();
        }

        private async Task StopServerAsync()
        {
            if (_server != null)
            {
                try
                {
                    await _server.ShutdownAsync();
                }
                catch (Exception e)
                {
                    this._loggerAction(e.Message);
                }
            }
        }
    }
}
